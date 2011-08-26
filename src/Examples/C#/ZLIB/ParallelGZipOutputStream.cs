//#define Trace

// ParallelGZipOutputStream.cs
// ------------------------------------------------------------------
//
// A GzipStream that does compression only, and only in output. It uses a
// divide-and-conquer approach with multiple threads to exploit multiple
// CPUs for the DEFLATE computation.
//
// Last Saved: <2011-July-11 14:36:48>
// ------------------------------------------------------------------
//
// Copyright (c) 2009-2011 by Dino Chiesa
// All rights reserved!
//
// ------------------------------------------------------------------
//
// compile: c:\.net4.0\csc.exe /t:module /R:Ionic.Zip.dll @@ORIG@@
// flymake: c:\.net4.0\csc.exe /t:module /R:Ionic.Zip.dll @@FILE@@
//

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Ionic.Zlib;
using System.IO;


namespace Ionic.Exploration
{
    internal class WorkItem
    {
        public byte[] buffer;
        public byte[] compressed;
        public int crc;
        public int index;
        public int ordinal;
        public int inputBytesAvailable;
        public int compressedBytesAvailable;

        public ZlibCodec compressor;

        public WorkItem(int size,
                        Ionic.Zlib.CompressionLevel compressLevel,
                        CompressionStrategy strategy)
        {
            buffer= new byte[size];
            // alloc 5 bytes overhead for every block (margin of safety= 2)
            int n = size + ((size / 32768)+1) * 5 * 2;
            compressed = new byte[n];

            compressor = new ZlibCodec();
            compressor.InitializeDeflate(compressLevel, false);
            compressor.OutputBuffer = compressed;
            compressor.InputBuffer = buffer;
        }
    }

    /// <summary>
    ///   A class for compressing and decompressing streams using the
    ///   Deflate algorithm with multiple threads.
    /// </summary>
    ///
    /// <remarks>
    /// <para>
    ///   This class is for compression only, and that can be only
    ///   through writing.
    /// </para>
    ///
    /// <para>
    ///   For more information on the Deflate algorithm, see IETF RFC 1952,
    ///   "GZIP file format specification version 4.3" http://tools.ietf.org/html/rfc1952
    /// </para>
    ///
    /// <para>
    ///   This class is similar to <see
    ///   cref="System.IO.Compression.GzipStream"/>, except that this
    ///   implementation uses an approach that employs multiple worker threads
    ///   to perform the compression.  On a multi-cpu or multi-core computer,
    ///   the performance of this class can be significantly higher than the
    ///   single-threaded DeflateStream, particularly for larger streams.  How
    ///   large?  In my experience, Anything over 10mb is a good candidate for parallel
    ///   compression.
    /// </para>
    ///
    /// <para>
    ///   The tradeoff is that this class uses more memory and more CPU than the
    ///   vanilla DeflateStream, and also is slightly less efficient as a compressor. For
    ///   large files the size of the compressed data stream can be less than 1%
    ///   larger than the size of a compressed data stream from the vanialla
    ///   DeflateStream.  For smaller files the difference can be larger.  The
    ///   difference will also be larger if you set the BufferSize to be lower
    ///   than the default value.  Your mileage may vary. Finally, for small
    ///   files, the ParallelGZipOutputStream can be much slower than the vanilla
    ///   DeflateStream, because of the overhead of using the thread pool.
    /// </para>
    ///
    /// </remarks>
    /// <seealso cref="Ionic.Zlib.DeflateStream" />
    public class ParallelGZipOutputStream : System.IO.Stream
    {
        private static readonly int IO_BUFFER_SIZE_DEFAULT = 64 * 1024;

        private System.Collections.Generic.List<WorkItem> _pool;
        private bool                        _leaveOpen;
        private bool                        emitting;
        private System.IO.Stream            _outStream;
        private Ionic.Zlib.CRC32            _runningCrc;
        private int                         _currentlyFilling;
        private AutoResetEvent              _newlyCompressedBlob;
        private int                         _lastWritten;
        private int                         _latestCompressed;
        private string                      _comment;
        private string                      _FileName;
        private int                         _lastFilled;
        private int                         _bufferSize;
        private int                         _nBuckets;
        private object                      _latestLock = new object();
        private object                      _outputLock = new object();
        private bool                        _isClosed;
        private bool                        _firstWriteDone;
        private int                         _Crc32;
        private Int64                       _totalBytesProcessed;
        private Ionic.Zlib.CompressionLevel _compressLevel;
        private volatile Exception          _pendingException;
        private object                      _eLock = new Object();  // protects _pendingException
        private BlockingCollection<int>     _toWrite;
        private BlockingCollection<int>     _toCompress;
        private BlockingCollection<int>     _toFill;

