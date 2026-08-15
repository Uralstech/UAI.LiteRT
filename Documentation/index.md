---
_layout: landing
---

# UAI.LiteRTLM

Cross-platform LiteRT-LM bindings for Unity & .NET apps.

[![openupm](https://img.shields.io/npm/v/com.uralstech.uai.litertlm?label=OpenUPM&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.uralstech.uai.litertlm/)
[![openupm](https://img.shields.io/badge/dynamic/json?color=brightgreen&label=Downloads&query=%24.downloads&suffix=%2Fmonth&url=https%3A%2F%2Fpackage.openupm.com%2Fdownloads%2Fpoint%2Flast-month%2Fcom.uralstech.uai.litertlm)](https://openupm.com/packages/com.uralstech.uai.litertlm/)
[![nuget](https://img.shields.io/nuget/v/Uralstech.UAI.LiteRTLM?label=NuGet)](https://www.nuget.org/packages/Uralstech.UAI.LiteRTLM)
[![nuget](https://img.shields.io/nuget/dt/Uralstech.UAI.LiteRTLM?color=brightgreen&label=Downloads)](https://www.nuget.org/packages/Uralstech.UAI.LiteRTLM)

> [!NOTE]
> This repository recently underwent a Git LFS migration. Following the migration, all commits were fully re-signed to ensure the rewritten history has verified signatures.

## Installation

This package was designed for Unity 6.0+ and .NET 8+. Built and tested in Unity 6.5 and .NET 10.

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

# [Unity Package Manager](#tab/upm)

1. Open the Unity Package Manager window (`Window` -> `Package Manager`)
2. Select the `+` icon and `Add package from git URL...`
3. Paste the package URL and press enter:
    - `https://github.com/Uralstech/UAI.LiteRTLM.git?path=UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm`

# [GitHub Clone](#tab/github)

1. Clone or download the repository from the desired branch (master, preview/unstable) or tag
2. Drag the package folder `UAI.LiteRTLM/UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm` into your Unity project's `Packages` folder

# [NuGet](#tab/nuget)

See [NuGet](https://www.nuget.org/packages/Uralstech.UAI.LiteRTLM) for install instructions.
Please read the [Quick Start](https://uralstech.github.io/UAI.LiteRTLM/DocSource/QuickStart.html#nuget-package--net-8)
for APIs specific to the NuGet version of UAI.LiteRTLM.

---

## Documentation

See <https://uralstech.github.io/UAI.LiteRTLM/DocSource/QuickStart.html> or `APIReferenceManual.pdf` and `Documentation.pdf` in the package documentation for the reference manual and tutorial.
