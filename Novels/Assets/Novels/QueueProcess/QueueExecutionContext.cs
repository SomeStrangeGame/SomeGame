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
            StoryContracts.StoryDecision savedDecision,
            CancellationToken cancellationToken)
        {
            Mode = mode;
            SavedDecision = savedDecision;
            CancellationToken = cancellationToken;
        }

        public QueueExecutionMode Mode { get; }
        public StoryContracts.StoryDecision SavedDecision { get; }
        public CancellationToken CancellationToken { get; }

        public static QueueExecutionContext Live(CancellationToken cancellationToken)
        {
            return new QueueExecutionContext(
                QueueExecutionMode.Live,
                default,
                cancellationToken);
        }

        public static QueueExecutionContext Replay(
            StoryContracts.StoryDecision savedDecision,
            CancellationToken cancellationToken)
        {
            return new QueueExecutionContext(
                QueueExecutionMode.Replay,
                savedDecision,
                cancellationToken);
        }
    }
}
