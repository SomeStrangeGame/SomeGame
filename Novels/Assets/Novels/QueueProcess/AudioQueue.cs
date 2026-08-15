using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public readonly struct AudioQueue : IQueue
    {
        private readonly Func<string, UniTask> _playAudio;
        private readonly string _assetName;

        public AudioQueue(Func<string, UniTask> playAudio, string assetName)
        {
            _playAudio = playAudio ?? throw new ArgumentNullException(nameof(playAudio));
            _assetName = assetName ?? string.Empty;
        }

        public async UniTask Run(QueueExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            await _playAudio(_assetName);
        }
    }
}
