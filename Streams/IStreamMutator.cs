using System.IO;

namespace Warpten.Utils.Streams
{
    public interface IStreamMutator
    {
        void Write(Stream outputStream, byte[] buffer, int offset, int count);

        int Read(Stream inputStream, byte[] buffer, int offset, int count);

        long Seek(Stream backingStream, long offset, SeekOrigin origin);
    }
}
