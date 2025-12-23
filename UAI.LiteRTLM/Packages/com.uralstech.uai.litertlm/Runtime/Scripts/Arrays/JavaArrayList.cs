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
    /// An array of <see cref="T"/>s backed by a native <c>java.util.ArrayList</c>.
    /// </summary>
    /// <remarks>
    /// This object manages a native <c>java.util.ArrayList</c> object and must be disposed after usage.
    /// </remarks>
    public abstract class JavaArrayList<T> : JavaObject
        where T : JavaObject
    {
        /// <summary>
        /// The elements contained in this array.
        /// </summary>
        public readonly IReadOnlyList<T> Elements;

        /// <summary>
        /// Is disposal of the elements of <see cref="Elements"/> handled by this instance?
        /// </summary>
        public readonly bool HandleElementsDispose;

        /// <inheritdoc/>
        public override AndroidJavaObject Handle { get; }

        /// <summary>
        /// Creates a new <see cref="JavaArrayList{T}"/> object.
        /// </summary>
        /// <param name="elements">The elements contained in this array.</param>
        /// <param name="handleChildDispose">Should disposal of <paramref name="elements"/> be handled by this instance?</param>
        public JavaArrayList(IReadOnlyList<T> elements, bool handleChildDispose = true)
        {
            Elements = elements;
            HandleElementsDispose = handleChildDispose;
            Handle = new AndroidJavaObject("java.util.ArrayList");

            try
            {
                int count = elements.Count;
                for (int i = 0; i < count; i++)
                {
                    T element = elements[i];
                    if (element.IsDisposed)
                        throw new ObjectDisposedException(nameof(T));

                    Handle.Call<bool>("add", element.Handle);
                }
            }
            catch
            {
                Handle.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a new <see cref="JavaArrayList{T}"/> from an existing one.
        /// </summary>
        /// <remarks>
        /// This creates a semi-deep copy of <paramref name="other"/>. A new <see cref="AndroidJavaObject"/>
        /// which refers to the same native Kotlin object as <paramref name="other"/> is created, and a shallow
        /// copy of each of <paramref name="other"/>'s elements is added into a new array and stored as <see cref="Elements"/>.
        /// The new instance's <see cref="HandleElementsDispose"/> is set to <see langword="true"/>.
        /// 
        /// For more detail on how the elements are shallow copied, see the implementation of <see cref="ElementFactory(T)"/>.
        /// </remarks>
        public JavaArrayList(JavaArrayList<T> other)
        {
            if (other.IsDisposed)
                throw new ObjectDisposedException(nameof(JavaArrayList<T>));

            Handle = new AndroidJavaObject(other.Handle.GetRawObject());
            HandleElementsDispose = true;

            int count = other.Elements.Count;
            T[] elements = new T[count];

            try
            {
                for (int i = 0; i < count; i++)
                    elements[i] = ElementFactory(other.Elements[i]);
                
                Elements = elements;
            }
            catch
            {
                DisposeArray(elements);
                Handle.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a new <see cref="JavaArrayList{T}"/> from a native handle.
        /// </summary>
        /// <param name="native">The native handle.</param>
        /// <param name="size">The size of the array.</param>
        internal protected JavaArrayList(AndroidJavaObject native, int size)
        {
            Handle = native;
            HandleElementsDispose = true;
            T[] elements = new T[size];

            try
            {
                for (int i = 0; i < size; i++)
                {
                    elements[i] = ElementFactory(native.Call<AndroidJavaObject>("get", i)
                        ?? throw new NullReferenceException($"Could not access array element at index {i}."));
                }
                
                Elements = elements;
            }
            catch
            {
                DisposeArray(elements);
                native.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a new <see cref="T"/> from a native handle.
        /// </summary>
        protected abstract T ElementFactory(AndroidJavaObject native);

        /// <summary>
        /// Creates a new <see cref="T"/> from an existing instance.
        /// </summary>
        protected abstract T ElementFactory(T other);

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            base.Dispose();
            if (HandleElementsDispose)
                DisposeArray(Elements);
        }

        /// <summary>
        /// Disposes all elements of <paramref name="array"/>, with null-checking.
        /// </summary>
        protected void DisposeArray(IReadOnlyList<T> array)
        {
            int count = array.Count;
            for (int i = 0; i < count; i++)
            {
                T element = array[i];
                if (element != default(T))
                    element.Dispose();
            }
        }
    }
}