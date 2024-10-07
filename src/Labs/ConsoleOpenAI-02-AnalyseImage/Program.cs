using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var openai_key = config["OPENAI_KEY"];

OpenAIClient client = new OpenAIClient(apiKey: openai_key);

var chat = client.GetChatClient("gpt-4o-mini");

ChatMessageContentPart contentImage = ChatMessageContentPart.CreateImagePart(
    BinaryData.FromBytes(File.ReadAllBytes("images/foggyday.png")),
    "image/jpeg");

var messages = new List<ChatMessage>
{
    new UserChatMessage("describe this image using emojis and a fun style"),
    new UserChatMessage(contentImage)
};

var response = chat.CompleteChatStreamingAsync(messages);

await foreach (var item in response)
{
    if (item.ContentUpdate.Count > 0)
        Console.Write(item.ContentUpdate[0].Text);
}

