using OpenCvSharp;
using System;
using System.Diagnostics;


var systemPrompt = @"You are a useful assistant. When you receive a group of images, they are frames of a unique video.";

//var videoFileName = $"videos/firetruck.mp4";
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

// Open video file
using var capture = new VideoCapture(videoFile);
if (!capture.IsOpened())
{
    Console.WriteLine("Camera not found!");
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
    Console.WriteLine(frameFileName);
}
