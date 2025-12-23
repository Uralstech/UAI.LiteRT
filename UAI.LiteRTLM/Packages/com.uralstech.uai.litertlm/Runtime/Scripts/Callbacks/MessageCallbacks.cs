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
    /// Callbacks for receiving streaming message responses.
    /// </summary>
    public sealed class MessageCallbacks : AndroidJavaProxy
    {
        /// <summary>
        /// Called when all message chunks are sent for a given SendMessageAsync call.
        /// </summary>
        public event Action? OnDone;

        /// <summary>
        /// Called when an error occurs during the response streaming process, with the Kotlin <c>Throwable</c> and any error message.
        /// </summary>
        /// <remarks>
        /// The <see cref="AndroidJavaObject"/> is disposed of immediately after the event's Invoke is completed.
        /// </remarks>
        public event Action<AndroidJavaObject, string?>? OnError;

        /// <summary>
        /// Called when a new message chunk is available from the model, along with the message.
        /// </summary>
        /// <remarks>
        /// This method may be called multiple times for a single SendMessageAsync call as the model streams its response.
        /// The <see cref="Message"/> object is disposed of immediately after the event's Invoke is completed.
        /// </remarks>
        public event Action<Message>? OnMessage;

        public MessageCallbacks() : base("com.google.ai.edge.litertlm.MessageCallback") { }

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

                        Debug.LogError($"{nameof(MessageCallbacks)}: Could not process async inference due to error: {errorMessage}");
                        OnError?.Invoke(error, errorMessage);
                    }
                    
                    return IntPtr.Zero;

                case "onMessage":
                    if (OnMessage is null)
                        return IntPtr.Zero;

                    using (Message wrapper = new(JNIHelpers.UnwrapObjectFromArray(javaArgs, 0)))
                        OnMessage.Invoke(wrapper);
                        
                    return IntPtr.Zero;
            }

            return base.Invoke(methodName, javaArgs);
        }
    }
}
