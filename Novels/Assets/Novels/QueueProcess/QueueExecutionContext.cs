using System.Threading;

namespace Novels.QueueProcess
{
    public enum QueueExecutionMode
    {
        Live,
        Replay,
    }

    public readonly struct QueueExecutionContext
    {
        private QueueExecutionContext(
            QueueExecutionMode mode,
            byte savedChoice,
            CancellationToken cancellationToken)
        {
            Mode = mode;
            SavedChoice = savedChoice;
            CancellationToken = cancellationToken;
        }

        public QueueExecutionMode Mode { get; }
        public byte SavedChoice { get; }
        public CancellationToken CancellationToken { get; }

        public static QueueExecutionContext Live(CancellationToken cancellationToken)
        {
            return new QueueExecutionContext(
                QueueExecutionMode.Live,
                default,
                cancellationToken);
        }

        public static QueueExecutionContext Replay(
            byte savedChoice,
            CancellationToken cancellationToken)
        {
            return new QueueExecutionContext(
                QueueExecutionMode.Replay,
                savedChoice,
                cancellationToken);
        }
    }
}
