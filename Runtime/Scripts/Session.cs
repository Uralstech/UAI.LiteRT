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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    /// <summary>
    /// Manages the lifecycle of a LiteRT-LM session, providing an interface for interacting with the native library.
    /// </summary>
    /// <remarks>
    /// This object manages a native wrapper for a <c>com.google.ai.edge.litertlm.Session</c> object and must be disposed after usage
    /// to close the <c>Session</c> object and to release the wrapper object.
    /// </remarks>
    public sealed class Session : JavaObject
    {
        /// <summary>
        /// Returns <see langword="true"/> if the session is alive and ready to be used; <see langword="false"/> otherwise.
        /// </summary>
        public bool IsAlive => !IsDisposed ? Handle.Call<bool>("isAlive") : throw new ObjectDisposedException(nameof(Session));

        /// <inheritdoc/>
        public override AndroidJavaObject Handle { get; }

        internal Session(AndroidJavaObject native)
        {
            Handle = native;
        }

        #region RunPrefill Variants
        /// <summary>
        /// Adds the <paramref name="input"/> and starts the prefilling process.
        /// </summary>
        /// <remarks>
        /// User can break down their <see cref="InputData"/> into multiple chunks and call this function multiple times.
        /// This is a blocking call and the function will return when the prefill process is done.
        /// </remarks>
        /// <param name="input">An array of <see cref="InputData"/> to be processed by the model.</param>
        /// <returns><see langword="true"/> if successful; <see langword="false"/> otherwise.</returns>
        public bool RunPrefill(InputDataArray input)
        {
            ThrowIfDisposed();
            if (Handle.Call<bool>("runPrefill", input.Handle))
                return true;
            
            Debug.LogError($"{nameof(Session)}: Could not prefill input.");
            return false;
        }

        /// <remarks>
        /// User can break down their <see cref="InputData"/> into multiple chunks and call this function multiple times.
        /// Cancellation of this operation <b>does not</b> cancel the prefill task, but cancels the awaiting of it.
        /// </remarks>
        /// <inheritdoc cref="RunPrefill(InputDataArray)"/>
        public async Awaitable<bool> RunPrefillAsync(InputDataArray input, CancellationToken token = default)
        {
            ThrowIfDisposed();
            TaskCompletionSource<bool> tcs = new();

            using CancellationTokenRegistration _ = token.Register(static tcs => ((TaskCompletionSource<bool>)tcs).TrySetCanceled(), tcs);
            if (Handle.Call<bool>("runPrefillAsync", input.Handle, new AndroidJavaRunnable(() => tcs.TrySetResult(true))))
                return await tcs.Task;

            Debug.LogError($"{nameof(Session)}: Could not prefill input asynchronously.");
            return false;
        }
        #endregion

        #region RunDecode Variants
        /// <summary>
        /// Runs the decode step for the model to predict the response based on the input data added by
        /// <see cref="RunPrefill(InputDataArray)"/> or <see cref="RunPrefillAsync(InputDataArray, CancellationToken)"/>.
        /// </summary>
        /// <remarks>
        /// This is a blocking call and the function will return when the decoding process is done.
        /// </remarks>
        /// <returns>The generated content as a <see cref="string"/> if successful, <see langword="null"/> otherwise.</returns>
        public string? RunDecode()
        {
            ThrowIfDisposed();
            if (Handle.Call<string>("runDecode") is string result)
                return result;

            Debug.LogError($"{nameof(Session)}: Could not decode output.");
            return null;
        }

        /// <remarks>
        /// Cancellation of this operation <b>does not</b> cancel the decode task, but cancels the awaiting of it.
        /// </remarks>
        /// <param name="callback">Optional cached callback proxy.</param>
        /// <inheritdoc cref="RunDecode"/>
        public async Awaitable<string?> RunDecodeAsync(AsyncDecodeCallback? callback = null, CancellationToken token = default)
        {
            ThrowIfDisposed();
            callback ??= new AsyncDecodeCallback();
            TaskCompletionSource<string> tcs = new();

            void OnDone(string result) => tcs.TrySetResult(result);
            callback.OnDone += OnDone;

            try
            {
                using CancellationTokenRegistration _ = token.Register(static tcs => ((TaskCompletionSource<string>)tcs).TrySetCanceled(), tcs);
                if (Handle.Call<bool>("runDecodeAsync", callback))
                    return await tcs.Task;

                Debug.LogError($"{nameof(Session)}: Could not decode output asynchronously.");
                return null;
            }
            finally
            {
                callback.OnDone -= OnDone;
            }
        }
#endregion

        #region GenerateContent Variants
        /// <summary>
        /// Generates content from the provided <paramref name="input"/> and any previous input data added by
        /// <see cref="RunPrefill(InputDataArray)"/> or <see cref="RunPrefillAsync(InputDataArray, CancellationToken)"/>.
        /// </summary>
        /// <remarks>
        /// This handles both the prefilling and decoding steps.
        /// </remarks>
        /// <param name="input">An array of <see cref="InputData"/> to be processed by the model. If the user wants to run the decode loop only, they can pass an empty array.</param>
        /// <returns>The generated content as a <see cref="string"/> if successful; <see langword="null"/> otherwise.</returns>
        public string? GenerateContent(InputDataArray input)
        {
            ThrowIfDisposed();
            if (Handle.Call<string>("generateContent", input.Handle) is string result)
                return result;

            Debug.LogError($"{nameof(Session)}: Could not generate content.");
            return null;
        }

        /// <param name="callbacks">The callback to receive the streaming responses.</param>
        /// <returns><see langword="true"/> if the request was executed successfuly; <see langword="false"/> otherwise.</returns>
        /// <inheritdoc cref="GenerateContent(InputDataArray)"/>
        public bool GenerateContentStream(InputDataArray input, ResponseCallbacks callbacks)
        {
            ThrowIfDisposed();
            if (Handle.Call<bool>("generateContentStream", input.Handle, callbacks))
                return true;
            
            Debug.LogError($"{nameof(Session)}: Could not stream generate content.");
            return false;
        }

        /// <remarks>
        /// This handles both the prefilling and decoding steps.
        /// <see cref="CancelProcess"/> is automatically called if this method is cancelled using <paramref name="token"/>.
        /// </remarks>
        /// <param name="callbacks">Optional cached callback to receive the streaming responses.</param>
        /// <returns>The streamed response chunks.</returns>
        /// <inheritdoc cref="GenerateContent(InputDataArray)"/>
        public async IAsyncEnumerable<string> GenerateContentStreamAsync(InputDataArray input, ResponseCallbacks? callbacks = null, [EnumeratorCancellation] CancellationToken token = default)
        {
            ThrowIfDisposed();
            Channel<string> channel = Channel.CreateUnbounded<string>();

            void OnDone() => channel.Writer.TryComplete();
            void OnError(AndroidJavaObject _, string? error) => channel.Writer.TryComplete(new Exception(error ?? "Unknown error during inference."));
            void OnNext(string message) => channel.Writer.TryWrite(message);

            callbacks ??= new ResponseCallbacks();
            callbacks.OnDone += OnDone;
            callbacks.OnError += OnError;
            callbacks.OnNext += OnNext;

            try
            {
                if (!GenerateContentStream(input, callbacks))
                {
                    channel.Writer.TryComplete();
                    yield break;
                }

                using CancellationTokenRegistration _ = token.Register(static (session) => ((Session)session).CancelProcess(), this);
                await foreach (string part in channel.Reader.ReadAllAsync(token))
                    yield return part;
            }
            finally
            {
                callbacks.OnDone -= OnDone;
                callbacks.OnError -= OnError;
                callbacks.OnNext -= OnNext;
            }
        }
        #endregion

        /// <summary>
        /// Cancels any ongoing inference process.
        /// </summary>
        public bool CancelProcess()
        {
            ThrowIfDisposed();
            if (Handle.Call<bool>("cancelProcess"))
                return true;
            
            Debug.LogError($"{nameof(Session)}: Could not cancel process.");
            return false;
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
