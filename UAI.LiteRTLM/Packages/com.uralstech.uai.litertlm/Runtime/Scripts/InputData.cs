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
    /// A sealed class representing the input data that can be provided to the LiteRT-LM.
    /// </summary>
    /// <remarks>
    /// This can store text or binary content, based on its <see cref="Type"/>.
    /// This object manages a native <c>com.google.ai.edge.litertlm.InputData</c> object and must be disposed after usage
    /// OR must be managed by a <see cref="InputDataArray"/>/<see cref="JavaArrayList{T}"/> to handle its disposal.
    /// </remarks>
    public sealed class InputData : JavaObject
    {
        /// <summary>
        /// The data type of the <see cref="InputData"/>.
        /// </summary>
        public enum DataType
        {
            /// <summary>Represents text input.</summary>
            Text    = 0,

            /// <summary>Represents image input.</summary>
            /// <remarks>Supported formats: PNG and JPG.</remarks>
            Image   = 1,

            /// <summary>Represents audio input.</summary>
            /// <remarks>Supported formats: WAV.</remarks>
            Audio   = 2,
        }

        private const string TextContentClass = "com.google.ai.edge.litertlm.InputData$Text";
        private const string ImageContentClass = "com.google.ai.edge.litertlm.InputData$Image";
        private const string AudioContentClass = "com.google.ai.edge.litertlm.InputData$Audio";

        /// <summary>
        /// The type of the data contained in this object.
        /// </summary>
        public readonly DataType Type;

        /// <summary>
        /// String content (<see cref="DataType.Text"/>).
        /// </summary>
        public readonly string? StringContent;

        /// <summary>
        /// Binary content (<see cref="DataType.Image"/>, <see cref="DataType.Audio"/>).
        /// </summary>
        public readonly byte[]? BytesContent;

        /// <inheritdoc/>
        public override AndroidJavaObject Handle { get; }

        private InputData(DataType type, string? stringContent = null, byte[]? bytesContent = null)
        {
            Type = type;
            StringContent = stringContent;
            BytesContent = bytesContent;

            Handle = type switch
            {
                DataType.Text => new AndroidJavaObject(TextContentClass, stringContent),
                DataType.Image => new AndroidJavaObject(ImageContentClass, bytesContent),
                DataType.Audio => new AndroidJavaObject(AudioContentClass, bytesContent),
                _ => throw new NotImplementedException()
            };
        }

        /// <inheritdoc/>
        public override string? ToString() => Type is DataType.Text ? StringContent : BytesContent?.ToString();

        /// <summary>
        /// Creates <see cref="InputData"/> representing text.
        /// </summary>
        public static InputData Text(string content) => new(DataType.Text, stringContent: content);

        /// <summary>
        /// Creates <see cref="InputData"/> representing an image.
        /// </summary>
        public static InputData Image(byte[] data) => new(DataType.Image, bytesContent: data);

        /// <summary>
        /// Creates <see cref="InputData"/> representing audio.
        /// </summary>
        public static InputData Audio(byte[] data) => new(DataType.Audio, bytesContent: data);

        public static implicit operator InputData(string current) => Text(current);
    }
}
