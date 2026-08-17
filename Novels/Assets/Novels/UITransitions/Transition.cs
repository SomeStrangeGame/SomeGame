using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Novels.UITransitions
{
    public static class Transition
    {
        public static UniTask Fade(
            CanvasGroup canvasGroup,
            float from,
            float to,
            float duration,
            CancellationToken cancellationToken)
        {
            if (canvasGroup == null)
                throw new ArgumentNullException(nameof(canvasGroup));
            return Animate(
                duration,
                progress => canvasGroup.alpha = Mathf.Lerp(from, to, progress),
                cancellationToken);
        }

        public static UniTask FadeAndMove(
            CanvasGroup canvasGroup,
            Vector3 from,
            Vector3 to,
            float duration,
            CancellationToken cancellationToken)
        {
            if (canvasGroup == null)
                throw new ArgumentNullException(nameof(canvasGroup));
            return Animate(
                duration,
                progress =>
                {
                    canvasGroup.alpha = progress;
                    canvasGroup.transform.localPosition = Vector3.Lerp(from, to, progress);
                },
                cancellationToken);
        }

        public static UniTask Move(
            Transform target,
            Vector3 from,
            Vector3 to,
            float duration,
            AnimationCurve curve,
            CancellationToken cancellationToken)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (from == to)
            {
                target.localPosition = to;
                return UniTask.CompletedTask;
            }
            return Animate(
                duration,
                progress => target.localPosition = Vector3.Lerp(
                    from,
                    to,
                    curve == null ? progress : curve.Evaluate(progress)),
                cancellationToken);
        }

        private static async UniTask Animate(
            float duration,
            Action<float> apply,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            apply(0f);
            if (duration <= 0f)
            {
                apply(1f);
                return;
            }
            var elapsed = 0f;
            while (elapsed < duration)
            {
                await UniTask.Yield(cancellationToken);
                elapsed += Time.deltaTime;
                apply(Mathf.Clamp01(elapsed / duration));
            }
            apply(1f);
        }
    }
}
