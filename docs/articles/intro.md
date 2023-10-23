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

> [!WARNING]
> As of `v0.5.1`,  
> For Windows only.  
> Target framework is currently configured with .NET 6 (Windows) and .NET Framework 481.  
> And PlatformTarget is `x64`.
