using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace PpmSharp.Models;

public class PpmImage
{
    public int Width { get; }
    public int Height { get; }
    public byte[] Pixels { get; }

    public PpmImage(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 8192);
        using var br = new BinaryReader(fs);

        var magic = ReadToken(br);
        if (magic != "P3")
            throw new Exception("Unsupported PPM format");

        var width = int.Parse(ReadToken(br));
        var height = int.Parse(ReadToken(br));
        var maxVal = int.Parse(ReadToken(br));
        
        if (maxVal is < 1 or > 65535)
            throw new Exception("Unsupported max color value");

        Width = width;
        Height = height;
        Pixels = new byte[width * height];

        ReadAsciiData(br, maxVal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadAsciiData(BinaryReader br, int maxVal)
    {
        var pixels = Pixels.AsSpan();
        
        if (maxVal == 255)
        {
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (byte)ParseNextInt(br);
            }
        }
        else
        {
            var scale = 255.0f / maxVal;
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (byte)(ParseNextInt(br) * scale);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ParseNextInt(BinaryReader br)
    {
        byte b;
        do
        {
            b = br.ReadByte();
            if (b != '#') continue;
            while (br.ReadByte() != '\n') { }
            b = (byte)' '; // Treat as whitespace
        } while (IsWhitespace(b));

        var value = b - '0';
        
        while (true)
        {
            if (br.BaseStream.Position >= br.BaseStream.Length)
                break;
                
            b = br.ReadByte();
            
            if (IsWhitespace(b))
                break;
                
            value = value * 10 + (b - '0');
        }

        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsWhitespace(byte b)
    {
        return b == ' ' || b == '\t' || b == '\n' || b == '\r';
    }

    private static string ReadToken(BinaryReader br)
    {
        var buffer = ArrayPool<char>.Shared.Rent(64);
        var length = 0;

        try
        {
            byte b;

            do
            {
                b = br.ReadByte();
                if (b != '#') continue;
                // Skip entire line
                while (br.ReadByte() != '\n') { }
                b = (byte)' ';
            } while (IsWhitespace(b));

            do
            {
                if (b != '#')
                {
                    if (length >= buffer.Length)
                    {
                        var newBuffer = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                        Array.Copy(buffer, newBuffer, length);
                        ArrayPool<char>.Shared.Return(buffer);
                        buffer = newBuffer;
                    }
                    buffer[length++] = (char)b;
                }
                
                if (br.BaseStream.Position >= br.BaseStream.Length)
                    break;
                    
                b = br.ReadByte();
            } while (!IsWhitespace(b));

            return new string(buffer, 0, length);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}

// Alternative: Memory-mapped file version for very large images
public class PpmImageMemoryMapped
{
    public int Width { get; }
    public int Height { get; }
    public byte[] Pixels { get; }

    public PpmImageMemoryMapped(string path)
    {
        var fileInfo = new FileInfo(path);
        
        if (fileInfo.Length > 500 * 1024 * 1024) // > 50MB
        {
            LoadWithMemoryMapping(path);
        }
        else
        {
            LoadNormal(path);
        }
    }

    private static void LoadNormal(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 65536);
        using var br = new BinaryReader(fs);
        
    }

    private void LoadWithMemoryMapping(string path)
    {
        // TODO: Implement memory-mapped version for very large files
        // This would use MemoryMappedFile for zero-copy loading
    }
}