using OpenCvSharp;
using Microsoft.Extensions.AI;

//////////////////////////////////////////////////////
/// VIDEO
//////////////////////////////////////////////////////

// main settings
var numberOfFrames = 5;

var videoFileName = $"videos/firetruck.mp4";

// define video file and data folder
string videosFolder = FindVideosFolder(Directory.GetCurrentDirectory());
string videoFile = Path.Combine(videosFolder, videoFileName);
string dataFolderPath = CreateDataFolder();

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

Console.WriteLine("====================================================");
Console.WriteLine("Start frame by frame analysis");
Console.WriteLine("Number of frames: " + frames.Count);
Console.WriteLine("====================================================");
Console.WriteLine("");

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
        new ChatMessage(ChatRole.User, @$"The image represents a frame of a video. Describe the image in a single sentence. Frame Number: [{i}]

In example:
Frame Number: 0
A view of a fire station with red garage doors. The sidewalk is empty, and there are yellow posts in front of the garage doors. Surrounding buildings are visible in the background, along with a clear blue sky.

Frame Number: 1
The same fire station is shown, but now a fire truck is partially visible, parked in front of the garage doors. The scene retains the same urban backdrop, with nearby buildings and trees.

Frame Number: 2
The fire truck is now seen moving out of the station and onto the street. The background features a tall black building and additional urban elements, including traffic signs and trees."),
        new ChatMessage(ChatRole.User, [aic])
     ];
    // send the messages to the assistant
    var imageAnalysis = await chatClientImageAnalyzer.CompleteAsync(messages);
    var imageAnalysisResponse = $"{imageAnalysis.Message.Text}\n";
    imageAnalysisResponses.Add(imageAnalysisResponse);

    Console.WriteLine(imageAnalysisResponse);
}

Console.WriteLine("====================================================");
Console.WriteLine("Start build prompt");
Console.WriteLine("====================================================");
Console.WriteLine("");
var imageAnalysisResponseCollection = string.Empty;

foreach (var desc in imageAnalysisResponses)
{
    imageAnalysisResponseCollection += $"\n[FRAME ANALYSIS START]\n{desc}\n[FRAME ANALYSIS END]\n";
}

var userPrompt = $"The texts below represets a video analysis from different frames from the video. Using that frames description, describe the video. Do not describe individual frames, using the frames information infer the content of the video.\n{imageAnalysisResponseCollection}";

// display the full user prompt 
Console.WriteLine(userPrompt);
Console.WriteLine("");

Console.WriteLine("====================================================");
Console.WriteLine("Start video analysis");
Console.WriteLine("====================================================");
Console.WriteLine("");

Console.WriteLine("ASSISTANT: ");
// send the messages to the assistant
var response = await chatClient.CompleteAsync(userPrompt);

Console.WriteLine(response.Message);


static string CreateDataFolder()
{
    // Create or clear the "data" folder and the "data/frames" folder
    string dataFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "data");
    if (Directory.Exists(dataFolderPath))
    {
        Directory.Delete(dataFolderPath, true);
    }
    Directory.CreateDirectory(dataFolderPath);
    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "data/frames"));
    return dataFolderPath;
}

static string FindVideosFolder(string startDirectory)
{
    var currentDirectory = startDirectory;

    while (true)
    {
        // display the current directory
        Console.WriteLine($"Current Directory: {currentDirectory}");

        var potentialVideos = Path.Combine(currentDirectory, "videos");
        if (Directory.Exists(potentialVideos))
        {
            return potentialVideos;
        }

        var parentDirectory = Directory.GetParent(currentDirectory);
        Console.WriteLine($"Parent Directory: {currentDirectory}");
        if (parentDirectory == null)
        {
            throw new DirectoryNotFoundException("The 'videos' folder was not found in any parent directory.");
        }

        currentDirectory = parentDirectory.FullName;
    }
}