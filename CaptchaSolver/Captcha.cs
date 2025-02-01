using CaptchaSolver.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
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
                PreProcessor.ApplyThresholding(image);
                PreProcessor.ApplyOpening(image);

                for (int i = 0; i < 5; i++)
                {
                    using (Image<L8> charImage = image.Clone(c => c.Crop(new Rectangle(i * 36, 0, rectangleWidth, rectangleHeight))))
                    {
                        TryProcessCharImage(charImage);
                        charImage.Save("C:/Users/calvi/source/repos/CaptchaSolver/output/CaptchaCharFinal" + i + ".jpg");
                    }
                }
                //OCR may add whitespace, remove it before returning!
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
                //We want tesseract to only look for a single character
                engine.DefaultPageSegMode = PageSegMode.SingleChar;
                //and we only want to match the following characters.
                engine.SetVariable("tessedit_char_whitelist", "!|Iilt0123456789");
                /* 
                 * You'll notice 'tIil' which were added to improve accuracy of 1 and 4's
                 * The 1's and 4's would occasionally be missed
                 * 4 is similar to the 't' character, so if OCR finds this, we replace with 4
                 * 1 is similar to '!|Iil', so if we find this, replace with 1
                 */
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
                                text = text.Replace('!', '1');
                                text = text.Replace('|', '1');
                                text = text.Replace('l', '1');
                                text = text.Replace('I', '1');
                                text = text.Replace('i', '1');
                                text = text.Replace('t', '4');
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
