using System;
using Cysharp.Threading.Tasks;

namespace Novels.StoryExecution
{
    public readonly struct NotificationOperation : IStoryOperation
    {
        private readonly string _notificationText;
        private readonly Action<string> _showNotification;

        public NotificationOperation(
            Action<string> showNotification,
            string notificationText)
        {
            _showNotification = showNotification
                ?? throw new ArgumentNullException(nameof(showNotification));
            _notificationText = notificationText ?? string.Empty;
        }

        public UniTask Run(StoryExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (context.Mode == QueueExecutionMode.Live)
                _showNotification(_notificationText);

            return UniTask.CompletedTask;
        }
    }
}
