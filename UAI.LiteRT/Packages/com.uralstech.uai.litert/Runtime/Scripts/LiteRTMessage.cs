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
    public record LiteRTMessage : IDisposable
    {
        public readonly ManagedLiteRTContentArray? Contents;
        public readonly bool HandleContentsDispose;
        public readonly string? TextMessage;

        internal readonly AndroidJavaObject _native;
        private bool _disposed;

        private LiteRTMessage(ManagedLiteRTContentArray? contents, string? textMessage, bool handleContentsDispose)
        {
            Contents = contents;
            TextMessage = textMessage;
            HandleContentsDispose = handleContentsDispose;

            using AndroidJavaClass nativeWrapper = new("com.uralstech.uai.litert.ConversationWrapper");
            _native = nativeWrapper.CallStatic<AndroidJavaObject>("messageOf", (object?)contents ?? textMessage);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            
            _disposed = true;
            _native.Dispose();
            
            GC.SuppressFinalize(this);
            if (!HandleContentsDispose)
                return;

            Contents!.Dispose();
        }

        public static LiteRTMessage Of(ManagedLiteRTContentArray contents, bool handleContentsDispose = true) =>
            new(contents, textMessage: null, handleContentsDispose);
        
        public static LiteRTMessage Of(string textMessage) =>
            new(contents: null, textMessage, handleContentsDispose: false);

        public static implicit operator LiteRTMessage(string current) => Of(current);
    }
}
