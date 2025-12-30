using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game
{
    internal sealed partial class Entity
    {
        private async UniTask<AssetBundle> BundleFromCache(string path)
        {
            var rawData = ByteArrayFromCash(path);
            return await AssetBundle.LoadFromMemoryAsync(rawData);
        }

        private async UniTask<AssetBundle> BundleToCache(string path, byte[] data)
        {
            ByteArrayToCash(data, path);
            return await BundleFromCache(path);
        }
        
        private string TextFromCache(string path)
        {
            return Encoding.UTF8.GetString(ByteArrayFromCash(path));
        }

        private string TextToCache(string path, string data)
        {
            ByteArrayToCash(Encoding.UTF8.GetBytes(data), path);
            return TextFromCache(path);
        }

        private byte[] ByteArrayFromCash(string path)
        {
            var file = ConvertLocalPath(path);

            using (var fs = File.OpenRead(file))
            {
                var buffer = new byte[(int)fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        private void ByteArrayToCash(byte[] data, string path)
        {
            var file = ConvertLocalPath(path);
            if (File.Exists(file))
                File.Delete(file);
            using (var fs = File.Create(file))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        private string ConvertLocalPath(string path)
        {
            var localFilesPath = GetLocalPath();

            if (!Directory.Exists(localFilesPath))
                Directory.CreateDirectory(localFilesPath);

            var localExtraPath = path.Split('/');
            for (var i = 0; i < localExtraPath.Length - 1; i++)
            {
                localFilesPath += "/" + localExtraPath[i];
                if (!Directory.Exists(localFilesPath))
                    Directory.CreateDirectory(localFilesPath);
            }

            var result = $"{localFilesPath}/{localExtraPath.Last()}";

            return result;
        }
        
        private string GetLocalPath() 
        {
            var localFilesPath = $"{Application.persistentDataPath}/CachedFiles";
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            localFilesPath = $"file://{localFilesPath}";
#elif !UNITY_EDITOR && UNITY_WEBGL
            localFilesPath = "idbfs/CachedFiles";
#endif

            return localFilesPath;
        }
    }
}
