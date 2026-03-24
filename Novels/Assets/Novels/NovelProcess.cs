using System;
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

            string notificationData = null;
            (string assetName, bool cutScene, string[] args)? locationData = null;
            (string assetName, bool cutScene, string[] args)? cutSceneData = null;
            string cameraData = null;
            float? awaitData = null;
            TextAlignment? dialogData = null;

            var lastCharacterName = string.Empty;

            while (!IsDisposed)
            {
                var bubbleDone = new UniTaskCompletionSource();

                _ctx.StoryProcessor.TryGetNextText(out var text);
                if (string.IsNullOrEmpty(text) && _ctx.StoryProcessor.GetChoices().Count == 0) continue;

                var data = text.Split(":");
                var prefix = data.FirstOrDefault().Trim();
                var value = data.LastOrDefault().Trim();

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
                    notificationData = value;
                    continue;
                }

                if (prefix.ToLower().Contains("location"))
                {
                    var locationRawArgsData = prefix.Split("(");
                    var locationArgs = locationRawArgsData.Length <= 1
                    ? new string[0]
                    : locationRawArgsData.LastOrDefault().Split(")").FirstOrDefault().Split(",").Select(a => a.Trim()).ToArray();
                    locationData = (value, false, locationArgs);
                    continue;
                }
                if (prefix.ToLower() == "cut-scene")
                {
                    cutSceneData = (value, true, null);
                    continue;
                }
                if (prefix.ToLower() == "camera")
                {
                    cameraData = value;
                    continue;
                }
                if (prefix.ToLower() == "await")
                {
                    if (int.TryParse(value, out var seconds))
                        awaitData = seconds;
                    continue;
                }

                var rawPrefixData = prefix.Split("(");
                var name = rawPrefixData.FirstOrDefault().Trim();
                var args = rawPrefixData.Length <= 1
                    ? new string[0]
                    : rawPrefixData.LastOrDefault().Split(")").FirstOrDefault().Split(",").Select(a => a.Trim()).ToArray();

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
                dialogData = dialogAlign;

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

                if (!_ctx.SaveSystem.IsLoadingInProcess && !loadingDone)
                {
                    loadingDone = true;
                    await _ctx.HideLoading();
                }

                //ShowContent

                var isNewCharacter = false;
                if (lastCharacterName != name)
                {
                    isNewCharacter = true;
                    lastCharacterName = name;
                    if (_ctx.SaveSystem.IsLoadingInProcess)
                        _ctx.Character.HideImmediate();
                    else
                        await _ctx.Character.Hide();
                }
                

                if (notificationData != null && !_ctx.SaveSystem.IsLoadingInProcess)
                {
                    _ctx.Notification.Show(notificationData).Forget();
                    notificationData = null;
                }
                if (locationData.HasValue)
                {
                    if (_ctx.SaveSystem.IsLoadingInProcess)
                        await _ctx.Location.SetImageImmediate(locationData.Value.assetName, locationData.Value.cutScene, locationData.Value.args);
                    else
                        await _ctx.Location.SetImage(locationData.Value.assetName, locationData.Value.cutScene, locationData.Value.args);
                    locationData = null;
                }
                if (cutSceneData.HasValue)
                {
                    if (_ctx.SaveSystem.IsLoadingInProcess)
                        await _ctx.Location.SetImageImmediate(cutSceneData.Value.assetName, cutSceneData.Value.cutScene, cutSceneData.Value.args);
                    else
                        await _ctx.Location.SetImage(cutSceneData.Value.assetName, cutSceneData.Value.cutScene, cutSceneData.Value.args);
                    cutSceneData = null;
                }
                if (cameraData != null)
                {
                    if (_ctx.SaveSystem.IsLoadingInProcess)
                        _ctx.Location.SetCameraImmediate(cameraData);
                    else
                        await _ctx.Location.SetCamera(cameraData);
                    cameraData = null;
                }
                if (awaitData.HasValue && !_ctx.SaveSystem.IsLoadingInProcess)
                {
                    await _ctx.Waiting.Await(awaitData.Value);
                    awaitData = null;
                }
                if (dialogData.HasValue)
                {
                    if (_ctx.SaveSystem.IsLoadingInProcess)
                        _ctx.Location.SetDialogImmediate(dialogData.Value);
                    else
                        await _ctx.Location.SetDialog(dialogData.Value);
                    dialogData = null;
                }

                _ctx.Character.SetImage(name, args);
                if (isNewCharacter)
                {
                    if (_ctx.SaveSystem.IsLoadingInProcess)
                        _ctx.Character.ShowImmediate();
                    else
                        await _ctx.Character.Show(name == _ctx.MainCharacter);
                }

                if (_ctx.SaveSystem.IsLoadingInProcess)
                    _ctx.Bubble.ShowImmediate();
                else
                    await _ctx.Bubble.Show();

                if (!_ctx.SaveSystem.TryLoad(out var result))
                {
                    await bubbleDone.Task;
                }
                else
                {
                    await UniTask.Yield();
                    if (result != 255)
                    {
                        SetCharacterView(_ctx.Character, args, _ctx.StoryProcessor.GetChoices()[result]);
                        _ctx.StoryProcessor.SetChoice(result);
                    }
                }

                //ResetContent
                if (_ctx.SaveSystem.IsLoadingInProcess)
                    _ctx.Bubble.HideImmediate();
                else
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

