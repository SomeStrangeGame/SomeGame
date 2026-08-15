namespace Novels
{
    internal partial class Entity
    {
        private static QueueProcess.Executor CreateQueueExecutor()
        {
            return new QueueProcess.Executor();
        }
    }
}
