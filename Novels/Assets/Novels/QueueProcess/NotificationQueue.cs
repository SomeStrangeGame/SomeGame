using System;
using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public readonly struct NotificationQueue : IQueue
    {
        private readonly string _notificationText;
        private readonly Action<string> _showNotification;

        public NotificationQueue(
            Action<string> showNotification,
            string notificationText)
        {
            _showNotification = showNotification
                ?? throw new ArgumentNullException(nameof(showNotification));
            _notificationText = notificationText ?? string.Empty;
        }

        public UniTask Run(QueueExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            if (context.Mode == QueueExecutionMode.Live)
                _showNotification(_notificationText);

            return UniTask.CompletedTask;
        }
    }
}
