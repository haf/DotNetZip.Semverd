// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;

namespace Ionic.Zip.Deflate64
{
    // Deflate64Stream supports decompression of Deflate64 format only.
    internal sealed class Deflate64Stream : Stream
    {
        internal const int DefaultBufferSize = 8192;

        private Stream _stream;
        private InflaterManaged _inflater;
        private readonly byte[] _buffer;

        // A specific constructor to allow decompression of Deflate64
        internal Deflate64Stream(Stream stream, long uncompressedSize = -1)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead)
                throw new ArgumentException("NotSupported_UnreadableStream", nameof(stream));

            _inflater = new InflaterManaged(null, true, uncompressedSize);

            _stream = stream;
            _buffer = new byte[DefaultBufferSize];
        }

        public override bool CanRead
        {
            get
            {
                if (_stream == null)
                {
                    return false;
                }

                return _stream.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek => false;

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override void Flush()
        {
            EnsureNotDisposed();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] array, int offset, int count)
        {
            ValidateParameters(array, offset, count);
            EnsureNotDisposed();

            int bytesRead;
            int currentOffset = offset;
            int remainingCount = count;

            while (true)
            {
                bytesRead = _inflater.Inflate(array, currentOffset, remainingCount);
                currentOffset += bytesRead;
                remainingCount -= bytesRead;

                if (remainingCount == 0)
                {
                    break;
                }

                if (_inflater.Finished())
                {
                    // if we finished decompressing, we can't have anything left in the outputwindow.
                    Debug.Assert(_inflater.AvailableOutput == 0, "We should have copied all stuff out!");
                    break;
                }

                int bytes = _stream.Read(_buffer, 0, _buffer.Length);
                if (bytes <= 0)
                {
                    break;
                }
                else if (bytes > _buffer.Length)
                {
                    // The stream is either malicious or poorly implemented and returned a number of
                    // bytes larger than the buffer supplied to it.
                    throw new InvalidDataException();
                }

                _inflater.SetInput(_buffer, 0, bytes);
            }

            return count - remainingCount;
        }

        private void ValidateParameters(byte[] array, int offset, int count)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (array.Length - offset < count)
                throw new ArgumentException("InvalidArgumentOffsetCount");
        }

        private void EnsureNotDisposed()
        {
            if (_stream == null)
                ThrowStreamClosedException();
        }

        private static void ThrowStreamClosedException()
        {
            throw new ObjectDisposedException(null, "ObjectDisposed_StreamClosed");
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] array, int offset, int count)
        {
            throw new InvalidOperationException("CannotWriteToDeflateStream");
        }

        // This is called by Dispose:
        private void PurgeBuffers(bool disposing)
        {
            if (!disposing)
                return;

            if (_stream == null)
                return;

            Flush();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                PurgeBuffers(disposing);
            }
            finally
            {
                // Close the underlying stream even if PurgeBuffers threw.
                // Stream.Close() may throw here (may or may not be due to the same error).
                // In this case, we still need to clean up internal resources, hence the inner finally blocks.
                try
                {
                    if (disposing && _stream != null)
                        _stream.Dispose();
                }
                finally
                {
                    _stream = null;

                    try
                    {
                        if (_inflater != null)
                            _inflater.Dispose();
                    }
                    finally
                    {
                        _inflater = null;
                        base.Dispose(disposing);
                    }
                }
            }
        }
    }
}
