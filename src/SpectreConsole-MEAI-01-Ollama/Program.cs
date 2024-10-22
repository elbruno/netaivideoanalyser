using OpenCvSharp;
using Microsoft.Extensions.AI;
using Spectre.Console;
using System.Diagnostics;

SpectreConsoleOutput.DisplayTitle("MEAI - OLLAMA");

// define video file and data folder
SpectreConsoleOutput.DisplayTitleH1("Video file and data folder");
string videoFile = VideosHelper.GetVideoFilePathFireTruck();
string dataFolderPath = VideosHelper.CreateDataFolder();
Console.WriteLine();

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
    // resize the frame to half of its size
    Cv2.Resize(frame, frame, new OpenCvSharp.Size(frame.Width / 2, frame.Height / 2));
    frames.Add(frame);
}
video.Release();
SpectreConsoleOutput.DisplaySubtitle("Total Frames", frames.Count.ToString());
SpectreConsoleOutput.DisplayTitleH3("Video Analysis using OpenCV done!");

//////////////////////////////////////////////////////
/// Microsoft.Extensions.AI using Ollama
//////////////////////////////////////////////////////
SpectreConsoleOutput.DisplayTitleH1("Video Analysis using Microsoft.Extensions.AI using Ollama");

IChatClient chatClientImageAnalyzer =
    new OllamaChatClient(new Uri("http://localhost:11434/"), "llava:7b");
IChatClient chatClient =
    new OllamaChatClient(new Uri("http://localhost:11434/"), "llama3.2");


List<string> imageAnalysisResponses = new();
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
        tableImageAnalysis.AddColumn("Elapsed");
        tableImageAnalysis.AddColumn("Description");
        ctx.Refresh();

        var stopwatch = new Stopwatch();

        for (int i = 0; i < frames.Count; i += step)
        {
            // save the frame to the "data/frames" folder
            string framePath = Path.Combine(dataFolderPath, "frames", $"{i}.jpg");
            Cv2.ImWrite(framePath, frames[i]);

            // read the image bytes, create a new image content part and add it to the messages
            AIContent aic = new ImageContent(File.ReadAllBytes(framePath), "image/jpeg");
            List<ChatMessage> messages =
            [
                new ChatMessage(ChatRole.User, @$"The image represents a frame of a video. Describe the image in a single sentence for the frame Number: [{i}]
In example:
[IMAGE DESCRIPTION START]
Frame 1: A view of a fire station with red garage doors. The sidewalk is empty, and there are yellow posts in front of the garage doors. Surrounding buildings are visible in the background, along with a clear blue sky.
[IMAGE DESCRIPTION END]
[IMAGE DESCRIPTION START]
Frame 2: The same fire station is shown, but now a fire truck is partially visible, parked in front of the garage doors. The scene retains the same urban backdrop, with nearby buildings and trees.
[IMAGE DESCRIPTION END]
[IMAGE DESCRIPTION START]
Frame 3: The fire truck is now seen moving out of the station and onto the street. The background features a tall black building and additional urban elements, including traffic signs and trees.
[IMAGE DESCRIPTION END]"),
        new ChatMessage(ChatRole.User, [aic])
             ];
            // send the messages to the assistant            
            stopwatch.Restart();
            var imageAnalysis = await chatClientImageAnalyzer.CompleteAsync(messages);
            var imageAnalysisResponse = $"{imageAnalysis.Message.Text}\n";
            imageAnalysisResponses.Add(imageAnalysisResponse);
            stopwatch.Stop();
            var elapsedTime = stopwatch.Elapsed;
            
            // add row
            var shortResponse = imageAnalysisResponse.Length > 100 ? imageAnalysisResponse.Substring(0, 100) + "..." : imageAnalysisResponse;
            tableImageAnalysis.AddRow(new Text(i.ToString()), new Text(elapsedTime.ToString("ss\\.fff")), new Text(shortResponse));
            ctx.Refresh();
        }
        ctx.Refresh();
    });

SpectreConsoleOutput.DisplayTitleH3("Frame by frame Analysis using llava models done!");

SpectreConsoleOutput.DisplayTitleH2("Start build prompt");
var imageAnalysisResponseCollection = string.Empty;

foreach (var desc in imageAnalysisResponses)
{
    imageAnalysisResponseCollection += $"\n[FRAME ANALYSIS START]{desc}[FRAME ANALYSIS END]";
}

var userPrompt = $"The texts below represets a video analysis from different frames from the video. Using the frames description, describe the video. Do not describe individual frames. Do not mention the frame number of frame description. Using the frames information infer the content and the story of the video.\n{imageAnalysisResponseCollection}";

//// display the full user prompt 
//Console.WriteLine(userPrompt);
//Console.WriteLine("");

SpectreConsoleOutput.DisplayTitleH3("Start build prompt done!");


SpectreConsoleOutput.DisplayTitleH2("Start video analysis using LLM");

// send the messages to the assistant
var response = await chatClient.CompleteAsync(userPrompt);

var panelResponse = new Panel(response.Message.ToString());
panelResponse.Header = new PanelHeader("MEAI Chat Client using Ollama Response");
AnsiConsole.Write(panelResponse);