using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;

namespace Warpten.Utils
{
    public interface IDownloadRequest
    {
        /// <summary>
        /// Processes the download request.
        /// </summary>
        /// <param name="tokenSource"></param>
        /// <returns></returns>
        Task Process(CancellationTokenSource tokenSource);

        /// <summary>
        /// Called upon progress status (Started, Queued, Downloading, Done)
        /// </summary>
        event Action<DownloadStatus> OnProgress;

        // Just in case you might need it - I do.
        void CreatePendingIntents(Context context);
    }
}