        // This bitfield is used only when Trace is defined.

        //private TraceBits _DesiredTrace = TraceBits.All;
        private TraceBits _DesiredTrace = TraceBits.Compress |
            TraceBits.Session |
            TraceBits.WriteTake |
            TraceBits.WriteEnter |
            TraceBits.EmitEnter |
            TraceBits.EmitDone |
            TraceBits.EmitLock |
            TraceBits.EmitSkip |
            TraceBits.EmitBegin;

        /// <summary>
        /// Create a ParallelGZipOutputStream.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   This stream compresses data written into it via the DEFLATE
        ///   algorithm (see RFC 1951), and writes out the compressed byte stream.
        /// </para>
        ///
        /// <para>
        ///   The instance will use the default compression level, the default
        ///   buffer sizes and the default number of threads and buffers per
        ///   thread.
        /// </para>
        ///
        /// <para>
        ///   This class is similar to <see cref="Ionic.Zlib.DeflateStream"/>,
        ///   except that this implementation uses an approach that employs
        ///   multiple worker threads to perform the DEFLATE.  On a multi-cpu or
        ///   multi-core computer, the performance of this class can be
        ///   significantly higher than the single-threaded DeflateStream,
        ///   particularly for larger streams.  How large?  Anything over 10mb is
        ///   a good candidate for parallel compression.
        /// </para>
        ///
        /// </remarks>
        ///
        /// <example>
        ///
        /// This example shows how to use a ParallelGZipOutputStream to compress
        /// data.  It reads a file, compresses it, and writes the compressed data to
        /// a second, output file.
        ///
        /// <code>
        /// byte[] buffer = new byte[WORKING_BUFFER_SIZE];
        /// int n= -1;
        /// String outputFile = fileToCompress + ".compressed";
        /// using (System.IO.Stream input = System.IO.File.OpenRead(fileToCompress))
        /// {
        ///     using (var raw = System.IO.File.Create(outputFile))
        ///     {
        ///         using (Stream compressor = new ParallelGZipOutputStream(raw))
        ///         {
        ///             while ((n= input.Read(buffer, 0, buffer.Length)) != 0)
        ///             {
        ///                 compressor.Write(buffer, 0, n);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// <code lang="VB">
        /// Dim buffer As Byte() = New Byte(4096) {}
        /// Dim n As Integer = -1
        /// Dim outputFile As String = (fileToCompress &amp; ".compressed")
        /// Using input As Stream = File.OpenRead(fileToCompress)
        ///     Using raw As FileStream = File.Create(outputFile)
        ///         Using compressor As Stream = New ParallelGZipOutputStream(raw)
        ///             Do While (n &lt;&gt; 0)
        ///                 If (n &gt; 0) Then
        ///                     compressor.Write(buffer, 0, n)
        ///                 End If
        ///                 n = input.Read(buffer, 0, buffer.Length)
        ///             Loop
        ///         End Using
        ///     End Using
        /// End Using
        /// </code>
        /// </example>
        /// <param name="stream">The stream to which compressed data will be written.</param>
        public ParallelGZipOutputStream(System.IO.Stream stream)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, false)
        {
        }

        /// <summary>
        ///   Create a ParallelDeflateOutputStream using the specified CompressionLevel.
        /// </summary>
        /// <remarks>
        ///   See the <see cref="ParallelDeflateOutputStream(System.IO.Stream)"/>
        ///   constructor for example code.
        /// </remarks>
        /// <param name="stream">The stream to which compressed data will be written.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        public ParallelGZipOutputStream(System.IO.Stream stream, CompressionLevel level)
            : this(stream, level, CompressionStrategy.Default, false)
        {
        }

        /// <summary>
        /// Create a ParallelDeflateOutputStream and specify whether to leave the captive stream open
        /// when the ParallelDeflateOutputStream is closed.
        /// </summary>
        /// <remarks>
        ///   See the <see cref="ParallelDeflateOutputStream(System.IO.Stream)"/>
        ///   constructor for example code.
        /// </remarks>
        /// <param name="stream">The stream to which compressed data will be written.</param>
        /// <param name="leaveOpen">
        ///    true if the application would like the stream to remain open after inflation/deflation.
        /// </param>
        public ParallelGZipOutputStream(System.IO.Stream stream, bool leaveOpen)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
        {
        }

