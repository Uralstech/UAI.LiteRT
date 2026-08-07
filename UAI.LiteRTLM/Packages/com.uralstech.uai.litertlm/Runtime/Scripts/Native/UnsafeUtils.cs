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

#nullable enable
namespace Uralstech.UAI.LiteRTLM.Native
{
    internal static class UnsafeUtils
    {
        public static string? MarshalStringUTF8(IntPtr ptr) =>
            Marshal.PtrToStringUTF8(ptr);

        public static IntPtr MarshalDelegate<T>(T @delegate) where T : Delegate =>
            Marshal.GetFunctionPointerForDelegate(@delegate);

        public static unsafe T[] CopyFrom<T>(IntPtr ptr, int length)
            where T : unmanaged
        {
            T[] copy = new T[length];
            
            ReadOnlySpan<T> data = new((void*)ptr, length);
            data.CopyTo(copy);

            return copy;
        }

        public static unsafe long CopyTo<T>(IntPtr src, UIntPtr length, Span<T> dst)
            where T : unmanaged
        {
            int copyLength = (int)length <= dst.Length
                ? (int)length : dst.Length;
            
            ReadOnlySpan<T> srcSpan = new((void*)src, copyLength);
            srcSpan.CopyTo(dst);
            return copyLength;
        }
    }
}