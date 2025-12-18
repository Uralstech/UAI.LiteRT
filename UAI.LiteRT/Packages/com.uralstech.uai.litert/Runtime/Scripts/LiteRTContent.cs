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
namespace Uralstech.UAI.LiteRT
{
    public record LiteRTContent : IDisposable
    {
        public enum ContentType
        {
            Text        = 0,
            ImageBytes  = 1,
            ImagePath   = 2,
            AudioBytes  = 3,
            AudioPath   = 4,
        }

        public readonly ContentType Type;
        public readonly string? StringContent;
        public readonly byte[]? BytesContent;

        internal readonly AndroidJavaObject _native;
        private bool _disposed;

        private LiteRTContent(ContentType type, string? stringContent = null, byte[]? bytesContent = null)
        {
            Type = type;
            StringContent = stringContent;
            BytesContent = bytesContent;

            _native = Type switch
            {
                ContentType.Text => new AndroidJavaObject("com.google.ai.edge.litertlm.Content$Text", stringContent),
                ContentType.ImagePath => new AndroidJavaObject("com.google.ai.edge.litertlm.Content$ImageFile", stringContent),
                ContentType.AudioPath => new AndroidJavaObject("com.google.ai.edge.litertlm.Content$AudioFile", stringContent),

                ContentType.ImageBytes => new AndroidJavaObject("com.google.ai.edge.litertlm.Content$ImageBytes", bytesContent),
                ContentType.AudioBytes => new AndroidJavaObject("com.google.ai.edge.litertlm.Content$AudioBytes", bytesContent),
                _ => throw new NotImplementedException()
            };
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _native.Dispose();

            GC.SuppressFinalize(this);
        }

        public static LiteRTContent Text(string content) => new(ContentType.Text, stringContent: content);

        public static LiteRTContent ImageBytes(byte[] data) => new(ContentType.ImageBytes, bytesContent: data);
        public static LiteRTContent ImageFile(string path) => new(ContentType.ImagePath, stringContent: path);

        public static LiteRTContent AudioBytes(byte[] data) => new(ContentType.AudioBytes, bytesContent: data);
        public static LiteRTContent AudioFile(string path) => new(ContentType.AudioPath, stringContent: path);

        public static implicit operator LiteRTContent(string current) => Text(current);
    }
}
