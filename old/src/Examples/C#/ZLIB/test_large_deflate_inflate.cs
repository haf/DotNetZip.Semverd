using System;
using Ionic.Zlib;

// Test deflate() with large buffers and dynamic change of compression level
class test_large_deflate_inflate
{
    [STAThread]
    public static void  Main(System.String[] arg)
    {
        try
        {
            var x = new test_large_deflate_inflate();
            x.Run();
        }
        catch (System.Exception e1)
        {
            Console.WriteLine("Exception: " + e1);
        }
    }

    private void Run()
    {
        int rc;
        int j;
        int bufferSize = 40000;
        byte[] compressedBytes = new byte[bufferSize];
        byte[] bufferToCompress= new byte[bufferSize];
        byte[] decompressedBytes = new byte[bufferSize];

        ZlibCodec compressingStream= new ZlibCodec();

        rc = compressingStream.InitializeDeflate(CompressionLevel.BestSpeed);
        CheckForError(compressingStream, rc, "InitializeDeflate");

        compressingStream.OutputBuffer = compressedBytes;
        compressingStream.NextOut = 0;
        compressingStream.AvailableBytesOut = compressedBytes.Length;

        // At this point, bufferToCompress is all zeroes, so it should compress
        // very well:
        compressingStream.InputBuffer = bufferToCompress;
        compressingStream.AvailableBytesIn = bufferToCompress.Length;
        rc = compressingStream.Deflate(FlushType.None);
        CheckForError(compressingStream, rc, "deflate");
        if (compressingStream.AvailableBytesIn != 0)
        {
            System.Console.Out.WriteLine("deflate not greedy");
            System.Environment.Exit(1);
        }

        Console.WriteLine("Stage 1: uncompressed bytes in so far:  {0,6}", compressingStream.TotalBytesIn);
        Console.WriteLine("          compressed bytes out so far:  {0,6}", compressingStream.TotalBytesOut);            


        // Feed in already compressed data and switch to no compression:
        compressingStream.SetDeflateParams(CompressionLevel.None, CompressionStrategy.Default);
        compressingStream.InputBuffer = compressedBytes;
        compressingStream.NextIn = 0;
        compressingStream.AvailableBytesIn = bufferSize / 2; // why? - for fun
        rc = compressingStream.Deflate(FlushType.None);
        CheckForError(compressingStream, rc, "Deflate");

        Console.WriteLine("Stage 2: uncompressed bytes in so far:  {0,6}", compressingStream.TotalBytesIn);
        Console.WriteLine("          compressed bytes out so far:  {0,6}", compressingStream.TotalBytesOut);

        // Insert data into bufferToCompress, and Switch back to compressing mode:
        System.Random rnd = new Random();

        for (int i = 0; i < bufferToCompress.Length / 1000; i++)
        {
            byte b = (byte) rnd.Next();
            int n = 500 + rnd.Next(500);
            for (j = 0; j < n; j++)
                bufferToCompress[j + i] = b;
            i += j-1;
        }

        compressingStream.SetDeflateParams(CompressionLevel.BestCompression, CompressionStrategy.Filtered);
        compressingStream.InputBuffer = bufferToCompress;
        compressingStream.NextIn = 0;
        compressingStream.AvailableBytesIn = bufferToCompress.Length;
        rc = compressingStream.Deflate(FlushType.None);
        CheckForError(compressingStream, rc, "Deflate");

        Console.WriteLine("Stage 3: uncompressed bytes in so far:  {0,6}", compressingStream.TotalBytesIn);
        Console.WriteLine("          compressed bytes out so far:  {0,6}", compressingStream.TotalBytesOut);

        rc = compressingStream.Deflate(FlushType.Finish);
        if (rc != ZlibConstants.Z_STREAM_END)
        {
            Console.WriteLine("deflate reported {0}, should report Z_STREAM_END", rc);
            Environment.Exit(1);
        }
        rc = compressingStream.EndDeflate();
        CheckForError(compressingStream, rc, "EndDeflate");

        Console.WriteLine("Stage 4: uncompressed bytes in (final): {0,6}", compressingStream.TotalBytesIn);
        Console.WriteLine("          compressed bytes out (final): {0,6}", compressingStream.TotalBytesOut);

        ZlibCodec decompressingStream = new ZlibCodec(CompressionMode.Decompress);
                
        decompressingStream.InputBuffer = compressedBytes;
        decompressingStream.NextIn = 0;
        decompressingStream.AvailableBytesIn = bufferSize;
                                
        // upon inflating, we overwrite the decompressedBytes buffer repeatedly
        while (true)
        {
            decompressingStream.OutputBuffer = decompressedBytes;
            decompressingStream.NextOut = 0;
            decompressingStream.AvailableBytesOut = decompressedBytes.Length;
            rc = decompressingStream.Inflate(FlushType.None);
            if (rc == ZlibConstants.Z_STREAM_END)
                break;
            CheckForError(decompressingStream, rc, "inflate large");
        }
                
        rc = decompressingStream.EndInflate();
        CheckForError(decompressingStream, rc, "EndInflate");

        if (decompressingStream.TotalBytesOut != 2 * decompressedBytes.Length + bufferSize / 2)
        {
            System.Console.WriteLine("bad large inflate: " + decompressingStream.TotalBytesOut);
            System.Environment.Exit(1);
        }

        for (j = 0; j < decompressedBytes.Length; j++)
            if (decompressedBytes[j] == 0)
                break;

        Console.WriteLine("compressed length: {0}", compressingStream.TotalBytesOut);
        Console.WriteLine("decompressed length (expected): {0}", 2 * decompressedBytes.Length + bufferSize / 2);
        Console.WriteLine("decompressed length (actual)  : {0}", decompressingStream.TotalBytesOut);
    }
        
    internal static void  CheckForError(ZlibCodec z, int rc, System.String msg)
    {
        if (rc != ZlibConstants.Z_OK)
        {
            if (z.Message != null)
                System.Console.Out.Write(z.Message + " ");
            System.Console.Out.WriteLine(msg + " error: " + rc);
                        
            System.Environment.Exit(1);
        }
    }
}