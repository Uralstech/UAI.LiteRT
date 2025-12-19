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
    /// Represents a message in the conversation. A message can contain multiple <see cref="Content"/>s.
    /// </summary>
    /// <remarks>
    /// This can store a <see cref="ContentArray"/> (<see cref="Contents"/>) OR a <see cref="string"/> message (<see cref="TextMessage"/>).
    /// This object manages a native <c>com.google.ai.edge.litertlm.Message</c> object and must be disposed after usage.
    /// </remarks>
    public class Message : IDisposable
    {
        /// <summary>
        /// The managed content array stored by this object.
        /// </summary>
        public readonly ContentArray? Contents;

        /// <summary>
        /// Is disposal of <see cref="Contents"/> handled by this instance?
        /// </summary>
        public readonly bool HandleContentsDispose;

        /// <summary>
        /// The single text content handled by this object.
        /// </summary>
        public readonly string? TextMessage;

        internal readonly AndroidJavaObject _native;
        internal bool Disposed { get; private set; }

        private Message(ContentArray? contents, string? textMessage, bool handleContentsDispose)
        {
            Contents = contents;
            TextMessage = textMessage;
            HandleContentsDispose = handleContentsDispose;

            if (contents is not null && contents.Disposed)
                throw new ObjectDisposedException(nameof(ContentArray));

            using AndroidJavaClass nativeWrapper = new("com.uralstech.uai.litertlm.ConversationWrapper");
            _native = nativeWrapper.CallStatic<AndroidJavaObject>("messageOf", (object?)contents?._native ?? textMessage);
        }

        /// <summary>
        /// Creates a new <see cref="Message"/> from an existing one.
        /// </summary>
        /// <remarks>
        /// This creates a semi-deep copy of <paramref name="other"/>. A new <see cref="AndroidJavaObject"/>
        /// which refers to the same native Kotlin object as <paramref name="other"/> is created, and a semi-deep
        /// copy of <paramref name="other"/>'s <see cref="Contents"/> is created. <see cref="TextMessage"/> is copied by reference.
        /// The new instance's <see cref="HandleContentsDispose"/> is set to <see langword="true"/>.
        /// 
        /// For more detail on how <see cref="Contents"/> is semi-deep copied, see <see cref="ContentArray(ContentArray)"/>.
        /// </remarks>

        public Message(Message other)
        {
            if (other.Disposed)
                throw new ObjectDisposedException(nameof(Message));

            _native = new AndroidJavaObject(other._native.GetRawObject());
            
            try
            {
                TextMessage = other.TextMessage;

                if (other.Contents is not null)
                {
                    Contents = new ContentArray(other.Contents);
                    HandleContentsDispose = true;
                }
            }
            catch
            {
                _native.Dispose();
                throw;
            }
        }

        internal Message(AndroidJavaObject native)
        {
            _native = native;

            bool shouldDisposeNativeContents = true;
            AndroidJavaObject nativeContents = native.Get<AndroidJavaObject>("contents")
                ?? throw new NullReferenceException("Could not get native contents array from message.");
            
            try
            {
                int nativeContentsSize = nativeContents.Get<int>("size");
                if (nativeContentsSize == 0)
                    throw new InvalidOperationException("Contents array was empty.");

                if (nativeContentsSize == 1)
                {
                    using AndroidJavaObject element = nativeContents.Call<AndroidJavaObject>("get", 0)
                        ?? throw new NullReferenceException("Could not access contents array element.");

                    using AndroidJavaClass textClass = new(Content.TextContentClass);
                    if (AndroidJNI.IsInstanceOf(element.GetRawObject(), textClass.GetRawClass()))
                    {
                        TextMessage = element.Get<string>("text")
                            ?? throw new NullReferenceException("Text content was null.");

                        HandleContentsDispose = false;
                        return;
                    }
                }

                shouldDisposeNativeContents = false;

                Contents = new ContentArray(nativeContents, nativeContentsSize);
                HandleContentsDispose = true;
            }
            catch
            {
                native.Dispose();
                throw;
            }
            finally
            {
                if (shouldDisposeNativeContents)
                    nativeContents.Dispose();
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
            if (!HandleContentsDispose)
                return;

            Contents!.Dispose();
        }

        /// <summary>
        /// Creates a <see cref="Message"/> from the <see cref="ContentArray"/>.
        /// </summary>
        /// <param name="handleContentsDispose">Should the message object handle the disposing of the array?</param>
        public static Message Of(ContentArray contents, bool handleContentsDispose = true) =>
            new(contents, textMessage: null, handleContentsDispose);
        
        /// <summary>
        /// Creates a <see cref="Message"/> from a text string.
        /// </summary>
        public static Message Of(string textMessage) =>
            new(contents: null, textMessage, handleContentsDispose: false);
    }
}
