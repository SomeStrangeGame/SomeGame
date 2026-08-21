using Cysharp.Threading.Tasks;
using Disposable;
using System.Threading;
using UnityEngine;

namespace Novels
{
    internal partial class NovelRuntime
    {
        private Bubble.BubbleController CreateBubble(
            IBaseDisposable owner,
            GameObject bubblePrefab,
            CancellationToken cancellationToken)
        {
            var bubble = new Bubble.BubbleController(new Bubble.BubbleController.Dependencies
            {
                BubblePrefab = bubblePrefab,
                CancellationToken = cancellationToken,
            }).AddTo(owner);
            bubble.Init();

            return bubble;
        }
    }
}
