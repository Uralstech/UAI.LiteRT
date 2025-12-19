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
    public class Message : IDisposable
    {
        internal const string ConversationWrapperClass = "com.uralstech.uai.litertlm.ConversationWrapper";

        /// <summary>
        /// The managed content array stored by this object.
        /// </summary>
        public readonly ContentArray Contents;

        /// <summary>
        /// Is disposal of <see cref="Contents"/> handled by this instance?
        /// </summary>
        public readonly bool HandleContentsDispose;

        internal readonly AndroidJavaObject _native;
        internal bool Disposed { get; private set; }

        private Message(ContentArray contents, bool handleContentsDispose)
        {
            Contents = contents;
            HandleContentsDispose = handleContentsDispose;

            using AndroidJavaClass nativeWrapper = new(ConversationWrapperClass);
            _native = nativeWrapper.CallStatic<AndroidJavaObject>("messageOf", contents._native);
        }

        private Message(string content)
        {
            using AndroidJavaClass nativeWrapper = new(ConversationWrapperClass);
            _native = nativeWrapper.CallStatic<AndroidJavaObject>("messageOf", content);

            try
            {
                AndroidJavaObject nativeContents = _native.Get<AndroidJavaObject>("contents")
                    ?? throw new NullReferenceException("Could not get native contents array from message.");

                Contents = new ContentArray(nativeContents, 1);
            }
            catch
            {
                _native.Dispose();
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
        /// For more detail on how <see cref="Contents"/> is semi-deep copied, see <see cref="ContentArray(ContentArray)"/>.
        /// </remarks>

        public Message(Message other)
        {
            if (other.Disposed)
                throw new ObjectDisposedException(nameof(Message));

            _native = new AndroidJavaObject(other._native.GetRawObject());
            
            try
            {
                Contents = new ContentArray(other.Contents);
                HandleContentsDispose = true;
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

        /// <inheritdoc/>
        public override string ToString() => string.Join(string.Empty, Contents.Elements);


        /// <summary>
        /// Creates a <see cref="Message"/> from the <see cref="ContentArray"/>.
        /// </summary>
        /// <param name="handleContentsDispose">Should the message object handle the disposing of the array?</param>
        public static Message Of(ContentArray contents, bool handleContentsDispose = true)
        {
            return !contents.Disposed
                ? new Message(contents, handleContentsDispose)
                : throw new ObjectDisposedException(nameof(ContentArray));
        }


        /// <summary>
        /// Creates a <see cref="Message"/> from a text string.
        /// </summary>
        public static Message Of(string textMessage) => new(textMessage);
    }
}
