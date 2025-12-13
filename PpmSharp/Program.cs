using System.Diagnostics.CodeAnalysis;
using PpmSharp.Models;

namespace PpmSharp;

public static class Program
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(OpenTK.Graphics.OpenGL4.GL))]
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return;
        }

        var convertMode = Array.IndexOf(args, "-c") >= 0;

        if (convertMode)
        {
            HandleConvertMode(args);
        }
        else
        {
            HandleViewMode(args);
        }
    }

    private static void HandleViewMode(string[] args)
    {
        var inputFile = args[0];

        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: File '{inputFile}' not found.");
            return;
        }

        try
        {
            Console.WriteLine($"Loading and displaying: {inputFile}");
            var ppmImage = new PpmImage(inputFile);
            
            using var window = new PpmViewerWindow(ppmImage);
            window.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image: {ex.Message}");
        }
    }


    private static void HandleConvertMode(string[] args)
    {
        string? inputFile = null;
        string? outputFile = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-c":
                {
                    if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
                    {
                        inputFile = args[i + 1];
                        i++;
                    }

                    break;
                }
                case "-o":
                {
                    if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
                    {
                        outputFile = args[i + 1];
                        i++;
                    }

                    break;
                }
                default:
                {
                    if (!args[i].StartsWith("-") && inputFile == null)
                    {
                        inputFile = args[i];
                    }

                    break;
                }
            }
        }

        if (inputFile == null)
        {
            Console.WriteLine("Error: No input file specified for conversion.");
            PrintUsage();
            return;
        }

        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Error: File '{inputFile}' not found.");
            return;
        }

        // If no output file specified, use input filename with .ppm extension
        outputFile ??= Path.ChangeExtension(inputFile, ".ppm");

        try
        {
            Console.WriteLine($"Converting '{inputFile}' to '{outputFile}'...");
            ImageToPpmConverter.Convert(inputFile, outputFile);
            Console.WriteLine("Conversion successful!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during conversion: {ex.Message}");
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("PPM Viewer and Converter");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  PpmSharp <file.ppm>              View a PPM file");
        Console.WriteLine("  PpmSharp -c <input> [-o output]  Convert image to PPM");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  PpmSharp image.ppm               Display image.ppm");
        Console.WriteLine("  PpmSharp -c photo.jpg            Convert photo.jpg to photo.ppm");
        Console.WriteLine("  PpmSharp -c photo.jpg -o out.ppm Convert photo.jpg to out.ppm");
        Console.WriteLine();
        Console.WriteLine("Supported input formats for conversion: JPEG, JPG, PNG");
    }
}