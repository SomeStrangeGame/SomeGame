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
            out Queue<StoryExecution.IStoryOperation> queue);
        internal delegate bool TryCompleteQueueDelegate(
            out Queue<StoryExecution.IStoryOperation> queue);
        internal delegate UniTask ExecuteQueueDelegate(
            Queue<StoryExecution.IStoryOperation> queue,
            StoryContracts.StoryDecision? savedDecision,
            CancellationToken cancellationToken);

        internal struct Dependencies
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
        }

        private readonly Dependencies _dependencies;

        internal NovelProcess(Dependencies dependencies)
        {
            _dependencies = dependencies;
        }

        internal async UniTask<EpisodeRunResult> Run()
        {
            await _dependencies.HideLoading()
                .AttachExternalCancellation(_dependencies.CancellationToken);

            while (!IsDisposed)
            {
                await UniTask.Yield(_dependencies.CancellationToken);

                var readResult = _dependencies.ReadNext();
                if (readResult.Status == StoryProcessor.StoryReadStatus.Completed)
                {
                    if (_dependencies.CompleteQueue(out var finalQueue))
                    {
                        var execution = await TryExecute(finalQueue, null);
                        if (execution.HasValue)
                            return execution.Value;
                    }
                    return EpisodeRunResult.Completed();
                }

                var stepResult = _dependencies.ParseStep(
                    readResult.Source,
                    readResult.Choices);
                if (!stepResult.IsSuccess)
                {
                    _dependencies.OnError(new Diagnostics.NovelError(
                        stepResult.Error.Code,
                        Diagnostics.NovelErrorSeverity.Recoverable,
                        stepResult.Error.Message,
                        stepResult.Error.Source));
                    continue;
                }

                if (!_dependencies.BuildQueue(stepResult.Step, out var queue))
                    continue;

                var result = await TryExecute(
                    queue,
                    _dependencies.GetNextSavedDecision());
                if (result.HasValue)
                    return result.Value;

                if (_dependencies.IsEpisodeEnd?.Invoke(readResult.Source) == true)
                    return EpisodeRunResult.Completed(_dependencies.ExportStoryState());
            }

            return EpisodeRunResult.Cancelled();
        }

        private async UniTask<EpisodeRunResult?> TryExecute(
            Queue<StoryExecution.IStoryOperation> queue,
            StoryContracts.StoryDecision? savedDecision)
        {
            try
            {
                await _dependencies.ExecuteQueue(
                    queue,
                    savedDecision,
                    _dependencies.CancellationToken);
                return null;
            }
            catch (OperationCanceledException)
                when (_dependencies.CancellationToken.IsCancellationRequested)
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
