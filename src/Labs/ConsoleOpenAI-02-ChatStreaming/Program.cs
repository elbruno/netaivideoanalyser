#pragma warning disable OPENAI001 

using Microsoft.Extensions.Configuration;
using OpenAI;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var openai_key = config["OPENAI_KEY"];

OpenAIClient client = new OpenAIClient(apiKey: openai_key);

var chat = client.GetChatClient("gpt-4o-mini");

var response = chat.CompleteChatStreamingAsync("write a 4 paragraph fun story about a .NET Cloud Advocate");


await foreach (var item in response)
{
    if (item.ContentUpdate.Count > 0)
        Console.Write(item.ContentUpdate[0].Text);
}

