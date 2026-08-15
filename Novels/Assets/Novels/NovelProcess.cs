using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    internal class NovelProcess : BaseDisposable
    {
        internal delegate bool TryBuildQueueDelegate(
            StoryCommands.StoryStep step,
            out Queue<QueueProcess.IQueue> queue);
        internal delegate UniTask ExecuteQueueDelegate(
            Queue<QueueProcess.IQueue> queue,
            byte? savedChoice);

        internal struct Ctx
        {
            internal Func<StoryCommands.StoryStepResult> GetNextStep;
            internal TryBuildQueueDelegate BuildQueue;
            internal ExecuteQueueDelegate ExecuteQueue;

            internal Func<byte?> GetNextSavedChoice;
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
            await _ctx.HideLoading();

            while (!IsDisposed)
            {
                await UniTask.Yield();

                var stepResult = _ctx.GetNextStep();
                if (!stepResult.IsSuccess)
                {
                    _ctx.OnLog((LogType.Error, $"[StoryParser] {stepResult.Error.Code}: {stepResult.Error.Message}\nSource: {stepResult.Error.Source}"));
                    continue;
                }

                if (!_ctx.BuildQueue(stepResult.Step, out var queue))
                    continue;

                await _ctx.ExecuteQueue(queue, _ctx.GetNextSavedChoice());
            }
        }
    }
}
