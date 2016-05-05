using System;
using System.IO;

namespace KeePass.Crypto
{
    /// <summary>
    /// <see cref="IInputStream"/> wrapper that provides the SHA 256 hash
    /// of the data retrieved from the stream.
    /// </summary>
    public class HashedStream : Stream
    {
        private enum Status { NotStarted, Reading, Writing };

        private Status _status;
        private readonly IHash _sha;
        private readonly Stream _stream;

        public HashedStream(IHashProvider hashProvider, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            _stream = stream;
            _sha = hashProvider.GetSha256();
            _status = Status.NotStarted;
        }

        public override bool CanRead => _stream.CanRead;

        public override bool CanSeek => _stream.CanSeek;

        public override bool CanWrite => _stream.CanWrite;

        public override long Length => _stream.Length;

        public override long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }

        protected override void Dispose(bool disposing)
        {
            _stream.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets hash of written bytes and reset.
        /// </summary>
        /// <returns>Hash of the written bytes.</returns>
        public byte[] GetHashAndReset()
        {
            _status = Status.NotStarted;

            return _sha.GetValueAndReset();
        }

        private void CheckStatus(Status expected)
        {
            if (_status == Status.NotStarted)
            {
                _status = expected;
            }
            else if (_status != expected)
            {
                throw new InvalidOperationException($"Tried to start {expected} while already {_status}");
            }
        }

        public override void Flush() => _stream.Flush();

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckStatus(Status.Reading);
            var result = _stream.Read(buffer, offset, count);
            _sha.Append(buffer);

            return result;
        }

        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

        public override void SetLength(long value) => _stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckStatus(Status.Writing);
            _sha.Append(buffer);
            _stream.Write(buffer, offset, count);
        }
    }
}