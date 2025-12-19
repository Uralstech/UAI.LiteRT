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
namespace Uralstech.UAI.LiteRTLM
{
    /// <summary>
    /// An array of <see cref="Content"/>s.
    /// </summary>
    /// <remarks>
    /// This object manages a native <c>java.util.ArrayList</c> object and must be disposed after usage
    /// OR must be managed by a <see cref="Message"/> to handle its disposal.
    /// </remarks>
    public class ContentArray : IDisposable
    {
        /// <summary>
        /// The contents contained in this array.
        /// </summary>
        public readonly IReadOnlyList<Content> Elements;

        /// <summary>
        /// Is disposal of the elements of <see cref="Elements"/> handled by this instance?
        /// </summary>
        public readonly bool HandleElementsDispose;

        internal readonly AndroidJavaObject _native;
        internal bool Disposed { get; private set; }

        /// <summary>
        /// Creates a new <see cref="ContentArray"/> object.
        /// </summary>
        /// <param name="contents">The contents contained in this array.</param>
        /// <param name="handleChildDispose">Should disposal of <paramref name="contents"/> be handled by this instance?</param>
        public ContentArray(IReadOnlyList<Content> contents, bool handleChildDispose = true)
        {
            Elements = contents;
            HandleElementsDispose = handleChildDispose;
            _native = new AndroidJavaObject("java.util.ArrayList");

            try
            {
                foreach (Content content in contents)
                {
                    if (content.Disposed)
                        throw new ObjectDisposedException(nameof(Content));

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
        /// Creates a new <see cref="ContentArray"/> from an existing one.
        /// </summary>
        /// <remarks>
        /// This creates a semi-deep copy of <paramref name="other"/>. A new <see cref="AndroidJavaObject"/>
        /// which refers to the same native Kotlin object as <paramref name="other"/> is created, and a shallow
        /// copy of each of <paramref name="other"/>'s elements is added into a new array and stored as <see cref="Elements"/>.
        /// The new instance's <see cref="HandleElementsDispose"/> is set to <see langword="true"/>.
        /// 
        /// For more detail on how the elements are shallow copied, see <see cref="Content(Content)"/>.
        /// </remarks>
        public ContentArray(ContentArray other)
        {
            if (other.Disposed)
                throw new ObjectDisposedException(nameof(ContentArray));

            _native = new AndroidJavaObject(other._native.GetRawObject());

            try
            {
                List<Content> contents = new();
                HandleElementsDispose = true;

                foreach (Content content in other.Elements)
                    contents.Add(new Content(content));

                Elements = contents;
            }
            catch
            {
                _native.Dispose();
                throw;
            }
        }

        internal ContentArray(AndroidJavaObject native, int size)
        {
            _native = native;
            HandleElementsDispose = true;

            try
            {
                Content[] contents = new Content[size];
                for (int i = 0; i < size; i++)
                {
                    contents[i] = new Content(native.Call<AndroidJavaObject>("get", i)
                        ?? throw new NullReferenceException($"Could not access contents array element at index {i}."));
                }
                
                Elements = contents;
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
            if (!HandleElementsDispose)
                return;

            foreach (Content content in Elements)
                content.Dispose();
        }

        public static implicit operator ContentArray(Content[] current) => new(current, true);
        public static implicit operator ContentArray(List<Content> current) => new(current, true);
    }
}