        /// <summary>
        /// Create a ParallelDeflateOutputStream and specify whether to leave the captive stream open
        /// when the ParallelDeflateOutputStream is closed.
        /// </summary>
        /// <remarks>
        ///   See the <see cref="ParallelDeflateOutputStream(System.IO.Stream)"/>
        ///   constructor for example code.
        /// </remarks>
        /// <param name="stream">The stream to which compressed data will be written.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        /// <param name="leaveOpen">
        ///    true if the application would like the stream to remain open after inflation/deflation.
        /// </param>
        public ParallelGZipOutputStream(System.IO.Stream stream, CompressionLevel level, bool leaveOpen)
            : this(stream, CompressionLevel.Default, CompressionStrategy.Default, leaveOpen)
        {
        }

        /// <summary>
        /// Create a ParallelDeflateOutputStream using the specified
        /// CompressionLevel and CompressionStrategy, and specifying whether to
        /// leave the captive stream open when the ParallelDeflateOutputStream is
        /// closed.
        /// </summary>
        /// <remarks>
        ///   See the <see cref="ParallelDeflateOutputStream(System.IO.Stream)"/>
        ///   constructor for example code.
        /// </remarks>
        /// <param name="stream">The stream to which compressed data will be written.</param>
        /// <param name="level">A tuning knob to trade speed for effectiveness.</param>
        /// <param name="strategy">
        ///   By tweaking this parameter, you may be able to optimize the compression for
        ///   data with particular characteristics.
        /// </param>
        /// <param name="leaveOpen">
        ///    true if the application would like the stream to remain open after inflation/deflation.
        /// </param>
        public ParallelGZipOutputStream(System.IO.Stream stream,
                                           CompressionLevel level,
                                           CompressionStrategy strategy,
                                           bool leaveOpen)
        {
            TraceOutput(TraceBits.Lifecycle | TraceBits.Session, "-------------------------------------------------------");
            TraceOutput(TraceBits.Lifecycle | TraceBits.Session, "Create {0:X8}", this.GetHashCode());
            _outStream = stream;
            _compressLevel= level;
            Strategy = strategy;
            _leaveOpen = leaveOpen;

            _nBuckets = 4; // default
            _bufferSize = IO_BUFFER_SIZE_DEFAULT;
        }


        /// <summary>
        ///   The ZLIB strategy to be used during compression.
        /// </summary>
        ///
        public CompressionStrategy Strategy
        {
            get;
            private set;
        }

        /// <summary>
        /// The number of buffers to use.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   This property sets the number of memory buffers to create. This
        ///   sets an upper limit on the amount of memory the stream can use,
        ///   and also the degree of parallelism the stream can employ.
        /// </para>
        ///
        /// <para>
        ///   The divide-and-conquer approach taken by this class assumes a
        ///   single thread from the application will call Write().  There
        ///   will be multiple Tasks that then compress (DEFLATE) the data
        ///   written into the stream. The application's thread aggregates
        ///   those results and emits the compressed output.
        /// </para>
        ///
        /// <para>
        ///   The default value is 4.  Different values may deliver better or
        ///   worse results, depending on the dynamic performance
        ///   characteristics of your storage and compute resources. If you
        ///   have more than 2 CPUs, or more than 3gb memory, you probably
        ///   want to increase this value.
        /// </para>
        ///
        /// <para>
        ///   The total amount of storage space allocated for buffering will
        ///   be (M*S*2), where M is the multiple (this property), S is the
        ///   size of each buffer (<see cref="BufferSize"/>). There are 2
        ///   buffers used by the compressor, one for input and one for
        ///   output. If you retain the default values for Buckets (4), and
        ///   BufferSize (64k), then the ParallelDeflateOutputStream will use
        ///   512kb of buffer memory in total.
        /// </para>
        ///
        /// <para>
        ///   The application can set this value at any time, but it is effective
        ///   only before the first call to Write(), which is when the buffers are
        ///   allocated.
        /// </para>
        /// </remarks>
        public int Buckets
        {
            get
            {
                return _nBuckets;
            }
            set
            {
                if (value < 1 || value > 10240)
                    throw new ArgumentOutOfRangeException("Buckets",
                                                          "Buckets must be between 1 and 10240");
                _nBuckets = value;
                TraceOutput(TraceBits.Instance, "Buckets   {0}", _nBuckets);
            }
        }

