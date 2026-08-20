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
            StoryContracts.StoryDecision? savedDecision,
            CancellationToken cancellationToken);

        internal struct Ctx
        {
            internal Func<StoryProcessor.StoryReadResult> ReadNext;
            internal Func<string> ExportStoryState;
            internal Func<string, bool> IsEpisodeEnd;
            internal Func<string, StoryContracts.StoryChoice[], StoryCommands.StoryStepResult> ParseStep;
            internal TryBuildQueueDelegate BuildQueue;
            internal TryCompleteQueueDelegate CompleteQueue;
            internal ExecuteQueueDelegate ExecuteQueue;

            internal Func<StoryContracts.StoryDecision?> GetNextSavedDecision;
            internal Func<UniTask> HideLoading;
            internal CancellationToken CancellationToken;

            internal Action<Diagnostics.NovelError> OnError;
            internal Action<StoryProcessor.StorySourceLocation> OnStorySourceChanged;
        }

        private Ctx _ctx;

        internal NovelProcess(Ctx ctx)
        {
            _ctx = ctx;
        }

        internal async UniTask<EpisodeRunResult> ShowNovelProcess()
        {
            await _ctx.HideLoading().AttachExternalCancellation(_ctx.CancellationToken);

            while (!IsDisposed)
            {
                await UniTask.Yield(_ctx.CancellationToken);

                var readResult = _ctx.ReadNext();
                _ctx.OnStorySourceChanged?.Invoke(readResult.SourceLocation);
                if (readResult.Status == StoryProcessor.StoryReadStatus.Completed)
                {
                    if (_ctx.CompleteQueue(out var finalQueue))
                    {
                        var execution = await TryExecute(finalQueue, null);
                        if (execution.HasValue)
                            return execution.Value;
                    }
                    return EpisodeRunResult.Completed();
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

                var result = await TryExecute(queue, _ctx.GetNextSavedDecision());
                if (result.HasValue)
                    return result.Value;

                if (_ctx.IsEpisodeEnd?.Invoke(readResult.Source) == true)
                    return EpisodeRunResult.Completed(_ctx.ExportStoryState());
            }

            return EpisodeRunResult.Cancelled();
        }

        private async UniTask<EpisodeRunResult?> TryExecute(
            Queue<QueueProcess.IQueue> queue,
            StoryContracts.StoryDecision? savedDecision)
        {
            try
            {
                await _ctx.ExecuteQueue(
                    queue,
                    savedDecision,
                    _ctx.CancellationToken);
                return null;
            }
            catch (OperationCanceledException)
                when (_ctx.CancellationToken.IsCancellationRequested)
            {
                return EpisodeRunResult.Cancelled();
            }
            catch (Exception exception)
            {
                return EpisodeRunResult.Failed(new Diagnostics.NovelError(
                    Diagnostics.NovelErrorCodes.QueueExecutionFailed,
                    Diagnostics.NovelErrorSeverity.Fatal,
                    "Story queue execution failed.",
                    exception: exception));
            }
        }
    }
}
