using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Chat;
using OpenAI.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AICaptchaSolver
{
    public static class AICaptcha
    {
        public static string Process(String imagePath)
        {
            ChatClient client = new("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

            using Stream imageStream = File.OpenRead(imagePath);
            BinaryData imageBytes = BinaryData.FromStream(imageStream);

            List<ChatMessage> messages =
            [
                new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart("Please provide the numbers and letters in the following image with no additional text."),
                    ChatMessageContentPart.CreateImagePart(imageBytes, "image/png")),
            ];

            ChatCompletion completion = client.CompleteChat(messages);
            return completion.Content[0].Text;
        }
    }
}
