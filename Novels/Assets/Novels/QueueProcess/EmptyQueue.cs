using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public struct EmptyQueue : IQueue
    {
        public async readonly UniTask Run(QueueExecutionContext context)
        {
        }
    }
}
