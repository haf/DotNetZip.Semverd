using System;
using Ionic.Zlib;

// Test deflate() with full flush
class test_flush_sync
{
                
    [STAThread]
    public static void  Main(System.String[] args)
    {
        try
        {
            var x = new test_flush_sync();
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
        int comprLen = 40000;
        int uncomprLen = comprLen;
        byte[] CompressedBytes = new byte[comprLen];
        byte[] DecompressedBytes = new byte[uncomprLen];
        string TextToCompress = "This is the text that will be compressed.";
        byte[] BytesToCompress = System.Text.ASCIIEncoding.ASCII.GetBytes(TextToCompress);

        ZlibCodec compressor = new ZlibCodec(CompressionMode.Compress);

        compressor.InputBuffer = BytesToCompress;
        compressor.NextIn = 0;
        compressor.OutputBuffer = CompressedBytes;
        compressor.NextOut = 0;
        compressor.AvailableBytesIn = 3;
        compressor.AvailableBytesOut = CompressedBytes.Length;
                
        rc = compressor.Deflate(ZlibConstants.Z_FULL_FLUSH);
        CheckForError(compressor, rc, "Deflate");
                
        CompressedBytes[3]++; // force an error in first compressed block // dinoch 
        compressor.AvailableBytesIn = TextToCompress.Length - 3;

        rc = compressor.Deflate(ZlibConstants.Z_FINISH);
        if (rc != ZlibConstants.Z_STREAM_END)
        {
            CheckForError(compressor, rc, "Deflate");
        }
        rc = compressor.EndDeflate();
        CheckForError(compressor, rc, "EndDeflate");
        comprLen = (int) (compressor.TotalBytesOut);
                
        ZlibCodec decompressor = new ZlibCodec(CompressionMode.Decompress);
                
        decompressor.InputBuffer = CompressedBytes;
        decompressor.NextIn = 0;
        decompressor.AvailableBytesIn = 2;

        decompressor.OutputBuffer = DecompressedBytes;
        decompressor.NextOut = 0;
        decompressor.AvailableBytesOut = DecompressedBytes.Length;

        rc = decompressor.Inflate(ZlibConstants.Z_NO_FLUSH);
        CheckForError(decompressor, rc, "Inflate");
                
        decompressor.AvailableBytesIn = CompressedBytes.Length - 2;

        rc = decompressor.SyncInflate();
        CheckForError(decompressor, rc, "SyncInflate");

        bool gotException = false;
        try
        {
            rc = decompressor.Inflate(ZlibConstants.Z_FINISH);
        }
        catch (ZlibException ex1)
        {
            Console.WriteLine("Got Expected Exception: " + ex1);
            gotException = true;
        }

        if (!gotException)
        {
            System.Console.Out.WriteLine("inflate should report DATA_ERROR");
            /* Because of incorrect adler32 */
            System.Environment.Exit(1);
        }
                
        rc = decompressor.EndInflate();
        CheckForError(decompressor, rc, "EndInflate");
                
        int j = 0;
        for (; j < DecompressedBytes.Length; j++)
            if (DecompressedBytes[j] == 0)
                break;

        var result = System.Text.ASCIIEncoding.ASCII.GetString(DecompressedBytes, 0, j);

        Console.WriteLine("orig length: {0}", TextToCompress.Length);
        Console.WriteLine("compressed length: {0}", compressor.TotalBytesOut);
        Console.WriteLine("uncompressed length: {0}", decompressor.TotalBytesOut);
        Console.WriteLine("result length: {0}", result.Length);
        Console.WriteLine("result of inflate:\n(Thi){0}", result);
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