        /// <summary>
        ///   The size of the buffers used by the compressor threads.
        /// </summary>
        /// <remarks>
        ///
        /// <para>
        ///   The default buffer size is 128k. The application can set
        ///   this value at any time, but it is effective only before
        ///   the first Write().
        /// </para>
        ///
        /// <para>
        ///   Larger buffer sizes implies larger memory consumption but allows
        ///   more efficient compression. Using smaller buffer sizes consumes less
        ///   memory but result in less effective compression.  For example, using
        ///   the default buffer size of 128k, the compression delivered is within
        ///   1% of the compression delivered by the single-threaded <see
        ///   cref="Ionic.Zlib.DeflateStream"/>.  On the other hand, using a
        ///   BufferSize of 8k can result in a compressed data stream that is 5%
        ///   larger than that delivered by the single-threaded
        ///   <c>DeflateStream</c>.  Excessively small buffer sizes can also cause
        ///   the speed of the ParallelDeflateOutputStream to drop, because of
        ///   larger thread scheduling overhead dealing with many many small
        ///   buffers.
        /// </para>
        ///
        /// <para>
        ///   The total amount of storage space allocated for buffering will be
        ///   (n*M*S*2), where n is the number of CPUs, M is the multiple (<see
        ///   cref="BuffersPerCore"/>), S is the size of each buffer (this
        ///   property), and there are 2 buffers used by the compressor, one for
        ///   input and one for output. For example, if your machine has a total
        ///   of 4 cores, and if you set <see cref="BuffersPerCore"/> to 3, and
        ///   you keep the default buffer size of 128k, then the
        ///   <c>ParallelDeflateOutputStream</c> will use 3mb of buffer memory in
        ///   total.
        /// </para>
        ///
        /// </remarks>
        public int BufferSize
        {
            get { return _bufferSize;}
            set
            {
                if (value < 1024)
                    throw new ArgumentOutOfRangeException("BufferSize",
                                                          "BufferSize must be greater than 1024 bytes");
                _bufferSize = value;
                TraceOutput(TraceBits.Instance, "BufferSize   {0}", _bufferSize);
            }
        }

        /// <summary>
        /// The CRC32 for the pre-compressed data that was written through the stream.
        /// </summary>
        /// <remarks>
        /// This value is meaningful only after a call to Close().
        /// </remarks>
        public int Crc32 { get { return _Crc32; } }


        /// <summary>
        /// The total number of uncompressed bytes processed by the ParallelGZipOutputStream.
        /// </summary>
        /// <remarks>
        /// This value is meaningful only after a call to Close().
        /// </remarks>
        public Int64 BytesProcessed { get { return _totalBytesProcessed; } }


        /// <summary>
        ///   The comment on the GZIP stream.
        /// </summary>
        ///
        /// <remarks>
        /// <para>
        ///   The GZIP format allows for each file to optionally have an associated
        ///   comment stored with the file.  The comment is encoded with the ISO-8859-1
        ///   code page.  To include a comment in a GZIP stream you create, set this
        ///   property before calling <c>Write()</c> for the first time on the
        ///   <c>ParallelGZipOutputStream</c>.
        /// </para>
        ///
        /// </remarks>
        public String Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                if (_firstWriteDone)
                    throw new InvalidOperationException();

