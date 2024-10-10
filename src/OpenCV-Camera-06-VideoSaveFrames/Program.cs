using OpenCvSharp;


// video file
var videoFileName = $"firetruck.mp4";
string videosFolder = FindVideosFolder(Directory.GetCurrentDirectory());
string videoFile = Path.Combine(videosFolder, videoFileName);

// create the data folder to store video frames
string dataFolderPath = CreateDataFolder();

// print the video file location
Console.WriteLine($"Video File: {videoFile}");

// Open video file
using var capture = new VideoCapture(videoFile);
if (!capture.IsOpened())
{
    Console.WriteLine($"File not found: {videoFile}");
    return;
}

// Create a Mat object to hold the frame data
using var frame = new Mat();

while (true)
{
    // Capture a frame from the camera
    capture.Read(frame);

    // If the frame is empty, break the loop
    if (frame.Empty())
        break;

    // save the frame
    string frameFileName = Path.Combine(Directory.GetCurrentDirectory(), $"data/frames/frame_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.jpg");
    frame.SaveImage(frameFileName);

    // show in the console the frame file name
    Console.WriteLine($"Saving frame FileName: {frameFileName}");
}

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