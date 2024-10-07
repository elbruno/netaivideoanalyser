using OpenCvSharp;
using System;
using System.Diagnostics;


// Open the default camera (usually the first camera)
using var capture = new VideoCapture(0);
if (!capture.IsOpened())
{
    Console.WriteLine("Camera not found!");
    return;
}

// Create a window to display the camera feed
using var window = new Window("Camera");

// Create a Mat object to hold the frame data
using var frame = new Mat();

// Create a Stopwatch to measure time
var stopwatch = new Stopwatch();
int frameCount = 0;
double fps = 0.0;

while (true)
{
    stopwatch.Restart();

    // Capture a frame from the camera
    capture.Read(frame);

    // If the frame is empty, break the loop
    if (frame.Empty())
        break;


    // Calculate FPS
    frameCount++;
    if (frameCount >= 10)
    {
        stopwatch.Stop();
        fps = frameCount / (stopwatch.ElapsedMilliseconds / 1000.0);
        frameCount = 0;
    }

    // Display FPS on the frame
    // Cv2.PutText(frame, $"FPS: {fps:F2}", new Point(10, 30), HersheyFonts.HersheySimplex, 1, Scalar.White, 2);

    // Show the frame in the window
    window.ShowImage(frame);

    // Wait for 1 ms and check if the 'q' key is pressed
    if (Cv2.WaitKey(1) == 'q')
        break;
}
