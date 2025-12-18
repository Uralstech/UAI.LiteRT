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
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRT
{
    public record ManagedLiteRTContentArray : IDisposable
    {
        public readonly IEnumerable<LiteRTContent> Contents;
        public readonly bool HandleChildDispose;

        internal readonly AndroidJavaObject _native;
        private bool _disposed;

        public ManagedLiteRTContentArray(IEnumerable<LiteRTContent> contents, bool handleChildDispose = true)
        {
            Contents = contents;
            HandleChildDispose = handleChildDispose;
            _native = new AndroidJavaObject("java.util.ArrayList");

            foreach (LiteRTContent content in contents)
                _native.Call("add", content._native);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _native.Dispose();
            
            GC.SuppressFinalize(this);
            if (!HandleChildDispose)
                return;

            foreach (LiteRTContent content in Contents)
                content.Dispose();
        }

        public static implicit operator ManagedLiteRTContentArray(LiteRTContent[] current) => new(current);
        public static implicit operator ManagedLiteRTContentArray(List<LiteRTContent> current) => new(current);
    }
}
