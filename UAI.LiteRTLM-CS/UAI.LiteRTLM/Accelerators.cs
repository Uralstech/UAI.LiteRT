// Copyright 2026 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using Uralstech.UAI.LiteRTLM.Native;

namespace Uralstech.UAI.LiteRTLM;

/// <summary>Contains methods for managing the accelerators.</summary>
public static class Accelerators
{
    private static int s_counter;

    /// <summary>Loads the native accelerator libraries. Call this before creating a new <see cref="Engine"/>.</summary>
    public static void LoadNativeLibraries()
    {
        if (Interlocked.Exchange(ref s_counter, 1) != 0)
            return;

        if (OperatingSystem.IsIOS() || OperatingSystem.IsMacOS())
        {
            NativeLibrary.Load("libLiteRtTopKMetalSampler", typeof(NativeAPI).Assembly, DllImportSearchPath.AssemblyDirectory);
            NativeLibrary.Load("libLiteRtMetalAccelerator", typeof(NativeAPI).Assembly, DllImportSearchPath.AssemblyDirectory);
        }

        if (OperatingSystem.IsWindows())
        {
            NativeLibrary.Load("libLiteRtWebGpuAccelerator", typeof(NativeAPI).Assembly, DllImportSearchPath.AssemblyDirectory);
            NativeLibrary.Load("libLiteRtTopKWebGpuSampler", typeof(NativeAPI).Assembly, DllImportSearchPath.AssemblyDirectory);
        }
        
        if (OperatingSystem.IsAndroid())
        {
            NativeLibrary.Load("libLiteRtOpenClAccelerator", typeof(NativeAPI).Assembly, DllImportSearchPath.AssemblyDirectory);
            NativeLibrary.Load("libLiteRtTopKOpenClSampler", typeof(NativeAPI).Assembly, DllImportSearchPath.AssemblyDirectory);
        }
    }
}