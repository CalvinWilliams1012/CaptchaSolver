using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static System.Net.Mime.MediaTypeNames;
namespace CaptchaSolver.Processing
{
    /* To get the best results from Tesseract OCR, we need to do some pre-processing.
     * https://tesseract-ocr.github.io/tessdoc/ImproveQuality
     */
    public static class PreProcessor
    {
        /* We need to scale the Image to minimum 300DPI for Tesseract OCR to provide optimal results.
         */
        public static void ApplyScaling(Image<L8> image)
        {
            int width = image.Width * 10;
            int height = image.Height * 10;
            image.Mutate(x => x.Resize(width, height));
        }

        public static void ApplyThresholding(Image<L8> image)
        {
            image.Mutate(x => x.BinaryThreshold(.53f));
        }

        public static void ApplyBorder(Image<L8> image)
        {
            image.Mutate(x => x.Pad(46,60,Color.Black));
        }

        public static void ApplyEdgeDetection(Image<L8> image)
        {
            image.Mutate(x => x.DetectEdges());
        }

        #region Opening
        /* We need to apply Opening to the image through erosion followed by dialation.
         * This will reduce and remove noise from the image.
         * https://github.com/SixLabors/ImageSharp/discussions/2372
         */
        public static void ApplyOpening(Image<L8> image)
        {
            image.Mutate(x => x.Invert());
            ApplyErosion3x3(image);
            ApplyDilation3x3(image);
            image.Mutate(x => x.Invert());
        }

        /* Grayscale Erosion https://en.wikipedia.org/wiki/Erosion_(morphology)
         * "In other words the erosion of a point is the minimum of the points in its neighborhood, with that neighborhood defined by the structuring element."
         * in this example, our 'structuring element' is a 3x3 square. 
         */
        public static void ApplyErosion3x3(Image<L8> target)
        {
            using var source = target.Clone();
            source.ProcessPixelRows(target, (sourceAccessor, targetAccessor) =>
            {
                //We're iterating through every row
                for (int y = 0; y < sourceAccessor.Height; y++)
                {
                    var rowSource = sourceAccessor.GetRowSpan(y);
                    var lastRowSource = y > 0 ? sourceAccessor.GetRowSpan(y - 1) : null;
                    var nextRowSource = y + 1 < sourceAccessor.Height ? sourceAccessor.GetRowSpan(y + 1) : null;

                    var rowTarget = targetAccessor.GetRowSpan(y);
                    //Iterate through each pixel in the row
                    for (var x = 0; x < sourceAccessor.Width; x++)
                    {
                        var min = rowSource[x].PackedValue;

                        // Min value from last row
                        if (lastRowSource != null)
                        {
                            if (x > 0 && lastRowSource[x - 1].PackedValue < min) min = lastRowSource[x - 1].PackedValue;
                            if (lastRowSource[x].PackedValue < min) min = lastRowSource[x].PackedValue;
                            if (x + 1 < sourceAccessor.Width && lastRowSource[x + 1].PackedValue < min) min = lastRowSource[x + 1].PackedValue;
                        }

                        // Min value from current row
                        if (x > 0 && rowSource[x - 1].PackedValue < min) min = rowSource[x - 1].PackedValue;
                        if (x + 1 < sourceAccessor.Width && rowSource[x + 1].PackedValue < min) min = rowSource[x + 1].PackedValue;

                        // Min value from next row
                        if (nextRowSource != null)
                        {
                            if (x > 0 && nextRowSource[x - 1].PackedValue < min) min = nextRowSource[x - 1].PackedValue;
                            if (nextRowSource[x].PackedValue < min) min = nextRowSource[x].PackedValue;
                            if (x + 1 < sourceAccessor.Width && nextRowSource[x + 1].PackedValue < min) min = nextRowSource[x + 1].PackedValue;
                        }
                        rowTarget[x].PackedValue = min;
                    }
                }
            });
        }
        public static void ApplyDilation3x3(Image<L8> target)
        {
            using var source = target.Clone();
            source.ProcessPixelRows(target, (sourceAccessor, targetAccessor) =>
            {
                for (int y = 0; y < sourceAccessor.Height; y++)
                {
                    var rowSource = sourceAccessor.GetRowSpan(y);
                    var lastRowSource = y > 0 ? sourceAccessor.GetRowSpan(y - 1) : null;
                    var nextRowSource = y + 1 < sourceAccessor.Height ? sourceAccessor.GetRowSpan(y + 1) : null;

                    var rowTarget = targetAccessor.GetRowSpan(y);
                    for (var x = 0; x < sourceAccessor.Width; x++)
                    {
                        var max = rowSource[x].PackedValue;

                        // Max value from last row
                        if (lastRowSource != null)
                        {
                           if (x > 0 && lastRowSource[x - 1].PackedValue > max) max = lastRowSource[x - 1].PackedValue;
                           if (lastRowSource[x].PackedValue > max) max = lastRowSource[x].PackedValue;
                           if (x + 1 < sourceAccessor.Width && lastRowSource[x + 1].PackedValue > max) max = lastRowSource[x + 1].PackedValue;
                        }

                        // Max value from current row
                        if (x > 0 && rowSource[x - 1].PackedValue > max) max = rowSource[x - 1].PackedValue;
                        if (x + 1 < sourceAccessor.Width && rowSource[x + 1].PackedValue > max) max = rowSource[x + 1].PackedValue;

                        // Max value from next row
                        if (nextRowSource != null)
                        {
                            if (x > 0 && nextRowSource[x - 1].PackedValue > max) max = nextRowSource[x - 1].PackedValue;
                            if (nextRowSource[x].PackedValue > max) max = nextRowSource[x].PackedValue;
                            if (x + 1 < sourceAccessor.Width && nextRowSource[x + 1].PackedValue > max) max = nextRowSource[x + 1].PackedValue;
                        }

                        rowTarget[x].PackedValue = max;
                    }
                }
            });
        }
        #endregion
    }
}
