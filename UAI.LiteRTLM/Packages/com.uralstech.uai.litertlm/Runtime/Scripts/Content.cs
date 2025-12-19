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
using System.Runtime.InteropServices;
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    /// <summary>
    /// Represents a content in the <see cref="Message"/> of the conversation.
    /// </summary>
    /// <remarks>
    /// This can store text or binary content, based on its <see cref="Type"/>.
    /// This object manages a native <c>com.google.ai.edge.litertlm.Content</c> object and must be disposed after usage
    /// OR must be managed by a <see cref="ContentArray"/> to handle its disposal.
    /// </remarks>
    public class Content : IDisposable
    {
        /// <summary>
        /// The data type of the <see cref="Content"/>.
        /// </summary>
        public enum ContentType
        {
            /// <summary>Text.</summary>
            Text        = 0,

            /// <summary>Image provided as raw bytes.</summary>
            ImageBytes  = 1,

            /// <summary>Image provided by a file.</summary>
            ImagePath   = 2,

            /// <summary>Audio provided as raw bytes.</summary>
            AudioBytes  = 3,

            /// <summary>Audio provided by a file.</summary>
            AudioPath   = 4,
        }

        internal const string TextContentClass = "com.google.ai.edge.litertlm.Content$Text";
        internal const string ImageFileContentClass = "com.google.ai.edge.litertlm.Content$ImageFile";
        internal const string AudioFileContentClass = "com.google.ai.edge.litertlm.Content$AudioFile";
        internal const string ImageBytesContentClass = "com.google.ai.edge.litertlm.Content$ImageBytes";
        internal const string AudioBytesContentClass = "com.google.ai.edge.litertlm.Content$AudioBytes";

        /// <summary>
        /// The type of the data contained in this object.
        /// </summary>
        public readonly ContentType Type;

        /// <summary>
        /// String content (<see cref="ContentType.Text"/>, <see cref="ContentType.ImagePath"/>, <see cref="ContentType.AudioPath"/>).
        /// </summary>
        public readonly string? StringContent;

        /// <summary>
        /// Binary content (<see cref="ContentType.ImageBytes"/>, <see cref="ContentType.AudioBytes"/>).
        /// </summary>
        public ReadOnlySpan<byte> BytesContent
        {
            get
            {
#pragma warning disable IDE0046 // Convert to conditional expression

                if (_csBytesContent is null && _jvmBytesContent is null)
                    return ReadOnlySpan<byte>.Empty;
#pragma warning restore IDE0046 // Convert to conditional expression


                return _csBytesContent is null
                    ? MemoryMarshal.Cast<sbyte, byte>(_jvmBytesContent)
                    : _csBytesContent;
            }
        }
        
        private readonly byte[]? _csBytesContent;
        private readonly sbyte[]? _jvmBytesContent;
        internal readonly AndroidJavaObject _native;
        internal bool Disposed { get; private set; }

        private Content(ContentType type, string? stringContent = null, byte[]? bytesContent = null)
        {
            Type = type;
            StringContent = stringContent;
            _csBytesContent = bytesContent;

            _native = Type switch
            {
                ContentType.Text => new AndroidJavaObject(TextContentClass, stringContent),
                ContentType.ImagePath => new AndroidJavaObject(ImageFileContentClass, stringContent),
                ContentType.AudioPath => new AndroidJavaObject(AudioFileContentClass, stringContent),

                ContentType.ImageBytes => new AndroidJavaObject(ImageBytesContentClass, bytesContent),
                ContentType.AudioBytes => new AndroidJavaObject(AudioBytesContentClass, bytesContent),
                _ => throw new NotImplementedException()
            };
        }

        /// <summary>
        /// Creates a new <see cref="Content"/> from an existing one.
        /// </summary>
        /// <remarks>
        /// This creates a shallow copy of <paramref name="other"/>. A new <see cref="AndroidJavaObject"/>
        /// which refers to the same native Kotlin object as <paramref name="other"/> is created, and
        /// the text and binary data of <paramref name="other"/> is also copied by reference. The new
        /// object takes on the same <see cref="Type"/> as <paramref name="other"/>.
        /// </remarks>
        public Content(Content other)
        {
            if (other.Disposed)
                throw new ObjectDisposedException(nameof(Content));

            _native = new AndroidJavaObject(other._native.GetRawObject());
            _csBytesContent = other._csBytesContent;
            _jvmBytesContent = other._jvmBytesContent;
            StringContent = other.StringContent;
            Type = other.Type;
        }

        internal Content(AndroidJavaObject native)
        {
            _native = native;

            try
            {
                IntPtr nativeObjectPtr = native.GetRawObject();
                using AndroidJavaClass textClass = new(TextContentClass);
                if (AndroidJNI.IsInstanceOf(nativeObjectPtr, textClass.GetRawClass()))
                {
                    Type = ContentType.Text;
                    StringContent = native.Get<string>("text")
                        ?? throw new NullReferenceException("Text content was null.");
                    
                    return;
                }

                using AndroidJavaClass imageBytesClass = new(ImageBytesContentClass);
                if (AndroidJNI.IsInstanceOf(nativeObjectPtr, imageBytesClass.GetRawClass()))
                {
                    Type = ContentType.ImageBytes;
                    _jvmBytesContent = native.Get<sbyte[]>("bytes")
                        ?? throw new NullReferenceException("Image content (sbyte[]) was null.");

                    return;
                }

                using AndroidJavaClass imageFileClass = new(ImageFileContentClass);
                if (AndroidJNI.IsInstanceOf(nativeObjectPtr, imageFileClass.GetRawClass()))
                {
                    Type = ContentType.ImagePath;
                    StringContent = native.Get<string>("absolutePath")
                        ?? throw new NullReferenceException("Image content (path) was null.");
                    
                    return;
                }

                using AndroidJavaClass audioBytesClass = new(AudioBytesContentClass);
                if (AndroidJNI.IsInstanceOf(nativeObjectPtr, audioBytesClass.GetRawClass()))
                {
                    Type = ContentType.AudioBytes;
                    _jvmBytesContent = native.Get<sbyte[]>("bytes")
                        ?? throw new NullReferenceException("Audio content (sbyte[]) was null.");

                    return;
                }

                using AndroidJavaClass audioFileClass = new(AudioFileContentClass);
                if (AndroidJNI.IsInstanceOf(nativeObjectPtr, audioFileClass.GetRawClass()))
                {
                    Type = ContentType.AudioPath;
                    StringContent = native.Get<string>("absolutePath")
                        ?? throw new NullReferenceException("Audio content (path) was null.");
                    
                    return;
                }

                using AndroidJavaObject nativeClass = native.Call<AndroidJavaObject>("getClass")
                    ?? throw new NullReferenceException("Could not get class of unknown content object.");

                throw new NotImplementedException($"Encountered unknown content type: {nativeClass.Call<string>("getName")}");
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
        }

        /// <inheritdoc/>
        public override string? ToString()
        {
            return Type is ContentType.Text or ContentType.ImagePath or ContentType.AudioPath
                ? StringContent : ((object?)_csBytesContent ?? _jvmBytesContent)?.ToString();
        }

        /// <summary>
        /// Creates a <see cref="Content"/> for text.
        /// </summary>
        public static Content Text(string content) => new(ContentType.Text, stringContent: content);

        /// <summary>
        /// Creates a <see cref="Content"/> for an image from bytes.
        /// </summary>
        public static Content ImageBytes(byte[] data) => new(ContentType.ImageBytes, bytesContent: data);

        /// <summary>
        /// Creates a <see cref="Content"/> for an image from a filepath.
        /// </summary>
        public static Content ImageFile(string path) => new(ContentType.ImagePath, stringContent: path);

        /// <summary>
        /// Creates a <see cref="Content"/> for audio from bytes.
        /// </summary>
        public static Content AudioBytes(byte[] data) => new(ContentType.AudioBytes, bytesContent: data);

        /// <summary>
        /// Creates a <see cref="Content"/> for audio from a filepath.
        /// </summary>
        public static Content AudioFile(string path) => new(ContentType.AudioPath, stringContent: path);

        public static implicit operator Content(string current) => Text(current);
        
        public static implicit operator string?(Content current) => current.StringContent;
        public static implicit operator ReadOnlySpan<byte>(Content current) => current.BytesContent;
    }
}
