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
        internal delegate bool TryCompleteQueueDelegate(
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
            internal TryCompleteQueueDelegate CompleteQueue;
            internal ExecuteQueueDelegate ExecuteQueue;

            internal Func<byte?> GetNextSavedChoice;
            internal Func<UniTask> HideLoading;
            internal CancellationToken CancellationToken;

            internal Action<Diagnostics.NovelError> OnError;
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
                {
                    if (_ctx.CompleteQueue(out var finalQueue))
                    {
                        if (!await TryExecute(finalQueue, null))
                            return;
                    }
                    return;
                }

                var stepResult = _ctx.ParseStep(readResult.Source, readResult.Choices);
                if (!stepResult.IsSuccess)
                {
                    _ctx.OnError(new Diagnostics.NovelError(
                        stepResult.Error.Code,
                        Diagnostics.NovelErrorSeverity.Recoverable,
                        stepResult.Error.Message,
                        stepResult.Error.Source));
                    continue;
                }

                if (!_ctx.BuildQueue(stepResult.Step, out var queue))
                    continue;

                if (!await TryExecute(queue, _ctx.GetNextSavedChoice()))
                    return;
            }
        }

        private async UniTask<bool> TryExecute(
            Queue<QueueProcess.IQueue> queue,
            byte? savedChoice)
        {
            try
            {
                await _ctx.ExecuteQueue(
                    queue,
                    savedChoice,
                    _ctx.CancellationToken);
                return true;
            }
            catch (OperationCanceledException)
                when (_ctx.CancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _ctx.OnError(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.QueueExecutionFailed,
                    Diagnostics.NovelErrorSeverity.Fatal,
                    "Story queue execution failed.",
                    exception: exception));
                return false;
            }
        }
    }
}
