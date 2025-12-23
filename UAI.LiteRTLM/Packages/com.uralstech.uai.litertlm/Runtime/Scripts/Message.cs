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
    /// This object manages a native <c>com.google.ai.edge.litertlm.Message</c> object and must be disposed after usage.
    /// </remarks>
    public sealed class Message : JavaObject
    {
        private const string ConversationWrapperClass = "com.uralstech.uai.litertlm.ConversationWrapper";

        /// <summary>
        /// The contents of this message.
        /// </summary>
        public readonly ContentArray Contents;

        /// <summary>
        /// Is disposal of <see cref="Contents"/> handled by this instance?
        /// </summary>
        public readonly bool HandleContentsDispose;

        /// <inheritdoc/>
        public override AndroidJavaObject Handle { get; }

        private Message(ContentArray contents, bool handleContentsDispose)
        {
            Contents = contents;
            HandleContentsDispose = handleContentsDispose;

            using AndroidJavaClass nativeWrapper = new(ConversationWrapperClass);
            Handle = nativeWrapper.CallStatic<AndroidJavaObject>("messageOf", contents.Handle)
                ?? throw new NullReferenceException("Could not create message object from wrapper.");
        }

        private Message(string content)
        {
            using AndroidJavaClass nativeWrapper = new(ConversationWrapperClass);
            Handle = nativeWrapper.CallStatic<AndroidJavaObject>("messageOf", content)
                ?? throw new NullReferenceException("Could not create message object from wrapper.");

            try
            {
                AndroidJavaObject nativeContents = Handle.Get<AndroidJavaObject>("contents")
                    ?? throw new NullReferenceException("Could not get contents array from message.");

                Contents = new ContentArray(nativeContents, 1);
            }
            catch
            {
                Handle.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Message"/> from an existing one.
        /// </summary>
        /// <remarks>
        /// This creates a semi-deep copy of <paramref name="other"/>. A new <see cref="AndroidJavaObject"/>
        /// which refers to the same native Kotlin object as <paramref name="other"/> is created, and a semi-deep
        /// copy of <paramref name="other"/>'s <see cref="Contents"/> is created.
        /// The new instance's <see cref="HandleContentsDispose"/> is set to <see langword="true"/>.
        /// 
        /// For more detail on how <see cref="Contents"/> is semi-deep copied, see <see cref="ContentArray(JavaArrayList{Content})"/>.
        /// </remarks>
        public Message(Message other)
        {
            if (other.IsDisposed)
                throw new ObjectDisposedException(nameof(Message));

            Handle = new AndroidJavaObject(other.Handle.GetRawObject());
            
            try
            {
                Contents = new ContentArray(other.Contents);
                HandleContentsDispose = true;
            }
            catch
            {
                Handle.Dispose();
                throw;
            }
        }

        internal Message(AndroidJavaObject native)
        {
            Handle = native;

            bool shouldDisposeNativeContents = true;
            AndroidJavaObject nativeContents = native.Get<AndroidJavaObject>("contents")
                ?? throw new NullReferenceException("Could not get contents array from message.");
            
            try
            {
                int nativeContentsSize = nativeContents.Get<int>("size");
                if (nativeContentsSize == 0)
                    throw new InvalidOperationException("Contents array was empty.");

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
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            base.Dispose();
            if (HandleContentsDispose)
                Contents.Dispose();
        }

        /// <inheritdoc/>
        public override string ToString() => string.Join(string.Empty, Contents.Elements);

        /// <summary>
        /// Creates a <see cref="Message"/> from the <see cref="ContentArray"/>.
        /// </summary>
        /// <param name="handleContentsDispose">Should the message object handle the disposing of the array?</param>
        public static Message Of(ContentArray contents, bool handleContentsDispose = true)
        {
            return !contents.IsDisposed
                ? new Message(contents, handleContentsDispose)
                : throw new ObjectDisposedException(nameof(ContentArray));
        }

        /// <summary>
        /// Creates a <see cref="Message"/> from a text string.
        /// </summary>
        public static Message Of(string textMessage) => new(textMessage);
    }
}