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
using System.Threading;
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    /// <summary>
    /// Callback for <see cref="Session.RunDecodeAsync(AsyncDecodeCallback?, CancellationToken)"/>.
    /// </summary>
    public sealed class AsyncDecodeCallback : AndroidJavaProxy
    {
        /// <summary>
        /// Called when the decode operation is completed with the result.
        /// </summary>
        public event Action<string>? OnDone;

        public AsyncDecodeCallback() : base("com.uralstech.uai.litertlm.SessionWrapper$AsyncDecodeCallback") { }

        /// <inheritdoc/>
        public override IntPtr Invoke(string methodName, IntPtr javaArgs)
        {
            if (methodName == "onDone")
            {
                OnDone?.Invoke(JNIHelpers.UnwrapStringFromArray(javaArgs, 0)!);
                return IntPtr.Zero;
            }

            return base.Invoke(methodName, javaArgs);
        }
    }
}
