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
namespace Uralstech.UAI.LiteRTLM
{
    /// <summary>
    /// Manages the lifecycle of a LiteRT-LM engine, providing an interface for interacting with the
    /// underlying native (C++) library.
    /// </summary>
    /// <remarks>
    /// This object manages a native wrapper for a <c>com.google.ai.edge.litertlm.Engine</c> object and must be disposed after usage
    /// to close the <c>Engine</c> object and to release the wrapper object.
    /// </remarks>
    public sealed class Engine : JavaObject
    {
        /// <summary>
        /// Backend for the LiteRT-LM engine.
        /// </summary>
        public enum Backend
        {
            /// <summary>Undefined value, equivalent to <see langword="null"/>.</summary>
            Undefined = -1,

            /// <summary>CPU LiteRT backend.</summary>
            CPU = 0,

            /// <summary>GPU LiteRT backend.</summary>
            GPU = 1,

            /// <summary>NPU LiteRT backend.</summary>
            NPU = 2
        }

        public enum LogSeverity
        {
            Verbose     = 0,
            Debug       = 1,
            Info        = 2,
            Warning     = 3,
            Error       = 4,
            Fatal       = 5,
            Infinity    = 1000,
        }

        private const string EngineWrapperClass = "com.uralstech.uai.litertlm.EngineWrapper";

        /// <summary>
        /// Returns <see langword="true"/> if the engine is initialized and ready for use; <see langword="false"/> otherwise.
        /// </summary>
        public bool IsInitialized => !IsDisposed ? Handle.Call<bool>("isInitialized") : throw new ObjectDisposedException(nameof(Engine));

        /// <inheritdoc/>
        public override AndroidJavaObject Handle { get; }

        private Engine(AndroidJavaObject wrapper)
        {
            Handle = wrapper;
        }

        /// <summary>
        /// Sets the minimum log severity for the native (C++) libraries. This affects global logging for all
        /// engine instances. If not set, it uses the native libraries' default.
        /// </summary>
        public static void SetNativeLogSeverity(LogSeverity severity)
        {
            using AndroidJavaClass wrapperClass = new(EngineWrapperClass);
            wrapperClass.CallStatic("setEngineLogSeverity", (int)severity);
        }

        /// <summary>
        /// Creates a new LiteRT LM engine.
        /// </summary>
        /// <remarks>
        /// The engine can take a long time to initialize. Check <see cref="IsInitialized"/> to see if it's done.
        /// </remarks>
        /// <param name="modelPath">The absolute file path to the LiteRT-LM model.</param>
        /// <param name="backend">The backend to use for the engine.</param>
        /// <param name="visionBackend">The backend to use for the vision executor. If <see cref="Backend.Undefined"/>, vision executor will not be initialized.</param>
        /// <param name="audioBackend">The backend to use for the audio executor. If <see cref="Backend.Undefined"/>, audio executor will not be initialized.</param>
        /// <param name="maxTokens">The maximum number of the sum of input and output tokens. It is equivalent to the size of the kv-cache. When 0, use the default value from the model or the engine.</param>
        /// <param name="useExternalCacheDir">Should cache files be placed in the external or internal cache dir appointed to the app by Android?</param>
        /// <returns>The uninitialized engine or <see langword="null"/> if the call failed.</returns>
        public static Engine? Create(string modelPath, Backend backend = Backend.CPU,
            Backend visionBackend = Backend.Undefined, Backend audioBackend = Backend.Undefined,
            int maxTokens = 0, bool useExternalCacheDir = true)
        {
            using AndroidJavaClass wrapperClass = new(EngineWrapperClass);
            AndroidJavaObject? wrapper = wrapperClass.CallStatic<AndroidJavaObject>("create",
                modelPath,
                (int)backend,
                (int)visionBackend,
                (int)audioBackend,
                maxTokens,
                useExternalCacheDir);

            if (wrapper is not null)
                return new Engine(wrapper);

            Debug.LogError($"{nameof(Engine)}: Could not create engine wrapper.");
            return null;
        }
        
        /// <summary>
        /// Creates a new LiteRT LM engine and waits for it to initialize.
        /// </summary>
        /// <remarks>
        /// Cancellation of this operation <b>does not</b> cancel the initialization of the engine, but cancels the awaiting of it.
        /// </remarks>
        /// <returns>The initialized engine or <see langword="null"/> if the call failed.</returns>
        /// <inheritdoc cref="Create"/>
        public static async Awaitable<Engine?> CreateAsync(string modelPath, Backend backend = Backend.CPU,
            Backend visionBackend = Backend.Undefined, Backend audioBackend = Backend.Undefined,
            int maxTokens = 0, bool useExternalCacheDir = true, CancellationToken token = default)
        {
            await Awaitable.MainThreadAsync();
            if (Create(modelPath, backend, visionBackend, audioBackend, maxTokens, useExternalCacheDir) is not Engine engine)
                return null;

            while (!engine.IsInitialized && !token.IsCancellationRequested)
                await Awaitable.NextFrameAsync(token);

            token.ThrowIfCancellationRequested();
            return engine;
        }

        /// <summary>
        /// Creates a new <see cref="Conversation"/> from the initialized engine.
        /// </summary>
        /// <param name="systemMessage">The optional system message to be used in the conversation.</param>
        /// <param name="samplerConfig">The optional configuration for the sampling process. If <see langword="null"/>, then uses the engine's default values.</param>
        /// <returns>The conversation or <see langword="null"/> if the call failed.</returns>
        public Conversation? CreateConversation(Message? systemMessage = null, SamplerConfig? samplerConfig = null)
        {
            ThrowIfDisposed();
            if (Handle.Call<AndroidJavaObject>("createConversation", systemMessage?.Handle, samplerConfig?.Handle) is AndroidJavaObject wrapper)
                return new Conversation(wrapper);

            Debug.LogError($"{nameof(Engine)}: Could not create conversation wrapper.");
            return null;
        }

        /// <summary>
        /// Creates a new <see cref="Session"/> from the initialized engine.
        /// </summary>
        /// <param name="samplerConfig">The optional configuration for the sampling process. If <see langword="null"/>, then uses the engine's default values.</param>
        /// <returns>The session or <see langword="null"/> if the call failed.</returns>
        public Session? CreateSession(SamplerConfig? samplerConfig = null)
        {
            ThrowIfDisposed();
            if (Handle.Call<AndroidJavaObject>("createSession", samplerConfig?.Handle) is AndroidJavaObject wrapper)
                return new Session(wrapper);

            Debug.LogError($"{nameof(Engine)}: Could not create session wrapper.");
            return null;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (IsDisposed)
                return;
            
            Handle.Call("close");
            base.Dispose();
        }
    }
}
