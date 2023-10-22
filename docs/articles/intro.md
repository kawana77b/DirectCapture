# Introduction

## About

C# library aimed at capturing `AVI` files playable in [DirectShow](https://learn.microsoft.com/en-US/windows/win32/directshow/directshow) as Bitmap.  
Includes a few simple utility classes.

> [!NOTE]
> If you want to perform frame capture against modern, general-purpose video codecs, do not hesitate to consider OpenCV or ffmpeg.  
> This is for niche purposes and experimental. I would do so if I could.

## Install

> [!NOTE]
> Legacy Windows Media Player must be available and configured to play the AVI files you wish to target.

You can use the `.nupkg` on the GitHub's Release page,  
So you can install this package by placing it in your [local sources](https://learn.microsoft.com/en-US/dotnet/core/tools/dotnet-nuget-add-source) etc.

Or clone the repository and include the project.

Currently I am not willing to host this project in NuGet.

> [!WARNING]
> As of `v0.5.0`,  
> For Windows only.  
> Target framework is currently configured with .NET 6 (Windows) and .NET Framework 481.  
> And PlatformTarget is `x64`.
