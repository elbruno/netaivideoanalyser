using OpenCvSharp;
using Microsoft.Extensions.AI;

//////////////////////////////////////////////////////
/// VIDEO
//////////////////////////////////////////////////////

// main settings
var numberOfFrames = 3;

var videoFileName = $"videos/firetruck.mp4";

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
    // resize the frame to half of its size
    Cv2.Resize(frame, frame, new OpenCvSharp.Size(frame.Width / 2, frame.Height / 2));
    frames.Add(frame);
}
video.Release();

//////////////////////////////////////////////////////
/// Microsoft.Extensions.AI
//////////////////////////////////////////////////////


IChatClient chatClientImageAnalyzer =
    new OllamaChatClient(new Uri("http://localhost:11434/"), "llava:7b");
IChatClient chatClient =
    new OllamaChatClient(new Uri("http://localhost:11434/"), "llama3.2");

Console.WriteLine("=============");
Console.WriteLine("Start frame by frame analysis");
Console.WriteLine("=============");
List<string> imageAnalysisResponses = new();
int step = (int)Math.Ceiling((double)frames.Count / numberOfFrames);
for (int i = 0; i < frames.Count; i += step)
{
    // save the frame to the "data/frames" folder
    string framePath = Path.Combine(dataFolderPath, "frames", $"{i}.jpg");
    Cv2.ImWrite(framePath, frames[i]);

    // read the image bytes, create a new image content part and add it to the messages
    AIContent aic = new ImageContent(File.ReadAllBytes(framePath), "image/jpeg");
    List<ChatMessage> messages =
    [
        new ChatMessage(ChatRole.User, $"The image represents a frame of a video. Describe the image. Include the frame number at the beginning of the description: [{i}]"),
        new ChatMessage(ChatRole.User, [aic])
     ];
    // send the messages to the assistant
    var imageAnalysis = await chatClientImageAnalyzer.CompleteAsync(messages);
    var imageAnalysisResponse = $"Frame [{i}]\n{imageAnalysis.Message.Text}\n";
    imageAnalysisResponses.Add(imageAnalysisResponse);

    Console.WriteLine(imageAnalysisResponse);
}

Console.WriteLine("=============");
Console.WriteLine("Start video analysis");
Console.WriteLine("=============");
var imageAnalysisResponseCollection = string.Join("\n===\n", imageAnalysisResponses);

var userPrompt = $"The following texts represets a video analysis from different frames from the video. Using that frames description, describe the video.\n{imageAnalysisResponseCollection}";

// send the messages to the assistant
var response = await chatClient.CompleteAsync(userPrompt);

Console.WriteLine(response.Message);