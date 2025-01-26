// See https://aka.ms/new-console-template for more information
using CaptchaSolver;
using CaptchaSolver.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Tesseract;

var files = Directory.GetFiles("C:/Users/calvi/source/repos/CaptchaSolver/examples/");
int correctCount = 0;
int incorrectCount = 0;
foreach(var file in files)
{
    var c = new Captcha(file).Process();
    if(c.Length == 5)
    {
        correctCount++;
        Console.Write($"Correct:{c}{Environment.NewLine}");
    }
    else
    {
        incorrectCount++;
        Console.Write($"Incorrect:{c}{Environment.NewLine}");
    }
}
Console.WriteLine($"Total Correct:{correctCount}{Environment.NewLine}");
Console.WriteLine($"Total Incorrect:{incorrectCount}{Environment.NewLine}");
Console.ReadLine();

