﻿using Microsoft.Extensions.Configuration;
using OpenCvSharp;
using Microsoft.Extensions.AI;
using Spectre.Console;
using Azure.AI.OpenAI;
using System.ClientModel;

SpectreConsoleOutput.DisplayTitle("MEAI - AOAI");

// define video file and data folder
SpectreConsoleOutput.DisplayTitleH1("Video file and data folder");
string videoFile = VideosHelper.GetVideoFilePathFireTruck();
string dataFolderPath = VideosHelper.CreateDataFolder();
Console.WriteLine();

var systemPrompt = PromptsHelper.SystemPrompt;
var userPrompt = PromptsHelper.UserPromptDescribeVideo;

//////////////////////////////////////////////////////
/// VIDEO ANALYSIS using OpenCV
//////////////////////////////////////////////////////

SpectreConsoleOutput.DisplayTitleH1("Video Analysis using OpenCV");

// Extract the frames from the video
var video = new VideoCapture(videoFile);
var frames = new List<Mat>();
while (video.IsOpened())
{
    var frame = new Mat();
    if (!video.Read(frame) || frame.Empty())
        break;
    // resize the frame to half of its size if the with is greater than 800
    if (frame.Width > 800)
    {
        Cv2.Resize(frame, frame, new OpenCvSharp.Size(frame.Width / 2, frame.Height / 2));
    }
    frames.Add(frame);
}
video.Release();
SpectreConsoleOutput.DisplaySubtitle("Total Frames", frames.Count.ToString());
SpectreConsoleOutput.DisplayTitleH3("Video Analysis using OpenCV done!");


//////////////////////////////////////////////////////
/// Microsoft.Extensions.AI using Azure OpenAI
//////////////////////////////////////////////////////

SpectreConsoleOutput.DisplayTitleH1("Video Analysis using Microsoft.Extensions.AI using Azure OpenAI");
var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var modelId = config["AZURE_OPENAI_MODEL"];

// create client using API Keys
var apiKey = config["AZURE_OPENAI_APIKEY"];
var credential = new ApiKeyCredential(apiKey);

IChatClient chatClient =
    new AzureOpenAIClient(new Uri(endpoint), credential)
            .AsChatClient(modelId: modelId);

List<ChatMessage> messages =
[
    new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, systemPrompt),
    new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, userPrompt),
];

// create the OpenAI files that represent the video frames
int step = (int)Math.Ceiling((double)frames.Count / PromptsHelper.NumberOfFrames);

// show the total number of frames and the step to get the desired number of frames using spectre console
SpectreConsoleOutput.DisplaySubtitle("Process", $"Get 1 frame every [{step}] to get the [{PromptsHelper.NumberOfFrames}] frames for analysis");

var tableImageAnalysis = new Table();
await AnsiConsole.Live(tableImageAnalysis)
    .AutoClear(false)   // Do not remove when done
    .Overflow(VerticalOverflow.Ellipsis) // Show ellipsis when overflowing
    .StartAsync(async ctx =>
    {
        tableImageAnalysis.AddColumn("N#");
        tableImageAnalysis.AddColumn("Location");
        ctx.Refresh();

        for (int i = 0; i < frames.Count; i += step)
        {
            // save the frame to the "data/frames" folder
            string framePath = Path.Combine(dataFolderPath, "frames", $"{i}.jpg");
            Cv2.ImWrite(framePath, frames[i]);

            // read the image bytes, create a new image content part and add it to the messages
            AIContent aic = new ImageContent(File.ReadAllBytes(framePath), "image/jpeg");
            var message = new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, [aic]);
            messages.Add(message);

            // add row
            tableImageAnalysis.AddRow(new Text(i.ToString()), new TextPath(framePath));
            ctx.Refresh();
        }
        ctx.Refresh();
    });

// display prompts
SpectreConsoleOutput.DisplayTablePrompts(systemPrompt, userPrompt);

SpectreConsoleOutput.DisplayTitleH1("Chat Client Response");


// send the messages to the assistant
var response = chatClient.CompleteStreamingAsync(messages);

// display the response
SpectreConsoleOutput.DisplayTitleH3("Azure OpenAI response using Microsoft Extensions for AI");

await foreach (var message in response)
{
    if (message.Contents.Count > 0)
        AnsiConsole.Write(message.Contents[0].ToString());
}

SpectreConsoleOutput.DisplayTitleH3("Video Analysis done!");