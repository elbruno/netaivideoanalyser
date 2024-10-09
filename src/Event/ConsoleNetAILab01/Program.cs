using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using OpenCvSharp;
using System.ClientModel;

var videoFileName = $"videos/racoon.mp4";

// Create or clear the "data" folder and the "data/frames" folder
string dataFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
if (Directory.Exists(dataFolderPath))
{
    Directory.Delete(dataFolderPath, true);
}
Directory.CreateDirectory(dataFolderPath);
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "data/frames"));

// video file
string videoFile = Path.Combine(Directory.GetCurrentDirectory(), videoFileName);

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

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var openai_key = config["OPENAI_KEY"];


var numberOfFrames = 15;
var systemPrompt = @"You are a useful assistant. When you receive a group of images, they are frames of a unique video.";

var userPrompt = @"The following frames represets a video. Describe the video.";


// Create the OpenAI client
var client = new OpenAIClient(openai_key);

var chatClient = client.GetChatClient("gpt-4o-mini");

List<ChatMessage> messages =
[
    new SystemChatMessage(systemPrompt),
    new UserChatMessage(userPrompt),
];


// create the OpenAI files that represent the video frames
int step = (int)Math.Ceiling((double)frames.Count / numberOfFrames);
for (int i = 0; i < frames.Count; i += step)
{
    // save the frame to the "data/frames" folder
    string framePath = Path.Combine(dataFolderPath, "frames", $"{i}.jpg");
    Cv2.ImWrite(framePath, frames[i]);

    // read the image bytes, create a new image content part and add it to the messages
    var imageContentPart = ChatMessageContentPart.CreateImagePart(
        imageBytes: BinaryData.FromBytes(File.ReadAllBytes(framePath)),
        imageBytesMediaType: "image/jpeg");
    var message = new UserChatMessage(imageContentPart);
    messages.Add(message);
}

// send the messages to the assistant
AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreamingAsync(messages);

// print the assistant responses
Console.Write($"[ASSISTANT]: ");
await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
{
    if (completionUpdate.ContentUpdate.Count > 0)
    {
        Console.Write(completionUpdate.ContentUpdate[0].Text);
    }
}