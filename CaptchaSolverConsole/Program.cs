// See https://aka.ms/new-console-template for more information
using AICaptchaSolver;
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
    if(c.Length == 5 && file.Contains(c))
    {
        correctCount++;
        Console.Write($"Correct {file}:{c}{Environment.NewLine}");
    }
    else
    {
        if (c.Length == 5)
        {
            Console.Write($"Incorrect 5'r {file}:{c}{Environment.NewLine}");
        }
        else
        {
            Console.Write($"Incorrect {file}:{c}{Environment.NewLine}");
        }
        incorrectCount++;
    }
}
Console.WriteLine($"Total Correct:{correctCount}{Environment.NewLine}");
Console.WriteLine($"Total Incorrect:{incorrectCount}{Environment.NewLine}");
/*
Console.WriteLine($"{new Captcha($"C:/Users/calvi/source/repos/CaptchaSolver/examples/56114.jpg").Process()}");
*/
/*
Console.WriteLine(AICaptcha.Process("C:/Users/calvi/source/repos/CaptchaSolver/examples/59572.jpg"));*/
Console.ReadLine();
