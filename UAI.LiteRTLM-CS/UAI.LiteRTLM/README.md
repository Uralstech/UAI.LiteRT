# UAI.LiteRTLM

Cross-platform LiteRT-LM bindings for Unity & .NET apps.

[![openupm](https://img.shields.io/npm/v/com.uralstech.uai.litertlm?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.uralstech.uai.litertlm/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.uralstech.uai.litertlm)](https://openupm.com/packages/com.uralstech.uai.litertlm/)
[![nuget](https://img.shields.io/nuget/v/Uralstech.UAI.LiteRTLM?label=NuGet)](https://www.nuget.org/packages/Uralstech.UAI.LiteRTLM)

## Notice

Starting with `2.2.0-preview.2`, UAI.LiteRTLM is also available on NuGet for .NET 8+! It supports all the same platforms as the Unity package, and
works with MAUI. The Unity package is still the main development target, so the NuGet/.NET package may sometimes lag behind and not have all the
latest features yet.

Because the package includes some Unity-specific workarounds, using the NuGet package requires a small amount of additional setup.
Before creating the `Engine`, call `Accelerators.LoadNativeLibraries()` to pre-load the native accelerator libraries.
You only need to call this method once; subsequent calls are no-ops.

## Documentation

See <https://uralstech.github.io/UAI.LiteRTLM> for the reference manual and tutorial.
