using Microsoft.Extensions.Configuration;
using OpenCvSharp;
using OpenAI;
using Microsoft.Extensions.AI;


//////////////////////////////////////////////////////
/// VIDEO
//////////////////////////////////////////////////////

// main settings
var numberOfFrames = 15;
var systemPrompt = @"You are a useful assistant. When you receive a group of images, they are frames of a unique video.";

var videoFileName = $"firetruck.mp4";
var userPrompt = @"The following frames represets a video. Describe the video.";

//var videoFileName = $"videos/insurance_v3.mp4";
//var userPrompt = @"You are an expert in evaluating car damage from car accidents for auto insurance reporting. 
//Create an incident report for the accident shown in the video with 3 sections. 
//- Section 1 will include the car details (license plate, car make, car model, approximant model year, color, mileage).
//- Section 2 list the car damage, per damage in a list.
//- Section 3 will only include exactly 6 sentence description of the car damage.";

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
    // resize the frame to half of its size if the with is greater than 800
    if (frame.Width > 800)
    {
        Cv2.Resize(frame, frame, new OpenCvSharp.Size(frame.Width / 2, frame.Height / 2));
    }
    frames.Add(frame);
}
video.Release();

//////////////////////////////////////////////////////
/// Microsoft.Extensions.AI
//////////////////////////////////////////////////////

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var openai_key = config["OPENAI_KEY"];

IChatClient chatClient = 
    new OpenAIClient(apiKey: openai_key)
    .AsChatClient("gpt-4o-mini");

List<ChatMessage> messages =
[
    new ChatMessage(ChatRole.System, systemPrompt),
    new ChatMessage(ChatRole.User, userPrompt),
];

// create the OpenAI files that represent the video frames
int step = (int)Math.Ceiling((double)frames.Count / numberOfFrames);
for (int i = 0; i < frames.Count; i += step)
{
    // save the frame to the "data/frames" folder
    string framePath = Path.Combine(dataFolderPath, "frames", $"{i}.jpg");
    Cv2.ImWrite(framePath, frames[i]);

    // read the image bytes, create a new image content part and add it to the messages
    AIContent aic = new ImageContent(File.ReadAllBytes(framePath), "image/jpeg");
    var message = new ChatMessage(ChatRole.User, [aic]);
    messages.Add(message);
}

// send the messages to the assistant
var response = await chatClient.CompleteAsync(messages);

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