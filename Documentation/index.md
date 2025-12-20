---
_layout: landing
---

# UAI.LiteRTLM

LiteRT-LM model inference support for Unity Android and Meta Quest apps. Supports v0.8.0+ of the LiteRT-LM Kotlin package for Android.

[![openupm](https://img.shields.io/npm/v/com.uralstech.uai.litertlm?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.uralstech.uai.litertlm/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.uralstech.uai.litertlm)](https://openupm.com/packages/com.uralstech.uai.litertlm/)

## Installation

This package was designed for Unity 6.0 and above. Built and tested in Unity 6.3.

This package uses `System.Threading.Channels`. You can install it through [*NuGetForUnity*](https://github.com/GlitchEnzo/NuGetForUnity)
if you do not have the `System.Threading.Channels` library in your project. See [*How do I use NuGetForUnity?*](https://github.com/GlitchEnzo/NuGetForUnity?tab=readme-ov-file#how-do-i-use-nugetforunity) on how to use NuGetForUnity.

Since this package contains a native AAR plugin, it depends on the [External Dependency Manager for Unity (EDM4U)](https://github.com/googlesamples/unity-jar-resolver) to resolve native dependencies.
You may already have EDM4U if you use Google or Firebase SDKs in your Unity project. If you do not, the installation steps are available
here: [EDM4U - Getting Started](https://github.com/googlesamples/unity-jar-resolver?tab=readme-ov-file#getting-started)

# [OpenUPM](#tab/openupm)

1. Open project settings
2. Select `Package Manager`
3. Add the OpenUPM package registry:
    - Name: `OpenUPM`
    - URL: `https://package.openupm.com`
    - Scope(s)
        - `com.uralstech`
4. Open the Unity Package Manager window (`Window` -> `Package Manager`)
5. Change the registry from `Unity` to `My Registries`
6. Add the `UAI.LiteRTLM` package
    - Install `1.0.0-preview.1-litertlm0.9.0alpha01` (default) for litertlm-android v0.9.0-alpha01
    - Install `1.0.0-preview.1` for litertlm-android v0.8.0

# [Unity Package Manager](#tab/upm)

This will install the latest release of UAI.LiteRTLM. See [*releases*](https://github.com/Uralstech/UAI.LiteRTLM/releases)
on which version of litertlm-android the latest release supports. If you want to use a different version of litertlm-android,
please choose a different installation method.

1. Open the Unity Package Manager window (`Window` -> `Package Manager`)
2. Select the `+` icon and `Add package from git URL...`
3. Paste the UPM branch URL and press enter:
    - `https://github.com/Uralstech/UAI.LiteRTLM.git#upm`

# [GitHub Clone](#tab/github)

1. Clone or download the repository from the desired branch (master, preview/unstable) or tag (`1.0.0-preview.1` for litertlm-android v0.8.0, `1.0.0-preview.1-litertlm0.9.0alpha01` for litertlm-android v0.9.0-alpha01)
2. Drag the package folder `UAI.LiteRTLM/UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm` into your Unity project's `Packages` folder

---

## Preview Versions

Do not use preview versions (i.e. versions that end with "-preview") for production use as they are unstable and untested.

## Documentation

See <https://uralstech.github.io/UAI.LiteRTLM/DocSource/QuickStart.html> or `APIReferenceManual.pdf` and `Documentation.pdf` in the package documentation for the reference manual and tutorial.
