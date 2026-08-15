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
            byte savedChoice)
        {
            Mode = mode;
            SavedChoice = savedChoice;
        }

        public QueueExecutionMode Mode { get; }
        public byte SavedChoice { get; }

        public static QueueExecutionContext Live()
        {
            return new QueueExecutionContext(QueueExecutionMode.Live, default);
        }

        public static QueueExecutionContext Replay(byte savedChoice)
        {
            return new QueueExecutionContext(QueueExecutionMode.Replay, savedChoice);
        }
    }
}
