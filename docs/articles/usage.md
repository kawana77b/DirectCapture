# Usage

```csharp
using DirectCapture;

// Use "using" to discard resources.
// The file path must be an AVI file with an .avi extension.
using var capture = new VideoCapture("test.avi");

// You can get basic various information about video from VideoInfo.
// VideoInfo class can also be instantiated and used by itself.
var info = capture.VideoInfo;
capture.CurrentPosition = 1.0; // set 1 second

// .GetBitmap() captures the frame in the current position as is.
using var bmp = capture.GetBitmap();
bmp.Save("test.png");
```
