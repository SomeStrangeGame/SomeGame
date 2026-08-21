using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public readonly struct PlayAudioOperation : IStoryOperation
    {
        private readonly Func<string, UniTask> _playAudio;
        private readonly string _assetName;

        public PlayAudioOperation(Func<string, UniTask> playAudio, string assetName)
        {
            _playAudio = playAudio ?? throw new ArgumentNullException(nameof(playAudio));
            _assetName = assetName ?? string.Empty;
        }

        public async UniTask Run(StoryExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            await _playAudio(_assetName);
        }
    }
}
