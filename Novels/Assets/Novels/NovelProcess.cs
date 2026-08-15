using System;
using System.Collections.Generic;
using System.Threading;
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
            byte? savedChoice,
            CancellationToken cancellationToken);

        internal struct Ctx
        {
            internal Func<StoryProcessor.StoryReadResult> ReadNext;
            internal Func<string, StoryContracts.StoryChoice[], StoryCommands.StoryStepResult> ParseStep;
            internal TryBuildQueueDelegate BuildQueue;
            internal ExecuteQueueDelegate ExecuteQueue;

            internal Func<byte?> GetNextSavedChoice;
            internal Func<UniTask> HideLoading;
            internal CancellationToken CancellationToken;

            public Action<(LogType type, string message)> OnLog;
        }

        private Ctx _ctx;

        internal NovelProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask ShowNovelProcess()
        {
            await _ctx.HideLoading().AttachExternalCancellation(_ctx.CancellationToken);

            while (!IsDisposed)
            {
                await UniTask.Yield(_ctx.CancellationToken);

                var readResult = _ctx.ReadNext();
                if (readResult.Status == StoryProcessor.StoryReadStatus.Completed)
                    return;

                var stepResult = _ctx.ParseStep(readResult.Source, readResult.Choices);
                if (!stepResult.IsSuccess)
                {
                    _ctx.OnLog((LogType.Error, $"[StoryParser] {stepResult.Error.Code}: {stepResult.Error.Message}\nSource: {stepResult.Error.Source}"));
                    continue;
                }

                if (!_ctx.BuildQueue(stepResult.Step, out var queue))
                    continue;

                await _ctx.ExecuteQueue(
                    queue,
                    _ctx.GetNextSavedChoice(),
                    _ctx.CancellationToken);
            }
        }
    }
}
