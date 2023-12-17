# DirectCapture

C# library aimed at capturing `AVI` files playable in [DirectShow](https://learn.microsoft.com/en-US/windows/win32/directshow/directshow) as Bitmap.  
Includes a few simple utility classes.

**Note: If you want to perform frame capture against modern, general-purpose video codecs, do not hesitate to consider OpenCV or ffmpeg. This is for niche purposes and experimental. I would do so if I could.**

## Install

Legacy Windows Media Player must be available and configured to play the AVI files you wish to target.

Packages are distributed through **Github Packages**. Check the `packages` page for details.  
**Personal Access Token configuration is required to install the package**. Please check the following page:  
[Working with the NuGet registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry#authenticating-with-a-personal-access-token)

Use `dotnet nuget add source` or add feeds with `Nuget.Config`.  
Please add `https://nuget.pkg.github.com/kawana77b/index.json` to the package source and set your personal token.

For example:

```bash
dotnet nuget add source "https://nuget.pkg.github.com/kawana77b/index.json" --name "kawana77b" --username "YourName" --password <personal-token>
```

Or clone the repository and include the project.

Currently I am not willing to host this project in NuGet.

## Usage

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

## Environment

For Windows only.  
Target framework is currently configured with .NET 8 (Windows) and .NET Framework 481.  
And PlatformTarget is x64.

## Why did you create this?

`DirectShow` is a very legacy API, but it may be necessary for organizations that deal with some specialized video codecs.
This library helps to analyze frames by capturing images from video of such codecs and allowing them to be handled as `Bitmap`.

Also, information about this legacy API is becoming less and less available every day. While there are many examples of libraries related to camera devices, there seem to be few examples that handle `AVI` files. This is intended to be one of the use cases and that many people can use as a code reference.

## Credits

This project uses, borrows, and references the following libraries.

- [Extract Frames from Video Files](https://www.codeproject.com/articles/13237/extract-frames-from-video-files?fid=273922&df=90&mpp=25&prof=True&sort=Position&view=Normal&spc=Relaxed&fr=36) by JockerSoft - CPOL
  - Copyright (c) 2006 Jocker

> This program uses code developed by Jocker.  
> http://www.jockersoft.com

- [DirectShowLib.Standard](https://github.com/luthfiampas/DirectShowLib) by luthfiampas - LGPL-2.1
  - [Original Link](https://sourceforge.net/projects/directshownet/)
  - Copyright (c) DirectShow .NET Team

TestData:

- [VJ Clip Collection One](https://archive.org/details/BriareusVJClips) by Briareus - Public Domain
