using System;
using System.IO;

namespace Warpten.Utils.Streams.
{
    /// <summary>
    /// A wrapper around <see cref="Stream"/>, allowing for proper manipulation on the
    /// data received during write or read operations before actually processing them.
    /// </summary>
    public class MutatedStream : Stream
    {
        private Stream _backingStream;
        private IStreamMutator _streamMutator;

        public event Action<int> ReadProgress;

        public MutatedStream(Stream backingStream, IStreamMutator mutator)
        {
            _backingStream = backingStream;

            _streamMutator = mutator;
        }
        
        protected override void Dispose(bool disposing)
        {
            Flush();
            _backingStream.Dispose();
            base.Dispose(disposing);
        }

        public override bool CanRead => _backingStream.CanRead;
        public override bool CanSeek => _backingStream.CanSeek;
        public override bool CanWrite => _backingStream.CanWrite;

        public override long Length => _backingStream.Length;

        public override long Position
        {
            get { return _backingStream.Position; }
            set => _backingStream.Seek(value, SeekOrigin.Begin);
        }

        public override void Flush()
        {
            if (_backingStream.CanWrite)
                _backingStream.Flush();
        }

        /// <summary>
        /// Reads from the underlying stream, mutating the data read
        /// using <see cref="_streamMutator"/> if it is assigned.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>The actual number of bytes read, after mutation if applicable.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var readCount = _streamMutator?.Read(_backingStream, buffer, offset, count)
                ?? _backingStream.Read(buffer, offset, count);
            ReadProgress?.Invoke(readCount);
            return readCount;
        }

        /// <summary>
        /// Writes to the underlying stream, mutating the provided
        /// buffer as needed.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_streamMutator != null)
                _streamMutator.Write(_backingStream, buffer, offset, count);
            else
                _backingStream.Write(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!_backingStream.CanSeek)
                throw new InvalidOperationException("The backing stream of this mutator cannot be seeked!");

            if (_streamMutator != null)
                return _streamMutator.Seek(_backingStream, offset, origin);

            return _backingStream.Seek(offset, origin);
        }

        public override void SetLength(long value) =>
            _backingStream.SetLength(value);
    }
}
