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
using System.Text;

namespace Uralstech.UAI.LiteRTLM.Native;

internal static class PackageUnsafeUtils
{
    /// <remarks>Use EXCLUSIVELY in <c>using</c> statements</remarks>
    internal readonly struct TempMem : IDisposable
    {
        public readonly IntPtr Ptr;
        public readonly UIntPtr Size;

        public TempMem(IntPtr ptr, UIntPtr size)
        {
            Ptr = ptr;
            Size = size;
        }

        public unsafe void Dispose() =>
            NativeMemory.Free((void*)Ptr);
    }
    
    /// <remarks>Allocates memory for SHORT-TERM usage.</remarks>
    public static unsafe TempMem AllocateStringUTF8(ReadOnlySpan<char> str)
    {
        int strSize = Encoding.UTF8.GetByteCount(str);
        UIntPtr totalSize = (UIntPtr)(strSize + 1);
        
        void* allocated = NativeMemory.Alloc(totalSize);
        Span<byte> allocatedSpan = new(allocated, (int)totalSize);
        
        Encoding.UTF8.GetBytes(str, allocatedSpan[..strSize]);
        allocatedSpan[strSize] = 0;

        return new TempMem((IntPtr)allocated, totalSize);
    }
}