using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Android.App;

namespace Warpten.Utils
{
    public static class DownloadManager
    {
        private static BlockingCollection<IDownloadRequest> _requests =
            new BlockingCollection<IDownloadRequest>(new ConcurrentQueue<IDownloadRequest>());

        private static CancellationTokenSource _token;

        public static int SynchronousDownloadCount { get; set; } = 1;
        private static int _usedSlots;

        public static event Action<IDownloadRequest, DownloadStatus> OnProgress;

        public static void Enqueue(IDownloadRequest request)
        {
            _requests.Add(request);

            if (_requests.Count > 1)
            {
                // If more than one request is in queue, it means we already have a worker running!
                return;
            }
            
            // .. Otherwise, we're free to start a worker that will immediately process the incoming data.
            _token = new CancellationTokenSource();
            Task.Factory.StartNew(() =>
            {
                while (!_token.IsCancellationRequested)
                {
                    if (_usedSlots == SynchronousDownloadCount)
                        continue;

                    // Exit early if no request was found
                    if (!_requests.TryTake(out var newRequest, 5000, _token.Token))
                        return;

                    newRequest.OnProgress += downloadStatus =>
                    {
                        switch (downloadStatus)
                        {
                            case DownloadStatus.Downloading:
                                ++_usedSlots;
                                break;
                            case DownloadStatus.Done:
                                --_usedSlots;
                                break;
                        }

                        OnProgress?.Invoke(newRequest, downloadStatus);
                    };

                    newRequest.CreatePendingIntents(Application.Context);
                    // Non-blocking call to allow for simultaneous downloads
                    newRequest.Process(_token).ConfigureAwait(false);
                }
            }, _token.Token);
        }

        public static void Stop()
        {
            _requests.CompleteAdding();
            _token.Cancel();
        }
    }
}
