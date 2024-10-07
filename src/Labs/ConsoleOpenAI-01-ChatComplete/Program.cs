#pragma warning disable OPENAI001 

using Microsoft.Extensions.Configuration;
using OpenAI;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var openai_key = config["OPENAI_KEY"];

OpenAIClient client = new OpenAIClient(apiKey: openai_key);

var chat = client.GetChatClient("gpt-4o-mini");

var response = await chat.CompleteChatAsync("What is the capital of the United States?");

Console.WriteLine(response.Value.Content[0].Text);