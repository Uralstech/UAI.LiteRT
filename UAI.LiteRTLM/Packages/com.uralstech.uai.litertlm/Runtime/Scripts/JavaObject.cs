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
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    /// <summary>
    /// A C# object backed by a native Java/Kotlin object.
    /// </summary>
    public abstract class JavaObject : IDisposable
    {
        /// <summary>
        /// The handle to the native object.
        /// </summary>
        public abstract AndroidJavaObject Handle { get; }

        /// <summary>
        /// Has the handle been disposed?
        /// </summary>
        public bool IsDisposed { get; private set; }

        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(JavaObject));
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Handle.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}