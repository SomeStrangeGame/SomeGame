using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Novels
{
    internal sealed class DirectApkUpdater : Catalog.ICatalogUpdateAction
    {
        private const string _nativeClass = "com.pureshechka.novels.update.DirectApkUpdater";
        private const long _maximumApkSize = 512L * 1024L * 1024L;
        private readonly Uri _uri;
        private readonly long _expectedSize;
        private readonly string _expectedSha256;
        private readonly int _expectedVersionCode;
        private readonly CancellationToken _cancellationToken;
        private bool _running;

        private DirectApkUpdater(Uri uri, long expectedSize, string expectedSha256,
            int expectedVersionCode)
        {
            _uri = uri;
            _expectedSize = expectedSize;
            _expectedSha256 = expectedSha256.ToLowerInvariant();
            _expectedVersionCode = expectedVersionCode;
            _cancellationToken = Application.exitCancellationToken;
            State = Catalog.CatalogUpdateState.Ready;
            StatusMessage = "Обновление будет загружено с защищённого сервера приложения.";
        }

        public Catalog.CatalogUpdateState State { get; private set; }
        public float Progress { get; private set; }
        public string StatusMessage { get; private set; }
        public bool CanStart => !_running;

        internal static int CurrentVersionCode
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    using var bridge = new AndroidJavaClass(_nativeClass);
                    using var activity = CurrentActivity();
                    return bridge.CallStatic<int>("getInstalledVersionCode", activity);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Cannot read Android versionCode: {exception.Message}");
                }
#endif
                return 0;
            }
        }

        internal static Catalog.ICatalogUpdateAction TryCreate(string url, long size,
            string sha256, int versionCode)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps
                || size <= 0
                || size > _maximumApkSize
                || versionCode <= 0
                || string.IsNullOrWhiteSpace(sha256)
                || sha256.Length != 64)
                return null;
            foreach (var symbol in sha256)
                if (!Uri.IsHexDigit(symbol)) return null;
            return new DirectApkUpdater(uri, size, sha256, versionCode);
        }

        public void Start()
        {
            if (!CanStart) return;
            if (State == Catalog.CatalogUpdateState.PermissionRequired
                || State == Catalog.CatalogUpdateState.Installing)
            {
                ContinueInstallation();
                return;
            }
            DownloadAndInstall().Forget();
        }

        private async UniTaskVoid DownloadAndInstall()
        {
            _running = true;
            var directory = Path.Combine(Application.temporaryCachePath, "direct-apk-update");
            var temporaryPath = Path.Combine(directory, "update.apk.part");
            var finalPath = Path.Combine(directory, "update.apk");
            try
            {
                Directory.CreateDirectory(directory);
                DeleteIfPresent(temporaryPath);
                DeleteIfPresent(finalPath);
                State = Catalog.CatalogUpdateState.Downloading;
                StatusMessage = "Загружаем обновление…";
                using var request = UnityWebRequest.Get(_uri);
                request.timeout = 120;
                request.downloadHandler = new DownloadHandlerFile(temporaryPath);
                var operation = request.SendWebRequest();
                using var cancellation = _cancellationToken.Register(request.Abort);
                while (!operation.isDone)
                {
                    Progress = Math.Max(0f, request.downloadProgress);
                    if ((long) request.downloadedBytes > _expectedSize)
                    {
                        request.Abort();
                        throw new InvalidDataException("APK exceeded its declared size.");
                    }
                    await UniTask.Yield(PlayerLoopTiming.Update, _cancellationToken);
                }
                if (request.result != UnityWebRequest.Result.Success)
                    throw new InvalidOperationException(request.error);
                if (!Uri.TryCreate(request.url, UriKind.Absolute, out var finalUri)
                    || finalUri.Scheme != Uri.UriSchemeHttps
                    || !string.Equals(finalUri.Host, _uri.Host,
                        StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("APK redirect left the trusted HTTPS host.");
                var actualSize = new FileInfo(temporaryPath).Length;
                if (actualSize != _expectedSize)
                    throw new InvalidDataException($"Unexpected APK size: {actualSize}.");

                State = Catalog.CatalogUpdateState.Verifying;
                StatusMessage = "Проверяем подлинность обновления…";
                await UniTask.SwitchToThreadPool();
                var actualSha256 = ComputeSha256(temporaryPath);
                await UniTask.SwitchToMainThread(_cancellationToken);
                if (!string.Equals(actualSha256, _expectedSha256,
                        StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("APK checksum does not match the manifest.");
                File.Move(temporaryPath, finalPath);

#if UNITY_ANDROID && !UNITY_EDITOR
                using var bridge = new AndroidJavaClass(_nativeClass);
                using var activity = CurrentActivity();
                var verification = bridge.CallStatic<string>(
                    "verifyApk", activity, finalPath, _expectedVersionCode);
                if (!string.IsNullOrEmpty(verification))
                    throw new InvalidDataException($"APK rejected: {verification}.");
                if (!bridge.CallStatic<bool>("canRequestPackageInstalls", activity))
                {
                    State = Catalog.CatalogUpdateState.PermissionRequired;
                    StatusMessage = "Разрешите приложению устанавливать обновления и нажмите кнопку ещё раз.";
                    return;
                }
                State = Catalog.CatalogUpdateState.Installing;
                StatusMessage = "Подтвердите обновление в системном окне Android.";
                bridge.CallStatic("installApk", activity, finalPath);
#else
                throw new PlatformNotSupportedException("Direct APK installation is Android-only.");
#endif
            }
            catch (OperationCanceledException) when (_cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                DeleteIfPresent(temporaryPath);
                DeleteIfPresent(finalPath);
                State = Catalog.CatalogUpdateState.Failed;
                StatusMessage = "Не удалось подготовить обновление. Проверьте интернет и повторите попытку.";
                Debug.LogWarning($"Direct APK update failed: {exception.Message}");
            }
            finally
            {
                _running = false;
            }
        }

        private void ContinueInstallation()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var bridge = new AndroidJavaClass(_nativeClass);
                using var activity = CurrentActivity();
                if (!bridge.CallStatic<bool>("canRequestPackageInstalls", activity))
                {
                    bridge.CallStatic("openUnknownSourcesSettings", activity);
                    return;
                }
                var finalPath = Path.Combine(Application.temporaryCachePath,
                    "direct-apk-update", "update.apk");
                if (!File.Exists(finalPath))
                {
                    State = Catalog.CatalogUpdateState.Ready;
                    DownloadAndInstall().Forget();
                    return;
                }
                State = Catalog.CatalogUpdateState.Installing;
                StatusMessage = "Подтвердите обновление в системном окне Android.";
                bridge.CallStatic("installApk", activity, finalPath);
            }
            catch (Exception exception)
            {
                State = Catalog.CatalogUpdateState.Failed;
                StatusMessage = "Не удалось продолжить установку обновления.";
                Debug.LogWarning($"Cannot continue APK installation: {exception.Message}");
            }
#endif
        }

        private static string ComputeSha256(string path)
        {
            using var stream = File.OpenRead(path);
            using var algorithm = SHA256.Create();
            return BitConverter.ToString(algorithm.ComputeHash(stream))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject CurrentActivity()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return player.GetStatic<AndroidJavaObject>("currentActivity");
        }
#endif

        private static void DeleteIfPresent(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception exception) { Debug.LogWarning($"Cannot delete update file: {exception.Message}"); }
        }
    }
}
