using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Novels
{
    public sealed class NovelProcess : BaseDisposable
    {
        public delegate bool TryBuildQueueDelegate(
            StoryCommands.StoryStep step,
            out Queue<StoryExecution.IStoryOperation> queue);
        public delegate bool TryCompleteQueueDelegate(
            out Queue<StoryExecution.IStoryOperation> queue);
        public delegate UniTask ExecuteQueueDelegate(
            Queue<StoryExecution.IStoryOperation> queue,
            StoryContracts.StoryDecision? savedDecision,
            CancellationToken cancellationToken);

        public struct Dependencies
        {
            public Func<StoryProcessor.StoryReadResult> ReadNext;
            public Func<string> ExportStoryState;
            public Func<string, bool> IsEpisodeEnd;
            public Func<string, StoryContracts.StoryChoice[], StoryCommands.StoryStepResult> ParseStep;
            public TryBuildQueueDelegate BuildQueue;
            public TryCompleteQueueDelegate CompleteQueue;
            public ExecuteQueueDelegate ExecuteQueue;

            public Func<StoryContracts.StoryDecision?> GetNextSavedDecision;
            public Func<UniTask> HideLoading;
            public Action OnReady;
            public CancellationToken CancellationToken;

            public Action<StoryRuntimeError> OnError;
            public Action<StoryProcessor.StorySourceLocation> OnStorySourceChanged;
        }

        private readonly Dependencies _dependencies;

        public NovelProcess(Dependencies dependencies)
        {
            _dependencies = dependencies;
        }

        public async UniTask<EpisodeRunResult> Run()
        {
            await _dependencies.HideLoading()
                .AttachExternalCancellation(_dependencies.CancellationToken);
            _dependencies.OnReady?.Invoke();

            while (!IsDisposed)
            {
                await UniTask.Yield(_dependencies.CancellationToken);

                var readResult = _dependencies.ReadNext();
                _dependencies.OnStorySourceChanged?.Invoke(readResult.SourceLocation);
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
                    _dependencies.OnError(new StoryRuntimeError(
                        stepResult.Error.Code,
                        StoryRuntimeErrorSeverity.Recoverable,
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
                return EpisodeRunResult.Failed(new StoryRuntimeError(
                    StoryRuntimeErrorCodes.QueueExecutionFailed,
                    StoryRuntimeErrorSeverity.Fatal,
                    "Story queue execution failed.",
                    exception: exception));
            }
        }
    }
}
