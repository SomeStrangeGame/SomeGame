using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Disposable;
using UnityEngine;

namespace Cache
{
    public class Entity : BaseDisposable
    {
        public async UniTask<AssetBundle> BundleFromCache(string path)
        {
            var rawData = ByteArrayFromCash(path);
            return await AssetBundle.LoadFromMemoryAsync(rawData);
        }

        public async UniTask<AssetBundle> BundleToCache(string path, byte[] data)
        {
            ByteArrayToCash(data, path);
            return await BundleFromCache(path);
        }
        
        public string TextFromCache(string path)
        {
            return Encoding.UTF8.GetString(ByteArrayFromCash(path));
        }

        public string TextToCache(string path, string data)
        {
            ByteArrayToCash(Encoding.UTF8.GetBytes(data), path);
            return TextFromCache(path);
        }

        public byte[] ByteArrayFromCash(string path)
        {
            var file = ConvertLocalPath(path);

            using (var fs = File.OpenRead(file))
            {
                var buffer = new byte[(int)fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }

        public void ByteArrayToCash(byte[] data, string path)
        {
            var file = ConvertLocalPath(path);
            if (File.Exists(file))
                File.Delete(file);
            using (var fs = File.Create(file))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        public string ConvertLocalPath(string path)
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
#if UNITY_EDITOR_OSX
            localFilesPath = $"file://{localFilesPath}";
#endif

            return localFilesPath;
        }
    }
}

