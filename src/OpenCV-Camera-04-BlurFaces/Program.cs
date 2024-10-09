using OpenCvSharp;
using System.Diagnostics;

// load a cascade file for detecting faces
var faceCascade = new CascadeClassifier("haarcascade_frontalface_default.xml");

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


    // Convert the frame to grayscale
    using var gray = new Mat();
    Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

    // Detect faces
    var faces = faceCascade.DetectMultiScale(gray, 1.3, 5);

    foreach (var face in faces)
    {
        // Draw a rectangle around the face
        Cv2.Rectangle(frame, face, new Scalar(0, 0, 255), 2);

        //// Extract the face region
        //var faceRegion = new Rect(face.X, face.Y, face.Width, face.Height);
        //using var faceMat = new Mat(frame, faceRegion);

        //// Apply Gaussian blur to the face region
        //Cv2.GaussianBlur(faceMat, faceMat, new Size(23, 23), 30);

        //// Merge the blurry face back to the frame
        //faceMat.CopyTo(new Mat(frame, faceRegion));
    }


    // Calculate FPS
    frameCount++;
    if (frameCount >= 10)
    {
        stopwatch.Stop();
        fps = frameCount / (stopwatch.ElapsedMilliseconds / 1000.0);
        frameCount = 0;
    }

    // Display FPS on the frame
    Cv2.PutText(frame, $"FPS: {fps:F2}", new Point(10, 30), HersheyFonts.HersheySimplex, 1, Scalar.White, 2);

    // Show the frame in the window
    window.ShowImage(frame);

    // Wait for 1 ms and check if the 'q' key is pressed
    if (Cv2.WaitKey(1) == 'q')
        break;
}
