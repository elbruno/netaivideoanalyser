
// Open the default camera (usually the first camera)
using OpenCvSharp;

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

while (true)
{
    // Capture a frame from the camera
    capture.Read(frame);

    // If the frame is empty, break the loop
    if (frame.Empty())
        break;

    // Show the frame in the window
    window.ShowImage(frame);

    // Wait for 1 ms and check if the 'q' key is pressed
    if (Cv2.WaitKey(1) == 'q')
        break;
}
