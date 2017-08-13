#addin nuget:https://nuget.org/api/v2/?package=Cake.XCode
#addin nuget:https://nuget.org/api/v2/?package=Cake.FileHelpers
#addin nuget:https://nuget.org/api/v2/?package=Cake.Xamarin
#addin nuget:https://nuget.org/api/v2/?package=Cake.Xamarin.Build

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default").ToUpper();

////////////////////////////////////////////////////////////////////////////////////////////////////
// TOOLS & FUNCTIONS - the bits to make it all work
////////////////////////////////////////////////////////////////////////////////////////////////////

EnsureDirectoryExists ("./output");
LogSystemInfo ();

var NugetToolPath = File ("./tools/nuget.exe");

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - 
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("externals")
    .Does (() => 
{
    if (!IsRunningOnUnix ()) {
        return;
    }

    // iOS
    BuildXCodeFatLibrary (
        xcodeProject: "./SimplePing.xcodeproj",
        target: "SimplePing",
        workingDirectory: "./external/Xamarin.SimplePing/");

    // macOS
    if (!FileExists ("./external/Xamarin.SimplePing/SimplePingMac.dylib")) {
        XCodeBuild (new XCodeBuildSettings {
            Project = "./external/Xamarin.SimplePing/SimplePing.xcodeproj",
            Target = "SimplePingMac",
            Sdk = "macosx",
            Arch = "x86_64",
            Configuration = "Release",
        });
        CopyFile (
            "./external/Xamarin.SimplePing/build/Release/SimplePingMac.dylib",
            "./external/Xamarin.SimplePing/SimplePingMac.dylib");
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// LIBS - 
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("libs")
    .IsDependentOn ("externals")
    .Does (() => 
{
    MSBuild ("./source/Xamarin.SimplePing.sln", s => s
        .SetConfiguration ("Release")
        .SetMSBuildPlatform (MSBuildPlatform.x86));

    CopyFileToDirectory ("./source/Xamarin.SimplePing.iOS/bin/Release/Xamarin.SimplePing.iOS.dll", "./output");
    CopyFileToDirectory ("./source/Xamarin.SimplePing.Mac/bin/Release/Xamarin.SimplePing.Mac.dll", "./output");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// PACKAGING - 
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("nuget")
    .IsDependentOn ("libs")
    .Does (() => 
{
    NuGetPack ("./nuget/Xamarin.SimplePing.nuspec", new NuGetPackSettings { 
        Verbosity = NuGetVerbosity.Detailed,
        OutputDirectory = "./output",
        BasePath = "./",
        ToolPath = NugetToolPath
    });
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES - 
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("samples")
    .IsDependentOn ("libs")
    .Does (() => 
{
    MSBuild ("./samples/SimplePingSample.iOS/SimplePingSample.iOS.sln", s => s
        .SetConfiguration ("Release")
        .WithProperty ("Platform", new [] { "iPhone" }));

    MSBuild ("./samples/SimplePingSample.Mac/SimplePingSample.Mac.sln", s => s
        .SetConfiguration ("Release"));
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - 
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("clean")
    .Does (() => 
{
    CleanDirectories ("./output");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// START - 
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("Fast")
    .IsDependentOn("externals")
    .IsDependentOn("libs")
    .IsDependentOn("nuget");

Task("Default")
    .IsDependentOn("externals")
    .IsDependentOn("libs")
    .IsDependentOn("nuget")
    .IsDependentOn("samples");

RunTarget (target);