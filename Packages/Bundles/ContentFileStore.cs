using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bundles
{
    internal sealed class ContentFileStore
    {
        private readonly ContentPayloadMaterializer _materializer;
        private readonly IContentSource _source;
        private readonly CancellationToken _cancellationToken;

        internal ContentFileStore(
            IContentSource source,
            ContentPayloadMaterializer materializer,
            CancellationToken cancellationToken)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _materializer = materializer
                ?? throw new ArgumentNullException(nameof(materializer));
            _cancellationToken = cancellationToken;
        }

        internal async UniTask<string> ResolveUrl(
            ContentReleaseSession session,
            string path,
            Action<long> onDownloadedBytes = null)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (string.IsNullOrWhiteSpace(path))
                return null;
            var descriptor = session.FindFile(path) ?? throw new ContentIntegrityException(
                $"File '{path}' is absent from release '{session.ReleaseId}'.");
            var localPath = await _materializer.Materialize(
                GetPayload(session, descriptor),
                onDownloadedBytes);
            return new Uri(localPath).AbsoluteUri;
        }

        internal ContentPayloadRequest GetPayload(
            ContentReleaseSession session,
            ContentFileDescriptor descriptor) =>
            new(
                descriptor.Path,
                _source.ResolveFilePayloadPath(
                    descriptor.Path,
                    descriptor.PayloadPath),
                ContentStoragePlanner.FilePath(descriptor),
                descriptor.Size,
                descriptor.Sha256);

        internal async UniTask<string> GetText(
            ContentReleaseSession session,
            string path)
        {
            var url = await ResolveUrl(session, path);
            var localPath = new Uri(url).LocalPath;
            string text = null;
            Exception failure = null;
            await UniTask.SwitchToThreadPool();
            try
            {
                text = File.ReadAllText(localPath);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            await UniTask.SwitchToMainThread();
            if (failure != null)
                ExceptionDispatchInfo.Capture(failure).Throw();
            _cancellationToken.ThrowIfCancellationRequested();
            return text;
        }
    }
}
