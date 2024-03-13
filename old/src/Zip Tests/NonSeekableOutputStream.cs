// NonSeekableOutputStream.cs
// ------------------------------------------------------------------
//
// Need a non-seekable output stream to test ZIP construction.
//
// ------------------------------------------------------------------
//
// Copyright (c) 2009 by Dino Chiesa
// All rights reserved!
//
// ------------------------------------------------------------------


using System;
using System.IO;


namespace Ionic.Zip.Tests
{
    public class NonSeekableOutputStream : Stream
    {
        protected Stream _s;
        protected bool  _disposed;

        public NonSeekableOutputStream (Stream s) : base()
        {
            if (!s.CanWrite)
                throw new NotSupportedException();
            _s = s;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _s.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            _s.Flush();
        }

        public override long Length
        {
            get { return _s.Length; }
        }

        public override long Position
        {
            get { return _s.Position; }
            set { _s.Position = value; }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            _s.SetLength(value);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!_disposed)
                {
                    if (disposing && (this._s != null))
                        this._s.Dispose();
                    _disposed = true;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

    }
}