                _comment = value;
            }
        }


        /// <summary>
        ///   The FileName for the GZIP stream.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   The GZIP format optionally allows each compressed file to embed an
        ///   associated filename. This property holds that value.  Set this
        ///   property before calling <c>Write()</c> the first time on the
        ///   <c>ParallelGZipOutputStream</c>.  The actual filename is encoded
        ///   into the GZIP bytestream with the ISO-8859-1 code page, according
        ///   to RFC 1952. It is the application's responsibility to insure that
        ///   the FileName can be encoded and decoded correctly with this code
        ///   page.
        /// </para>
        ///
        /// <para>
        ///   The value of this property is merely written into the GZIP output.
        ///   There is nothing in this class that verifies that the value you
        ///   set here is consistent with any filesystem file the compressed
        ///   data eventually written to, if any.
        /// </para>
        /// </remarks>
        public String FileName
        {
            get { return _FileName; }
            set
            {
                if (_firstWriteDone)
                    throw new InvalidOperationException();

                _FileName = value;
                if (_FileName == null) return;
                if (_FileName.IndexOf("/") != -1)
                    _FileName = _FileName.Replace("/", "\\");

                if (_FileName.EndsWith("\\"))
                    throw new ArgumentException("FileName", "The FileName property may not end in slash.");

                if (_FileName.IndexOf("\\") != -1)
                    _FileName = Path.GetFileName(_FileName);
            }
        }


        /// <summary>
        ///   The last modified time for the GZIP stream.
        /// </summary>
        ///
        /// <remarks>
        ///   GZIP allows the storage of a last modified time with each GZIP entry.
        ///   When compressing data, you must set this before the first call to
        ///   <c>Write()</c>, in order for it to be written to the output stream.
        /// </remarks>
        public DateTime? LastModified;


        private void _TakeAndCompress()
        {
            var rnd = new System.Random();

            while (!_toCompress.IsCompleted)
            {
                WorkItem workitem = null;
                int ix = -1;
                try
                {
                    ix = _toCompress.Take();
                    workitem = _pool[ix];
                }
                catch (InvalidOperationException)
                {
                    // The collection has been completed.
                    // Some other thread has called CompleteAdding()
                    // after this thread passed the
                    // IsCompleted check.
                }
                if (workitem == null) continue;

                try
                {
                    TraceOutput(TraceBits.Compress,
                                "Compress lock     wi({0}) ord({1})",
                                workitem.index,
                                workitem.ordinal);

                    // compress one buffer
                    Ionic.Zlib.CRC32 crc = new CRC32();
                    int ib = workitem.inputBytesAvailable;
                    crc.SlurpBlock(workitem.buffer, 0, workitem.inputBytesAvailable);
                    DeflateOneSegment(workitem);
                    workitem.crc = crc.Crc32Result;
                    TraceOutput(TraceBits.Compress,
                                "Compress done     wi({0}) ord({1}) ib-({2}) cba({3})",
                                workitem.index,
                                workitem.ordinal,
                                ib,
                                workitem.compressedBytesAvailable
                                );

                    lock(_latestLock)
                    {
                        if (workitem.ordinal > _latestCompressed)
                            _latestCompressed = workitem.ordinal;
                    }

                    _toWrite.Add(workitem.index);
                    _newlyCompressedBlob.Set();
                }
                catch (System.Exception exc1)
                {
                    lock(_eLock)
                    {
                        // expose the exception to the main thread
                        if (_pendingException!=null)
                            _pendingException = exc1;
                    }
                }
            }
        }



        private void _InitializeBuffers()
        {
            _toCompress = new BlockingCollection<int>(Buckets);
            _toFill = new BlockingCollection<int>(Buckets);
            _toWrite = new BlockingCollection<int>(new ConcurrentQueue<int>());
            _pool = new System.Collections.Generic.List<WorkItem>();
            for(int i=0; i < Buckets; i++)
            {
                _pool.Add(new WorkItem(_bufferSize, _compressLevel, Strategy));
                _toFill.Add(i);

                // Start one perpetual compressor task per bucket.
                Task.Factory.StartNew( _TakeAndCompress );
            }

            // for diagnostic purposes only
            for(int i=0; i < _pool.Count; i++)
                _pool[i].index= i;

            _newlyCompressedBlob = new AutoResetEvent(false);
            _runningCrc = new Ionic.Zlib.CRC32();
            _currentlyFilling = -1;
            _lastFilled = -1;
            _lastWritten = -1;
            _latestCompressed = -1;
        }


        internal static readonly System.DateTime _unixEpoch = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static readonly System.Text.Encoding iso8859dash1 = System.Text.Encoding.GetEncoding("iso-8859-1");


        private int EmitHeader()
        {
            byte[] commentBytes = (Comment == null) ? null : iso8859dash1.GetBytes(Comment);
            byte[] filenameBytes = (FileName == null) ? null : iso8859dash1.GetBytes(FileName);

            int cbLength = (Comment == null) ? 0 : commentBytes.Length + 1;
            int fnLength = (FileName == null) ? 0 : filenameBytes.Length + 1;

            int bufferLength = 10 + cbLength + fnLength;
            byte[] header = new byte[bufferLength];
            int i = 0;
            // ID
            header[i++] = 0x1F;
            header[i++] = 0x8B;

            // compression method
            header[i++] = 8;
            byte flag = 0;
            if (Comment != null)
                flag ^= 0x10;
            if (FileName != null)
                flag ^= 0x8;

            // flag
            header[i++] = flag;

            // mtime
            if (!LastModified.HasValue) LastModified = DateTime.Now;
            System.TimeSpan delta = LastModified.Value - _unixEpoch;
            Int32 timet = (Int32)delta.TotalSeconds;
            Array.Copy(BitConverter.GetBytes(timet), 0, header, i, 4);
            i += 4;

            // xflg
            header[i++] = 0;    // this field is totally useless
            // OS
            header[i++] = 0xFF; // 0xFF == unspecified

            // extra field length - only if FEXTRA is set, which it is not.
            //header[i++]= 0;
            //header[i++]= 0;

            // filename
            if (fnLength != 0)
            {
                Array.Copy(filenameBytes, 0, header, i, fnLength - 1);
                i += fnLength - 1;
                header[i++] = 0; // terminate
            }

            // comment
            if (cbLength != 0)
            {
                Array.Copy(commentBytes, 0, header, i, cbLength - 1);
                i += cbLength - 1;
                header[i++] = 0; // terminate
            }

            _outStream.Write(header, 0, header.Length);

            return header.Length; // bytes written
        }



        private void _EmitPendingBuffers(bool doAll, bool mustWait)
        {
            // When combining parallel deflation with a ZipSegmentedStream, it's
            // possible for the ZSS to throw from within this method.  In that
            // case, Close/Dispose will be called on this stream, if this stream
            // is employed within a using or try/finally pair as required. But
            // this stream is unaware of the pending exception, so the Close()
            // method invokes this method AGAIN.  This can lead to a deadlock.
            // Therefore, failfast if re-entering.

            if (emitting) return;
            emitting = true;

            if (doAll || mustWait)
                _newlyCompressedBlob.WaitOne();

            do
            {
                int firstSkip = -1;
                int millisecondsToWait = doAll ? 200 : (mustWait ? -1 : 0);
                int nextToWrite;

                while (_toWrite.TryTake(out nextToWrite, millisecondsToWait))
                {
                    WorkItem workitem = _pool[nextToWrite];
                    if (workitem.ordinal != _lastWritten + 1)
                    {
                        // not the needed ordinal, so requeue and try again.
                        TraceOutput(TraceBits.EmitSkip,
                                    "Emit     skip     wi({0}) ord({1}) lw({2}) fs({3})",
                                    workitem.index,
                                    workitem.ordinal,
                                    _lastWritten,
                                    firstSkip);

                        _toWrite.Add(nextToWrite);

                        if (firstSkip == nextToWrite)
                        {
                            // We went around the list once.
                            // None of the items in the list is the one we want.
                            // Now wait for a compressor to signal.
                            _newlyCompressedBlob.WaitOne();
                            firstSkip = -1;
                        }
                        else if (firstSkip == -1)
                            firstSkip = nextToWrite;

                        continue;
                    }

                    firstSkip = -1;

                    TraceOutput(TraceBits.EmitBegin,
                                "Emit     begin    wi({0}) ord({1})              cba({2})",
                                workitem.index,
                                workitem.ordinal,
                                workitem.compressedBytesAvailable);

                    _outStream.Write(workitem.compressed, 0, workitem.compressedBytesAvailable);
                    _runningCrc.Combine(workitem.crc, workitem.inputBytesAvailable);
                    _totalBytesProcessed += workitem.inputBytesAvailable;
                    workitem.inputBytesAvailable= 0;

                    TraceOutput(TraceBits.EmitDone,
                                "Emit     done     wi({0}) ord({1})              cba({2}) mtw({3})",
                                workitem.index,
                                workitem.ordinal,
                                workitem.compressedBytesAvailable,
                                millisecondsToWait);

                    _lastWritten = workitem.ordinal;
                    _toFill.Add(workitem.index);

                    // don't wait next time through
                    if (millisecondsToWait == -1) millisecondsToWait = 0;
                }

            } while (doAll &&
                     !_toCompress.IsCompleted &&
                     (_lastWritten != _latestCompressed));

            emitting = false;
        }





        /// <summary>
        ///   Write data to the stream.
        /// </summary>
        ///
        /// <remarks>
        ///
        /// <para>
        ///   To use the ParallelGZipOutputStream to compress data, create a
        ///   ParallelGZipOutputStream, passing a writable output stream.
        ///   Then call Write() on that ParallelGZipOutputStream, providing
        ///   uncompressed data as input. The data sent to the output stream
        ///   will be the compressed form of the data written into the stream.
        /// </para>
        ///
        /// <para>
        ///   To decompress data, use the <see cref="Ionic.Zlib.DeflateStream"/> class.
        ///   Any RFC-1951
        /// </para>
        ///
        /// </remarks>
        /// <param name="buffer">The buffer holding data to write to the stream.</param>
        /// <param name="offset">the offset within that data array to find the first byte to write.</param>
        /// <param name="count">the number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            bool wantWaitEmit = false;
            if (_isClosed)
                throw new InvalidOperationException();

            // dispense any exception that occurred on the BG threads
            if (_pendingException != null)
                throw _pendingException;

            if (count == 0) return; // NOP

            TraceOutput(TraceBits.WriteEnter, "Write    enter");

            if (!_firstWriteDone)
            {
                _InitializeBuffers();
                _firstWriteDone = true;
                EmitHeader();
            }

            do
            {
                // may need to make buffers available
                _EmitPendingBuffers(false, wantWaitEmit);

                wantWaitEmit = false;
                // use current, or get a buffer to fill
                int ix = -1;
                if (_currentlyFilling >= 0)
                {
                    ix = _currentlyFilling;
                    TraceOutput(TraceBits.WriteTake,
                                "Write    notake   wi({0}) lf({1})",
                                ix,
                                _lastFilled);
                }
                else
                {
                    TraceOutput(TraceBits.WriteTake, "Write    take?");
                    if (!_toFill.TryTake(out ix, 0))
                    {
                        // no available buffers, so... need to emit
                        // compressed buffers.
                        wantWaitEmit = true;
                        continue;
                    }

                    TraceOutput(TraceBits.WriteTake,
                                "Write    take     wi({0}) lf({1})",
                                ix,
                                _lastFilled);
                    ++_lastFilled; // TODO: consider rollover?
                }

                WorkItem workitem = _pool[ix];

                int limit = ((workitem.buffer.Length - workitem.inputBytesAvailable) > count)
                    ? count
                    : (workitem.buffer.Length - workitem.inputBytesAvailable);

                workitem.ordinal = _lastFilled;

                TraceOutput(TraceBits.Write,
                            "Write    lock     wi({0}) ord({1}) iba({2})",
                            workitem.index,
                            workitem.ordinal,
                            workitem.inputBytesAvailable );

                // copy from the provided buffer to our workitem, starting at
                // the tail end of whatever data we might have in there currently.
                Array.Copy(buffer,
                           offset,
                           workitem.buffer,
                           workitem.inputBytesAvailable,
                           limit);

                count -= limit;
                offset += limit;
                workitem.inputBytesAvailable += limit;

                if (workitem.inputBytesAvailable==workitem.buffer.Length)
                {
                    TraceOutput(TraceBits.Write,
                                "Write    full     wi({0}) ord({1}) iba({2})",
                                workitem.index,
                                workitem.ordinal,
                                workitem.inputBytesAvailable );
                    _toCompress.Add(ix);
                    _currentlyFilling = -1; // will get a new buffer next time
                }
                else
                {
                    _currentlyFilling = ix;
                }

                if (count > 0)
                    TraceOutput(TraceBits.WriteEnter, "Write    more");
            }
            while (count > 0);  // until no more to write

            TraceOutput(TraceBits.WriteEnter, "Write    exit");
            return;
        }



        private void _FlushFinish()
        {
            // After writing a series of compressed buffers, each one closed
            // with Flush.Sync, we now write the final one as Flush.Finish,
            // and then stop.
            byte[] buffer = new byte[128];
            var compressor = new ZlibCodec();
            int rc = compressor.InitializeDeflate(_compressLevel, false);
            compressor.InputBuffer = null;
            compressor.NextIn = 0;
            compressor.AvailableBytesIn = 0;
            compressor.OutputBuffer = buffer;
            compressor.NextOut = 0;
            compressor.AvailableBytesOut = buffer.Length;
            rc = compressor.Deflate(FlushType.Finish);

            if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
                throw new Exception("deflating: " + compressor.Message);

            if (buffer.Length - compressor.AvailableBytesOut > 0)
            {
                TraceOutput(TraceBits.EmitBegin,
                            "Emit     begin    flush bytes({0})",
                            buffer.Length - compressor.AvailableBytesOut);

                _outStream.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);

                TraceOutput(TraceBits.EmitDone,
                            "Emit     done     flush");
            }

            compressor.EndDeflate();

            _Crc32 = _runningCrc.Crc32Result;
        }


        private void _EmitTrailer()
        {
            // Emit the GZIP trailer: CRC32 and  size mod 2^32
            _outStream.Write(BitConverter.GetBytes(_runningCrc.Crc32Result), 0, 4);

            int c2 = (Int32)(_totalBytesProcessed & 0x00000000FFFFFFFF);
            _outStream.Write(BitConverter.GetBytes(c2), 0, 4);
        }


        private void _Flush(bool lastInput)
        {
            if (_isClosed)
                throw new InvalidOperationException();

            // post the current partial buffer to the _toCompress queue
            if (_currentlyFilling>=0)
            {
                _toCompress.Add(_currentlyFilling);
                TraceOutput(TraceBits.Flush,
                            "Flush    filled   wi({0})",
                            _currentlyFilling);

                _currentlyFilling = -1; // get a new buffer next Write()
            }

            if (lastInput)
            {
                //_toWrite.CompleteAdding(); // cannot do because of sifting
                _toCompress.CompleteAdding();
                _EmitPendingBuffers(true, false);
                _FlushFinish();
                _EmitTrailer();
            }
            else
            {
                _EmitPendingBuffers(false, false);
            }
        }



        /// <summary>
        ///   Flush the stream.
        /// </summary>
        public override void Flush()
        {
            _Flush(false);
        }



        /// <summary>
        ///   Close the stream.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     The application must call Close() on this stream to guarantee
        ///     that all of the data written in has been compressed, and the
        ///     compressed data has been written out.
        ///   </para>
        ///   <para>
        ///     Close() is called implicitly when this stream is used within
        ///     a using clause.
        ///   </para>
        /// </remarks>
        public override void Close()
        {
            TraceOutput(TraceBits.Session, "Close {0:X8}", this.GetHashCode());

            if (_isClosed) return;

            _Flush(true);

            if (!_leaveOpen)
                _outStream.Close();

            _isClosed= true;
        }



        /// <summary>Dispose the object</summary>
        /// <remarks>
        ///   <para>
        ///     Because ParallelDeflateOutputStream is IDisposable, the
        ///     application must call this method when finished using the instance.
        ///   </para>
        ///   <para>
        ///     This method is generally called implicitly upon exit from
        ///     a <c>using</c> scope in C# (<c>Using</c> in VB).
        ///   </para>
        /// </remarks>
        new public void Dispose()
        {
            TraceOutput(TraceBits.Lifecycle, "Dispose  {0:X8}", this.GetHashCode());
            _pool = null;
            Close();
            Dispose(true);
        }



        /// <summary>The Dispose method</summary>
        protected override void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                // dispose managed resources
            }
        }



        private bool DeflateOneSegment(WorkItem workitem)
        {
            ZlibCodec compressor = workitem.compressor;
            int rc= 0;
            compressor.ResetDeflate();
            compressor.NextIn = 0;

            compressor.AvailableBytesIn = workitem.inputBytesAvailable;

            // step 1: deflate the buffer
            compressor.NextOut = 0;
            compressor.AvailableBytesOut =  workitem.compressed.Length;
            do
            {
                compressor.Deflate(FlushType.None);
            }
            while (compressor.AvailableBytesIn > 0 || compressor.AvailableBytesOut == 0);

            // step 2: flush (sync)
            rc = compressor.Deflate(FlushType.Sync);

            workitem.compressedBytesAvailable = (int) compressor.TotalBytesOut;
            return true;
        }



        [System.Diagnostics.ConditionalAttribute("Trace")]
        private void TraceOutput(TraceBits bits, string format, params object[] varParams)
        {
            if ((bits & _DesiredTrace) != 0)
            {
                lock(_outputLock)
                {
                    int tid = Thread.CurrentThread.GetHashCode();
#if !SILVERLIGHT
                    Console.ForegroundColor = (ConsoleColor) (tid % 8 + 8);
#endif
                    Console.Write("{0:000} PGOS ", tid);
                    Console.WriteLine(format, varParams);
#if !SILVERLIGHT
                    Console.ResetColor();
#endif
                }
            }
        }



        // used only when Trace is defined
        [Flags]
        enum TraceBits : uint
        {
            None         = 0,
            NotUsed1     = 1,
            EmitLock     = 2,
            EmitEnter    = 4,    // enter _EmitPending
            EmitBegin    = 8,    // begin to write out
            EmitDone     = 16,   // done writing out
            EmitSkip     = 32,   // writer skipping a workitem
            EmitAll      = 58,   // All Emit flags
            Flush        = 64,
            Lifecycle    = 128,  // constructor/disposer
            Session      = 256,  // Close/Reset
            Synch        = 512,  // thread synchronization
            Instance     = 1024, // instance settings
            Compress     = 2048,  // compress task
            Write        = 4096,    // filling buffers, when caller invokes Write()
            WriteEnter   = 8192,    // upon entry to Write()
            WriteTake    = 16384,    // on _toFill.Take()
            All          = 0xffffffff,
        }



        /// <summary>
        /// Indicates whether the stream supports Seek operations.
        /// </summary>
        /// <remarks>
        /// Always returns false.
        /// </remarks>
        public override bool CanSeek
        {
            get { return false; }
        }


        /// <summary>
        /// Indicates whether the stream supports Read operations.
        /// </summary>
        /// <remarks>
        /// Always returns false.
        /// </remarks>
        public override bool CanRead
        {
            get {return false;}
        }

        /// <summary>
        /// Indicates whether the stream supports Write operations.
        /// </summary>
        /// <remarks>
        /// Returns true if the provided stream is writable.
        /// </remarks>
        public override bool CanWrite
        {
            get { return _outStream.CanWrite; }
        }

        /// <summary>
        /// Reading this property always throws a NotSupportedException.
        /// </summary>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        ///   Writing this property always throws a NotSupportedException.
        ///   On Read, the value is the number of bytes written so far to the
        ///   output.
        /// </summary>
        /// <seealso cref="TotalBytesProcessed" />
        public override long Position
        {
            get { return _outStream.Position; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// This method always throws a NotSupportedException.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method always throws a NotSupportedException.
        /// </summary>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method always throws a NotSupportedException.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}


