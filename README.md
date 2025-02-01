
# Solving Captcha's
##### tl;dr - experimenting with solving basic captcha's, simple ocr will actually resolve them fairly consistently, but training your own neural net or use existing ai to solve is more consistent, but I don't see a need to pay for Captcha solving API services for these basic Captchas. 

I was attempting to implement a solution to create my own modernized version of an old website... however I encountered a problem... Some of the  data was protected behind 'old' captcha's like the following:
![A very basic captcha with the numbers '25553' surrounded by dots](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/25553.jpg)

I could tell due to the simplicity, that these types of captcha's we're already a 'solved' problem. A few years ago I remember following a ML course which had us train a model and use **O**ptical **C**haracter **R**ecognition(**OCR**) to identify characters from an image.

I first tried to find an API to do this for me, and they exist, but the pricing is something like $1 per 1000 basic captcha's, which despite being inexpensive I thought was rather expensive, so this is me playing around to see if them charging $1 per 1000 makes sense 

## Neural Network
This will work, but didn't implement this. You would need to find out how to split the characters given your specific captcha, label your data, and then feed it into a [Convolutional Neural Network](https://en.wikipedia.org/wiki/Convolutional_neural_network) implemented for example with [TensorFlow](https://www.tensorflow.org/tutorials/images).

I thought however the captcha's I needed to solve may be so simple that I didn't need to do that.

## OCR

I first tried to just feed the Captcha to modern OCR projects.. Load the image, and feed it into [Tesseract](https://github.com/tesseract-ocr/tesseract), whitelisting only the characters '0123456789', as our captcha only has numbers. 

This unsurprisingly gave me <5% success rate, but still got a few! So if you can infinitely attempt your captcha's this would work and take nearly no time to implement, small C# sample utilizing this [.NET Tesseract wrapper](https://github.com/charlesw/tesseract):

    using (var engine = new TesseractEngine("pathToYourTesseractData", "eng", EngineMode.Default))
    {
    	using (var pixImage = Pix.LoadFromMemory(yourImage))
    	{
        	using (var p = engine.Process(pixImage))
        	{
        		var text = p.GetText();
       		}
    	}
    }
To build upon this however we need to process the images.

## Preprocessing
Tesseract has a document describing [how to improve the quality of the output](https://tesseract-ocr.github.io/tessdoc/ImproveQuality.html).

I played around with all of these options, and the current state of the [Preprocessed OCR Captcha in the repo here](https://github.com/CalvinWilliams1012/CaptchaSolver/tree/main/CaptchaSolver) is solving them ~61% of the time.

We start with our captcha:
![A very basic captcha with the numbers '25553' surrounded by dots](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/25553.jpg)

I load the image in with [ImageSharp](https://github.com/SixLabors/ImageSharp) and then apply Binarisation to the image. This actually removes a lot of noise, but we still have some:
![enter image description here](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/ThresholdImage.jpg)

So we need to remove the remaining noise. These captcha's have a lot of small dots, so to remove these, I apply Opening, which Erodes then Dilates the image. This is well described in the [OpenCV documentation here](https://docs.opencv.org/4.x/d9/d61/tutorial_py_morphological_ops.html).
I tried different 'structuring elements', but a 3x3 appeared to work best here, giving us the output:
![enter image description here](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/OpenedImage.jpg)

To improve upon this, I wanted to split the image and then use [Tesseracts single character Page segmentation method](https://tesseract-ocr.github.io/tessdoc/ImproveQuality.html#noise-removal:~:text=10%20%20%20%20Treat%20the%20image%20as%20a%20single%20character.).
Luckily, from reviewing my captcha, they are all 5 characters, all 180x50 pixels, and each character appears to only be generated within every 36 pixels, so I split the image every 36 pixels, giving us the individual characters:
![2](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/2.jpg)
![5](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/5.jpg)
![5](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/5-2.jpg)
![5](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/5-3.jpg)
![3](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/3.jpg)

Feeding these to Tesseract was giving me around ~50% accuracy but I noticed that a lot of these failures were due to Tesseract thinking everything was a 4, which I don't blame it due to the captcha's font and 1's:
![1](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/1.jpg)
I somewhat alleviated this by also whitelisting characters like 'i' 'l' 'I' '|' and then performing a Replace with '1'. 
I also found that sometimes an actual 4 would not be found, for example:
![t](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/t.jpg)
Adding 't' to the whitelist, and performing a replace resolved these scenarios, which gave me the current 61% accuracy. 

## Possible further improvements
I still have Tesseract thinking a lot of 1's, 8's and 9's are 4's. From the failures I can see, eliminating these remaining incorrect characters would give us an additional +9% accuracy. This can be done through [fine tuning tesseract with your data](https://github.com/tesseract-ocr/tessdoc/blob/main/tess5/TrainingTesseract-5.md), this would be similar steps to training your own neural net, you label the data, but train Tesseract with this data instead.

The next problem is occasionally you'll get characters which were 'split' by the preprocessing, for example:
![a split 3](https://raw.githubusercontent.com/CalvinWilliams1012/CaptchaSolver/refs/heads/main/readme-examples/split.jpg)
which would be recognized as multiple characters, despite using single character page segmentation. 
To solve this, instead of opening to remove noise, I believe simply finding the 'largest island' in the image(largest set of neighboring black pixels) and removing the rest may be a better way and be more successful. 

## Using Existing AI Vision
I had some OpenAI credits, so I tried [just passing the captcha unprocessed and asking OpenAI for the numbers in the image](https://github.com/CalvinWilliams1012/CaptchaSolver/blob/main/AICaptchaSolver/AICaptcha.cs). I used "gpt-4o-mini", and "gpt-4o", the mini model gave me ~95% accuracy... but "gpt-4o" was 100% accurate from all Captcha's I tested.

According to the [OpenAI page](https://openai.com/api/pricing/), a 180x50 image should cost ~$0.000638 each with gpt-4o, from my actual tests, each image with prompt was utilizing 277 input tokens and 2 output tokens, which almost exactly aligns with this estimate.

So the cost to process 1000 images, in order to compare to captcha services, would be $0.64, which is only pennies, but is a 41% difference in cost.

# Summary
I believe that the captcha services may make sense for modern complex captcha's, as most of them appear to utilize humans on the other end to solve the captcha, however for more basic captcha's, rolling out your own neural net or utilizing image processing with OCR can yield fairly good results if you don't mind retrying on failure, or you can pay less for modern AI vision to get similar accuracy as the services, for a bit cheaper. 