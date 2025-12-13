using System.Text;
using PpmSharp.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PpmSharp;

public static class ImageToPpmConverter
{

    public static void Convert(string inputPath, string outputPath)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException($"Input file not found: {inputPath}");

        var extension = Path.GetExtension(inputPath).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png"))
            throw new NotSupportedException($"Unsupported file format: {extension}");

        using var image = Image.Load<Rgb24>(inputPath);
        
        image.Mutate(x => x.Grayscale());

        WriteP3Format(image, outputPath);
    }

    public static PpmImage ConvertToPpmImage(string inputPath)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            Convert(inputPath, tempFile);
            return new PpmImage(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private static void WriteP3Format(Image<Rgb24> image, string outputPath)
    {
        using var writer = new StreamWriter(outputPath, false, Encoding.ASCII);
        
        writer.WriteLine("P3");
        writer.WriteLine($"{image.Width} {image.Height}");
        writer.WriteLine("255");

        for (var y = 0; y < image.Height; y++)
        {
            for (var x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                writer.Write(pixel.R);
                
                // Add space between values, newline every 10 values for readability :)
                if ((y * image.Width + x + 1) % 10 == 0)
                    writer.WriteLine();
                else
                    writer.Write(' ');
            }
        }
        
        writer.WriteLine();
    }
}
