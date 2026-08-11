using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public struct EmptyQueue : IQueue
    {
        public async readonly UniTask Run()
        {
            
        }

        public async readonly UniTask RunImmediate(byte choice)
        {
            
        }
    }
}
