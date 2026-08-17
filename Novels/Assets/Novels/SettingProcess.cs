using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal enum SettingSelection
    {
        NewGame,
        ContinueGame,
    }

    internal class SettingProcess : BaseDisposable
    {
        private const string _newGameButtonId = "newGame";
        private const string _continueGameButtonId = "continueGame";

        internal struct Ctx
        {
            internal GameObject BundledPrefab;
            internal Func<UniTask> ShowLoading;
            internal Func<UniTask> HideLoading;
            internal Func<bool> ContainAnySave;
            internal string NovelTitle;
            internal Func<string, string> GetLocalizationValue;
            internal CancellationToken CancellationToken;
        }

        private Ctx _ctx;

        internal SettingProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask<SettingSelection> ShowSettingProcess()
        {
            var setting = new Setting.Entity(new Setting.Entity.Ctx
            {
                BundledPrefab = _ctx.BundledPrefab,
            }).AddTo(this);

            setting.Init();

            setting.SetDescription(_ctx.NovelTitle);

            var selection = new UniTaskCompletionSource<SettingSelection>();
            setting.AddOrUpdateButton(
                _newGameButtonId,
                $"<b>{_ctx.GetLocalizationValue(UiTextKeys.NewGame)}</b>",
                () => selection.TrySetResult(SettingSelection.NewGame));

            if (_ctx.ContainAnySave())
            {
                setting.AddOrUpdateButton(
                    _continueGameButtonId,
                    $"<b>{_ctx.GetLocalizationValue(UiTextKeys.ContinueGame)}</b>",
                    () => selection.TrySetResult(SettingSelection.ContinueGame));
            }

            try
            {
                setting.Show();
                await _ctx.HideLoading();
                var result = await selection.Task.AttachExternalCancellation(
                    _ctx.CancellationToken);
                await _ctx.ShowLoading();
                return result;
            }
            finally
            {
                if (!setting.IsDisposed)
                    setting.Hide();
            }
        }
    }

    public static class UiTextKeys
    {
        public const string NewGame = "ui.new_game";
        public const string ContinueGame = "ui.continue_game";
    }
}
