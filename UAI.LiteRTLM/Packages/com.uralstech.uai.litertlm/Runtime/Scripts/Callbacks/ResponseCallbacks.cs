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
    /// Callbacks for receiving streaming responses.
    /// </summary>
    public sealed class ResponseCallbacks : AndroidJavaProxy
    {
        /// <summary>
        /// Called when the stream is complete.
        /// </summary>
        public event Action? OnDone;

        /// <summary>
        /// Called when an error occurs, with the Kotlin <c>Throwable</c> and any error message.
        /// </summary>
        /// <remarks>
        /// The error will be a <c>java.util.concurrent.CancellationException</c> if the stream was cancelled normally,
        /// and a <c>LiteRtLmJniException</c> for other errors.
        /// 
        /// The <see cref="AndroidJavaObject"/> is disposed of immediately after the event's Invoke is completed.
        /// </remarks>
        public event Action<AndroidJavaObject, string?>? OnError;

        /// <summary>
        /// Called when a new response is available, with the message chunk.
        /// </summary>
        public event Action<string>? OnNext;

        public ResponseCallbacks() : base("com.google.ai.edge.litertlm.ResponseCallback") { }

        /// <inheritdoc/>
        public override IntPtr Invoke(string methodName, IntPtr javaArgs)
        {
            switch (methodName)
            {
                case "onDone":
                    OnDone?.Invoke();
                    return IntPtr.Zero;

                case "onError":
                    using (AndroidJavaObject error = JNIHelpers.UnwrapObjectFromArray(javaArgs, 0))
                    {
                        string? errorMessage = error.Call<string>("getMessage");

                        Debug.LogError($"{nameof(ResponseCallbacks)}: Could not process streamed inference due to error: {errorMessage}");
                        OnError?.Invoke(error, errorMessage);
                    }
                    
                    return IntPtr.Zero;

                case "onNext":
                    OnNext?.Invoke(JNIHelpers.UnwrapStringFromArray(javaArgs, 0)!);
                    return IntPtr.Zero;
            }

            return base.Invoke(methodName, javaArgs);
        }
    }
}
