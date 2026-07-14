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
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRTLM.Native
{
    internal static class UnsafeUtils
    {
        private static readonly int s_byteAlignment = UnsafeUtility.AlignOf<byte>();
        
        /// <remarks>Use EXCLUSIVELY in <c>using</c> statements</remarks>
        internal readonly struct TempMem : IDisposable
        {
            public readonly IntPtr Ptr;
            public readonly UIntPtr Size;
            private readonly Allocator _allocator;

            public TempMem(IntPtr ptr, UIntPtr size, Allocator allocator)
            {
                Ptr = ptr;
                Size = size;
                _allocator = allocator;
            }

            public unsafe void Dispose() =>
                UnsafeUtility.Free((void*)Ptr, _allocator);
        }

        public static string MarshalStringUTF8(IntPtr ptr) =>
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
            long copyLength = (long)length <= dst.Length
                ? (long)length : dst.Length;

            fixed (T* dstPtr = dst)
                UnsafeUtility.MemCpy(dstPtr, (T*)src, copyLength * UnsafeUtility.SizeOf<T>());

            return copyLength;
        }
        
        /// <remarks>Allocates memory for SHORT-TERM usage.</remarks>
        public static unsafe TempMem AllocateStringUTF8(ReadOnlySpan<char> str)
        {
            int strSize = Encoding.UTF8.GetByteCount(str);
            int totalSize = strSize + 1;
            
            Allocator allocator = ChooseAllocator(totalSize);
            void* allocated = UnsafeUtility.Malloc(totalSize, s_byteAlignment, allocator);
            Span<byte> allocatedSpan = new(allocated, totalSize);
            
            Encoding.UTF8.GetBytes(str, allocatedSpan[..strSize]);
            allocatedSpan[strSize] = 0;

            return new TempMem((IntPtr)allocated, (UIntPtr)totalSize, allocator);
        }

        private static Allocator ChooseAllocator(int dataSize)
        {
            return dataSize switch
            {
                <= 5 * 1024 when Awaitable.MainThreadAsync().IsCompleted => Allocator.Temp,
                > 20 * 1024 * 1024 => Allocator.TempJob,
                _ => Allocator.Persistent,
            };
        }
    }
}