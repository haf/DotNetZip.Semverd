// RandomTextGenerator.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// last saved (in emacs):
// Time-stamp: <2011-July-13 16:37:19>
//
// ------------------------------------------------------------------
//
// This module defines a class that generates random byte sequences
//
// ------------------------------------------------------------------

using System;
using System.IO;

namespace Ionic.Zip.Tests.Utilities
{
    public class RandomBytesInputStream : Stream
    {
        Int64 _desiredLength;
        Int64 _bytesRead;
        System.Random _rnd;

        public RandomBytesInputStream(Int64 length)
        {
            _desiredLength = length;
            _rnd = new System.Random();
        }

        new public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>The Dispose method</summary>
        protected override void Dispose(bool disposeManagedResources)
        {
        }

        public Int64 BytesRead
        {
            get { return _bytesRead; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_desiredLength - _bytesRead < count)
            {
                int bytesToReadThisTime = unchecked((int)(_desiredLength - _bytesRead));
                var buf = new byte[bytesToReadThisTime];
                _rnd.NextBytes(buf);
                Array.Copy(buf, buffer, bytesToReadThisTime);
                _bytesRead += bytesToReadThisTime;
                return bytesToReadThisTime;
            }
            else
            {
                _bytesRead += count;
                _rnd.NextBytes(buffer);
                return count;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }
        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return _desiredLength; }
        }

        public override long Position
        {
            get { return _desiredLength - _bytesRead; }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            if (value < _bytesRead)
                throw new NotSupportedException();
            _desiredLength = value;
        }

        public override void Flush()
        {
        }
    }
}