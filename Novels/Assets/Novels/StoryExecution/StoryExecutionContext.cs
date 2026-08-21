using System.Threading;

namespace Novels.StoryExecution
{
    public enum QueueExecutionMode
    {
        Live,
        Replay,
    }

    public readonly struct StoryExecutionContext
    {
        private StoryExecutionContext(
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

        public static StoryExecutionContext Live(CancellationToken cancellationToken)
        {
            return new StoryExecutionContext(
                QueueExecutionMode.Live,
                default,
                cancellationToken);
        }

        public static StoryExecutionContext Replay(
            StoryContracts.StoryDecision savedDecision,
            CancellationToken cancellationToken)
        {
            return new StoryExecutionContext(
                QueueExecutionMode.Replay,
                savedDecision,
                cancellationToken);
        }
    }
}
