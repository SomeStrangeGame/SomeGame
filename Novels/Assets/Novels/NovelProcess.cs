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

        private Ctx _ctx;

        internal NovelProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask ShowNovelProcess()
        {
            var loadingDone = false;

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
                    _ctx.Notification.Show(_ctx.SaveSystem.IsLoadingInProcess, value).Forget();
                    continue;
                }

                if (prefix.ToLower().Contains("location"))
                {
                    var locationRawArgsData = prefix.Split("(");
                    var locationArgs = locationRawArgsData.Length <= 1
                    ? new string[0]
                    : locationRawArgsData.LastOrDefault().Split(")").FirstOrDefault().Split(",").Select(a => a.Trim()).ToArray();

                    await _ctx.Location.SetImage(_ctx.SaveSystem.IsLoadingInProcess, value, false, locationArgs);
                    continue;
                }
                if (prefix.ToLower() == "cut-scene")
                {
                    await _ctx.Location.SetImage(_ctx.SaveSystem.IsLoadingInProcess, value, true, null);
                    continue;
                }
                if (prefix.ToLower() == "camera")
                {
                    await _ctx.Location.SetCamera(_ctx.SaveSystem.IsLoadingInProcess, value);
                    continue;
                }
                if (prefix.ToLower() == "await")
                {
                    if (int.TryParse(value, out var seconds))
                        await _ctx.Waiting.Await(_ctx.SaveSystem.IsLoadingInProcess, seconds);
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

                await _ctx.Location.SetDialog(_ctx.SaveSystem.IsLoadingInProcess, dialogAlign);

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

                //show content
                var showProcess = UniTask.WhenAll(
                    _ctx.Character.SetImageAndShow(_ctx.SaveSystem.IsLoadingInProcess, name, args),
                    _ctx.Bubble.Show(_ctx.SaveSystem.IsLoadingInProcess)
                );
                await showProcess;

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

                //reset content
                var resetProcess = UniTask.WhenAll(
                    _ctx.Character.Hide(_ctx.SaveSystem.IsLoadingInProcess),
                    _ctx.Bubble.Hide(_ctx.SaveSystem.IsLoadingInProcess)
                );
                await resetProcess;
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

