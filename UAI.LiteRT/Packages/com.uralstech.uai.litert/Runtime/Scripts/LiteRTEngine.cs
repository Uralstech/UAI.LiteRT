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
using System.Threading;
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRT
{
    public class LiteRTEngine : IDisposable
    {
        public enum Backend
        {
            Undefined = -1,
            CPU = 0,
            GPU = 1,
            NPU = 2
        }

        public bool IsInitialized => !_disposed ? _wrapper.Call<bool>("isInitialized") : throw new ObjectDisposedException(nameof(LiteRTEngine));
        
        private readonly AndroidJavaObject _wrapper;
        private bool _disposed;

        private LiteRTEngine(AndroidJavaObject wrapper)
        {
            _wrapper = wrapper;
        }

        public static LiteRTEngine? Create(string modelPath, Backend backend = Backend.CPU,
            Backend visionBackend = Backend.Undefined, Backend audioBackend = Backend.Undefined,
            int maxTokens = 1024, bool useExternalCacheDir = true)
        {
            using AndroidJavaClass wrapperClass = new("com.uralstech.uai.litert.EngineWrapper");
            AndroidJavaObject? wrapper = wrapperClass.CallStatic<AndroidJavaObject>("create",
                modelPath,
                (int)backend,
                (int)visionBackend,
                (int)audioBackend,
                maxTokens,
                useExternalCacheDir);

            if (wrapper is not null)
                return new LiteRTEngine(wrapper);

            Debug.LogError($"{nameof(LiteRTEngine)}: Could not create engine wrapper.");
            return null;
        }
        
        public static async Awaitable<LiteRTEngine?> CreateAsync(string modelPath, Backend backend = Backend.CPU,
            Backend visionBackend = Backend.Undefined, Backend audioBackend = Backend.Undefined,
            int maxTokens = 1024, bool useExternalCacheDir = true, CancellationToken token = default)
        {
            await Awaitable.MainThreadAsync();
            if (Create(modelPath, backend, visionBackend, audioBackend, maxTokens, useExternalCacheDir) is not LiteRTEngine engine)
                return null;

            while (!engine.IsInitialized && !token.IsCancellationRequested)
                await Awaitable.NextFrameAsync(token);

            token.ThrowIfCancellationRequested();
            return engine;
        }

        public LiteRTConversation? CreateConversation()
        {
            ThrowIfDisposed();

            if (_wrapper.Call<AndroidJavaObject>("createConversation") is AndroidJavaObject wrapper)
                return new LiteRTConversation(wrapper);

            Debug.LogError($"{nameof(LiteRTEngine)}: Could not create conversation wrapper.");
            return null;
        }

        public LiteRTConversation? CreateConversation(LiteRTMessage? systemMessage = null, LiteRTSamplerConfig? samplerConfig = null)
        {
            ThrowIfDisposed();

            if (_wrapper.Call<AndroidJavaObject>("createConversation", systemMessage?._native, samplerConfig?._native) is AndroidJavaObject wrapper)
                return new LiteRTConversation(wrapper);

            Debug.LogError($"{nameof(LiteRTEngine)}: Could not create conversation wrapper.");
            return null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LiteRTEngine));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _wrapper.Call("close");
            _wrapper.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
