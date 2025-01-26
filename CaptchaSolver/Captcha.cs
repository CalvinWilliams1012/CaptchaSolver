using CaptchaSolver.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace CaptchaSolver
{
    public class Captcha : IDisposable
    {
        public Image<L8> image { get; set; }
        private String parsedString = "";

        const int rectangleWidth = 36;
        const int rectangleHeight = 50;


        public Captcha(String imagePath) 
        {
            this.image = Image.Load<L8>(imagePath);
        }


        public string Process()
        {
            if (image != null && image.Bounds.Width == 180 && image.Bounds.Height == 50)
            {
                for (int i = 0; i < 5; i++)
                {
                    using (Image<L8> charImage = image.Clone(c => c.Crop(new Rectangle(i * 36, 0, rectangleWidth, rectangleHeight))))
                    {
                        charImage.Save("C:/Users/calvi/source/repos/CaptchaSolver/output/CaptchaCharStart" + i + ".jpg");
                        PreProcessor.ApplyOpening(charImage);
                        PreProcessor.ApplySharpen(charImage);
                        //PreProcessor.ApplyScaling(charImage);
                        TryProcessCharImage(charImage);
                        charImage.Save("C:/Users/calvi/source/repos/CaptchaSolver/output/CaptchaCharFinal" + i + ".jpg");
                    }
                }
                return parsedString.Replace("\n",String.Empty);
            }
            else
            {
                throw new Exception("image not valid captcha");
            }
        }

        private bool TryProcessCharImage(Image<L8> charImage)
        {
            using (var engine = new TesseractEngine("C:/Users/calvi/source/repos/CaptchaSolver/CaptchaSolver/tessdata", "eng", EngineMode.Default))
            {
                engine.DefaultPageSegMode = PageSegMode.SingleChar;
                engine.SetVariable("tessedit_char_whitelist", "0123456789");
                using (var streamedImage = new MemoryStream())
                {
                    charImage.Save(streamedImage, new PngEncoder());
                    using (var pixImage = Pix.LoadFromMemory(streamedImage.ToArray()))
                    {
                        using (var p = engine.Process(pixImage))
                        {
                            var text = p.GetText();
                            if (text != null && text.Length > 0)
                            {
                                parsedString += text;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            image.Dispose();
        }
    }
}
