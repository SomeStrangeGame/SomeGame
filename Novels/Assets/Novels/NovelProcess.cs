using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal class NovelProcess : BaseDisposable
    {
        internal struct Ctx
        {
            internal string MainCharacter;

            internal StoryProcessor.Entity StoryProcessor;
            internal Notification.Entity Notification;
            internal Location.Entity Location;
            internal Waiting.Entity Waiting;
            internal Localization.Entity Localization;
            internal Bubble.Entity Bubble;
            internal Save.Entity SaveSystem;
            internal Character.Entity Character;

            internal Func<UniTask> ShowLoading;
            internal Func<UniTask> HideLoading;

            public Action<(LogType type, string message)> OnLog;
        }

        private interface IQueue { }
        private struct NotificationQueue : IQueue
        {
            public string NotificationText;
        }
        private struct LocationQueue : IQueue
        {
            public string AssetName;
            public string[] Args;
        }
        private struct CutSceneQueue : IQueue
        {
            public string AssetName;
            public string[] Args;
        }
        private struct CameraQueue : IQueue
        {
            public string Value;
        }
        private struct AwaitQueue : IQueue
        {
            public float Timer;
        }
        private struct DialogQueue : IQueue
        {
            public TextAlignment DialogAlign;
        }

        private const string _mainCharacter = "MainCharacter";
        private const string _wardrobe = "Wardrobe";

        private Ctx _ctx;

        internal NovelProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask ShowNovelProcess()
        {
            var loadingDone = false;
            var queue = new Queue<IQueue>();
            var lastCharacterName = string.Empty;

            LocationQueue? lastLocation = null;
            CutSceneQueue? lastCutScene = null;
            CameraQueue? lastCamera = null;
            DialogQueue? lastDialog = null;

            while (!IsDisposed)
            {
                var bubbleDone = new UniTaskCompletionSource();

                _ctx.StoryProcessor.TryGetNextText(out var text);
                if (string.IsNullOrEmpty(text) && _ctx.StoryProcessor.GetChoices().Count == 0) continue;

                var data = text.Split(":");
                var prefix = data.FirstOrDefault().Trim();
                var value = data.LastOrDefault().Trim();

                var rawPrefixData = prefix.Split("(");
                var name = rawPrefixData.FirstOrDefault().Trim();
                var args = rawPrefixData.Length <= 1
                    ? new string[0]
                    : rawPrefixData.LastOrDefault().Split(")").FirstOrDefault().Split(",").Select(a => a.Trim()).ToArray();

                if (prefix.ToLower() == "title") continue;
                if (prefix.ToLower() == "series") continue;
                if (prefix.ToLower() == "genres") continue;
                if (prefix.ToLower() == "annotation") continue;
                if (prefix.ToLower() == "stats") continue;

                if (prefix.ToLower().Contains("keyboard")) continue;

                if (prefix.ToLower() == "music") continue;
                if (prefix.ToLower() == "sound") continue;
                if (prefix.ToLower() == "ambient") continue;

                if (prefix.ToLower() == "notification")
                {
                    queue.Enqueue(new NotificationQueue
                    {
                        NotificationText = value
                    });
                    continue;
                }

                if (prefix.ToLower().Contains("location"))
                {
                    queue.Enqueue(new LocationQueue
                    {
                        AssetName = value,
                        Args = args
                    });
                    lastLocation = new LocationQueue
                    {
                        AssetName = value,
                        Args = args
                    };
                    lastCutScene = null;
                    lastCamera = null;
                    continue;
                }
                if (prefix.ToLower().Contains("cut-scene"))
                {
                    queue.Enqueue(new CutSceneQueue{
                        AssetName = value,
                        Args = args
                    });
                    lastCutScene = new CutSceneQueue
                    {
                        AssetName = value,
                        Args = args
                    };
                    lastLocation = null;
                    lastCamera = null;
                    continue;
                }
                if (prefix.ToLower() == "camera")
                {
                    queue.Enqueue(new CameraQueue
                    {
                        Value = value
                    });
                    lastCamera = new CameraQueue
                    {
                        Value = value
                    };
                    lastLocation = null;
                    lastCutScene = null;
                    continue;
                }
                if (prefix.ToLower() == "await")
                {
                    if (int.TryParse(value, out var seconds))
                        queue.Enqueue(new AwaitQueue
                        {
                            Timer = seconds
                        });
                    continue;
                }

                var characterName = string.Empty;
                if (!_ctx.Localization.TryGetValue(name, out characterName))
                    _ctx.OnLog.Invoke((LogType.Warning, $"No localized character name [{name}]"));

                TextAlignment dialogAlign;
                if (name == _ctx.MainCharacter)
                    dialogAlign = TextAlignment.Left;
                else if (name == "..." || name == "Wardrobe")
                    dialogAlign = TextAlignment.Center;
                else
                    dialogAlign = TextAlignment.Right;
                queue.Enqueue(new DialogQueue
                {
                    DialogAlign = dialogAlign
                });
                lastDialog = new DialogQueue
                {
                    DialogAlign = dialogAlign
                };

                _ctx.Bubble.SetText(name, characterName, value, args);
                _ctx.Bubble.RemoveAllButtons();
                var choices = _ctx.StoryProcessor.GetChoices();
                if (choices.Count > 0)
                    _ctx.Bubble.ResetBackgroundButton();
                else
                    _ctx.Bubble.SetBackgroundButton(() => 
                    {
                        _ctx.SaveSystem.TrySave();
                        bubbleDone.TrySetResult();
                    });
                foreach (var choice in choices)
                {
                    var choiceText = choice.text;
                    if (!_ctx.Localization.TryGetValue(choice.text, out choiceText))
                        _ctx.OnLog.Invoke((LogType.Warning, $"No localized choice [{choice.text}]"));
                    _ctx.Bubble.AddOrUpdateButton(choice.index, name, choiceText, args, id =>
                    {
                        SetCharacterView(_ctx.Character, args, choice);

                        _ctx.SaveSystem.TrySave((byte)id);
                        _ctx.StoryProcessor.SetChoice(id);
                        bubbleDone.TrySetResult();
                    });
                }

                if (_ctx.SaveSystem.TryLoad(out var savedChoice))
                {
                    if (savedChoice != 255)
                    {
                        SetCharacterView(_ctx.Character, args, _ctx.StoryProcessor.GetChoices()[savedChoice]);
                        _ctx.StoryProcessor.SetChoice(savedChoice);
                    }
                    continue;
                }

                if (!loadingDone)
                {
                    queue.Clear();
                    if (lastLocation.HasValue)
                        queue.Enqueue(lastLocation.Value);
                    if (lastCutScene.HasValue)
                        queue.Enqueue(lastCutScene.Value);
                    if (lastCamera.HasValue)
                        queue.Enqueue(lastCamera.Value);
                    if (lastDialog.HasValue)
                        queue.Enqueue(lastDialog.Value);

                    loadingDone = true;
                    await _ctx.HideLoading();
                }

                //ShowContent

                var characterNameTemp = $"{name}";
                if (args.Any(a => a.ToLower() == "маленькая"))
                    characterNameTemp += "_child";
                var isNewCharacter = false;
                if (lastCharacterName != characterNameTemp)
                {
                    isNewCharacter = true;
                    lastCharacterName = characterNameTemp;
                    await _ctx.Character.Hide();
                }

                while(queue.TryDequeue(out var element))
                {
                    switch (element)
                    {
                        case NotificationQueue notificationQueue:
                            _ctx.Notification.Show(notificationQueue.NotificationText).Forget();
                        break;
                        case LocationQueue locationQueue:
                            await _ctx.Location.SetImage(locationQueue.AssetName, false, locationQueue.Args);
                        break;
                        case CutSceneQueue cutSceneQueue:
                            await _ctx.Location.SetImage(cutSceneQueue.AssetName, true, cutSceneQueue.Args);
                        break;
                        case CameraQueue cameraQueue:
                            await _ctx.Location.SetCamera(cameraQueue.Value);
                        break;
                        case AwaitQueue awaitQueue:
                            await _ctx.Waiting.Await(awaitQueue.Timer);
                        break;
                        case DialogQueue dialogQueue:
                            await _ctx.Location.SetDialog(dialogQueue.DialogAlign);
                        break;
                    }
                }

                await _ctx.Character.SetImage(name, args);
                if (isNewCharacter)
                {
                    await _ctx.Character.Show(name == _ctx.MainCharacter);
                }

                await _ctx.Bubble.Show();

                await bubbleDone.Task;

                //ResetContent
                queue.Clear();
                await _ctx.Bubble.Hide();
            }
        }

        private void SetCharacterView(Character.Entity character, string[] args, Ink.Runtime.Choice choice)
        {
            if (args.Any(a => a == "Выбери внешность"))
                character.SetMainCharacterView(choice.text);
            if (args.Any(a => a == "Выбери одежду"))
                character.SetMainCharacterClothes(choice.text);
            if (args.Any(a => a == "Выбери прическу" || a == "Выбери причёску"))
                character.SetMainCharacterHair(choice.text);
        }
    }
}

