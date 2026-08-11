using Cysharp.Threading.Tasks;

namespace Novels.QueueProcess
{
    public interface IQueue
    {
        public UniTask Run();
        public UniTask RunImmediate(byte choice);
    }
}

