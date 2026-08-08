using System;
using System.Collections.Generic;
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

            internal Func<string> GetNextText;
            internal Func<List<Ink.Runtime.Choice>> GetChoices;
            internal Action<int> SetChoice;

            internal Func<string, UniTask> ShowNotification;

            internal Func<bool, string, bool, bool, string[], UniTask> SetImage;
            internal Func<bool, string, UniTask> SetCamera;
            internal Func<bool, TextAlignment, UniTask> SetDialogue;

            internal Func<float, UniTask> Wait;

            internal Func<string, Audio.Entity.Audio, UniTask> PlayAudio;

            internal Func<string, string> GetLocalizationValue;

            internal Bubble.Entity Bubble;

            internal Save.Entity SaveSystem;

            internal Action<string> SetMainCharacterView;
            internal Action<string> SetMainCharacterClothes;
            internal Action<string> SetMainCharacterHair;
            internal Func<UniTask> CharacterHide;
            internal Action CharacterHideImmediate;
            internal Func<bool, UniTask> CharacterShow;
            internal Action<bool> CharacterShowImmediate;
            internal Func<string, string[], UniTask> CharacterSetImage;


            internal Func<UniTask> ShowLoading;
            internal Func<UniTask> HideLoading;

            public Action<(LogType type, string message)> OnLog;
        }

        private interface IQueue { }
        private struct NotificationQueue : IQueue
        {
            public string NotificationText;
        }
        private struct MusicQueue : IQueue
        {
            public string AssetName;
            public string[] Args;
        }
        private struct SoundQueue : IQueue
        {
            public string AssetName;
            public string[] Args;
        }
        private struct AmbientQueue : IQueue
        {
            public string AssetName;
            public string[] Args;
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

        private Ctx _ctx;

        internal NovelProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask ShowNovelProcess()
        {
            var queue = new Queue<IQueue>();
            var lastCharacterName = string.Empty;

            await _ctx.HideLoading();

            while (!IsDisposed)
            {
                var bubbleDone = new UniTaskCompletionSource();

                var text = _ctx.GetNextText();
                if (string.IsNullOrEmpty(text) && _ctx.GetChoices().Count == 0) continue;

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
                    continue;
                }
                if (prefix.ToLower().Contains("cut-scene"))
                {
                    queue.Enqueue(new CutSceneQueue{
                        AssetName = value,
                        Args = args
                    });
                    continue;
                }
                if (prefix.ToLower().Contains("music"))
                {
                    queue.Enqueue(new MusicQueue
                    {
                        AssetName = value,
                        Args = args
                    });
                    continue;
                }
                if (prefix.ToLower().Contains("sound"))
                {
                    queue.Enqueue(new SoundQueue
                    {
                        AssetName = value,
                        Args = args
                    });
                    continue;
                }
                if (prefix.ToLower().Contains("ambient"))
                {
                    queue.Enqueue(new AmbientQueue
                    {
                        AssetName = value,
                        Args = args
                    });
                    continue;
                }
                if (prefix.ToLower() == "camera")
                {
                    queue.Enqueue(new CameraQueue
                    {
                        Value = value
                    });
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

                if (name == "some wardrobe trigger")
                {
                    // set wardrobe screen here...
                }
                else if (name == "some choose trigger")
                {
                    //set choose screen here...
                }
                else
                {
                    _ctx.Bubble.SetBubbleScreen(new Bubble.Entity.BubbleScreenCtx
                    {
                        Name = name,
                        Args = args,
                        Text = new Bubble.Entity.BubbleScreenCtx.TextCtx
                        {
                            Header = _ctx.GetLocalizationValue(name),
                            Text = value
                        },
                        Buttons = _ctx.GetChoices().Select(c => new Bubble.Entity.BubbleScreenCtx.ButtonCtx
                        {
                            Id = c.index,
                            Text = c.text,
                            OnClick = id =>
                            {
                                SetCharacterView(args, c);
                                _ctx.SaveSystem.TrySaveChoice((byte)id);
                                _ctx.SetChoice(id);
                                bubbleDone.TrySetResult();
                            }
                        }).ToArray(),
                        OnBackgroundClick = () =>
                        {
                            _ctx.SaveSystem.TrySaveChoice();
                            bubbleDone.TrySetResult();
                        }
                    });
                }

                if (_ctx.SaveSystem.TryLoadChoice(out var savedChoice))
                {
                    if (savedChoice != 255)
                    {
                        SetCharacterView(args, _ctx.GetChoices()[savedChoice]);
                        _ctx.SetChoice(savedChoice);
                    }
                    bubbleDone.TrySetResult();
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
                    if (!_ctx.SaveSystem.IsLoadingInProcess)
                        await _ctx.CharacterHide();
                    else
                        _ctx.CharacterHideImmediate();
                }

                while(queue.TryDequeue(out var element))
                {
                    switch (element)
                    {
                        case NotificationQueue notificationQueue:
                            if (!_ctx.SaveSystem.IsLoadingInProcess)
                                _ctx.ShowNotification(notificationQueue.NotificationText).Forget()  ;
                        break;
                        case LocationQueue locationQueue:
                            await _ctx.SetImage(_ctx.SaveSystem.IsLoadingInProcess, locationQueue.AssetName, false, false, locationQueue.Args);
                        break;
                        case CutSceneQueue cutSceneQueue:
                            await _ctx.SetImage(_ctx.SaveSystem.IsLoadingInProcess, cutSceneQueue.AssetName, true, false, cutSceneQueue.Args);
                        break;
                        case MusicQueue musicQueue:
                            await _ctx.PlayAudio(musicQueue.AssetName, Audio.Entity.Audio.Music);
                        break;
                        case SoundQueue soundQueue:
                            await _ctx.PlayAudio(soundQueue.AssetName, Audio.Entity.Audio.Sound);
                        break;
                        case AmbientQueue ambientQueue:
                            await _ctx.PlayAudio(ambientQueue.AssetName, Audio.Entity.Audio.Ambient);
                        break;
                        case CameraQueue cameraQueue:
                            await _ctx.SetCamera(_ctx.SaveSystem.IsLoadingInProcess, cameraQueue.Value);
                        break;
                        case AwaitQueue awaitQueue:
                            if (!_ctx.SaveSystem.IsLoadingInProcess)
                                await _ctx.Wait(awaitQueue.Timer);
                        break;
                        case DialogQueue dialogQueue:
                            await _ctx.SetDialogue(_ctx.SaveSystem.IsLoadingInProcess, dialogQueue.DialogAlign);
                        break;
                    }
                }

                await _ctx.CharacterSetImage(name, args);
                if (isNewCharacter)
                {
                    if (!_ctx.SaveSystem.IsLoadingInProcess)
                        await _ctx.CharacterShow(name == _ctx.MainCharacter);
                    else
                        _ctx.CharacterShowImmediate(name == _ctx.MainCharacter);
                }

                if (!_ctx.SaveSystem.IsLoadingInProcess)
                    await _ctx.Bubble.Show();
                else
                    _ctx.Bubble.ShowImmediate();

                await bubbleDone.Task;

                //ResetContent
                queue.Clear();
                if (!_ctx.SaveSystem.IsLoadingInProcess)
                    await _ctx.Bubble.Hide();
                else
                    _ctx.Bubble.HideImmediate();
            }
        }

        private void SetCharacterView(string[] args, Ink.Runtime.Choice choice)
        {
            if (args.Any(a => a == "Выбери внешность"))
                _ctx.SetMainCharacterView(choice.text);
            if (args.Any(a => a == "Выбери одежду"))
                _ctx.SetMainCharacterClothes(choice.text);
            if (args.Any(a => a == "Выбери прическу" || a == "Выбери причёску"))
                _ctx.SetMainCharacterHair(choice.text);
        }
    }
}

