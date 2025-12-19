// Copyright 2025 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
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
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRT
{
    /// <summary>
    /// An array of <see cref="LiteRTContent"/>s.
    /// </summary>
    /// <remarks>
    /// This object manages a native <c>java.util.ArrayList</c> object and must be disposed after usage
    /// OR must be managed by a <see cref="LiteRTMessage"/> to handle its disposal.
    /// </remarks>
    public class LiteRTContentArray : IDisposable
    {
        /// <summary>
        /// The contents contained in this array.
        /// </summary>
        public readonly IReadOnlyCollection<LiteRTContent> Contents;

        /// <summary>
        /// Is disposal of <see cref="Contents"/> handled by this instance?
        /// </summary>
        public readonly bool HandleChildDispose;

        internal readonly AndroidJavaObject _native;
        internal bool Disposed { get; private set; }

        /// <summary>
        /// Creates a new <see cref="LiteRTContentArray"/> object.
        /// </summary>
        /// <param name="contents">The contents contained in this array.</param>
        /// <param name="handleChildDispose">Should disposal of <paramref name="contents"/> be handled by this instance?</param>
        public LiteRTContentArray(IReadOnlyCollection<LiteRTContent> contents, bool handleChildDispose = true)
        {
            Contents = contents;
            HandleChildDispose = handleChildDispose;
            _native = new AndroidJavaObject("java.util.ArrayList");

            try
            {
                foreach (LiteRTContent content in contents)
                {
                    if (content.Disposed)
                        throw new ObjectDisposedException(nameof(LiteRTContent));

                    _native.Call<bool>("add", content._native);
                }
            }
            catch
            {
                _native.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a new <see cref="LiteRTContentArray"/> from an existing one.
        /// </summary>
        public LiteRTContentArray(LiteRTContentArray other)
        {
            if (other.Disposed)
                throw new ObjectDisposedException(nameof(LiteRTContentArray));

            _native = new AndroidJavaObject(other._native.GetRawObject());

            try
            {
                List<LiteRTContent> contents = new();
                HandleChildDispose = true;

                foreach (LiteRTContent content in other.Contents)
                    contents.Add(new LiteRTContent(content));

                Contents = contents;
            }
            catch
            {
                _native.Dispose();
                throw;
            }
        }

        internal LiteRTContentArray(AndroidJavaObject native, int size)
        {
            _native = native;
            HandleChildDispose = true;

            try
            {
                LiteRTContent[] contents = new LiteRTContent[size];
                for (int i = 0; i < size; i++)
                {
                    contents[i] = new LiteRTContent(native.Call<AndroidJavaObject>("get", i)
                        ?? throw new NullReferenceException($"Could not access contents array element at index {i}."));
                }
                
                Contents = contents;
            }
            catch
            {
                native.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;
            _native.Dispose();
            
            GC.SuppressFinalize(this);
            if (!HandleChildDispose)
                return;

            foreach (LiteRTContent content in Contents)
                content.Dispose();
        }

        public static implicit operator LiteRTContentArray(LiteRTContent[] current) => new(current, true);
        public static implicit operator LiteRTContentArray(List<LiteRTContent> current) => new(current, true);
    }
}
