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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRT
{
    public class LiteRTConversation : AndroidJavaProxy, IDisposable
    {
        public event Action? OnAsyncInferenceDone;
        public event Action<string?>? OnAsyncInferenceErred;
        public event Action<string>? OnAsyncInferenceMessagePart;

        private readonly AndroidJavaObject _wrapper;
        private bool _disposed;

        internal LiteRTConversation(AndroidJavaObject wrapper) : base("com.uralstech.uai.litert.ConversationWrapper$AsyncInferenceCallbacks")
        {
            _wrapper = wrapper;
        }

        public override IntPtr Invoke(string methodName, IntPtr javaArgs)
        {
            switch (methodName)
            {
                case "onDone":
                    OnAsyncInferenceDone?.Invoke();
                    return IntPtr.Zero;

                case "onError":
                    IntPtr ptr = AndroidJNI.GetObjectArrayElement(javaArgs, 0);
                    string? value = AndroidJNI.GetStringUTFChars(ptr);

                    AndroidJNI.DeleteLocalRef(ptr);

                    Debug.LogError($"{nameof(LiteRTConversation)}: Could not process async inference due to error: {value}");
                    OnAsyncInferenceErred?.Invoke(value);
                    return IntPtr.Zero;

                case "onMessage":
                    ptr = AndroidJNI.GetObjectArrayElement(javaArgs, 0);
                    value = AndroidJNI.GetStringUTFChars(ptr);

                    AndroidJNI.DeleteLocalRef(ptr);
                    OnAsyncInferenceMessagePart?.Invoke(value);
                    return IntPtr.Zero;
            }

            return base.Invoke(methodName, javaArgs);
        }

        public string? SendMessage(LiteRTMessage message)
        {
            ThrowIfDisposed();

            if (_wrapper.Call<string>("sendMessage", message._native) is string result)
                return result;

            Debug.LogError($"{nameof(LiteRTConversation)}: Could not send message.");
            return null;
        }

        public bool SendMessageAsync(LiteRTMessage message)
        {
            ThrowIfDisposed();

            if (_wrapper.Call<bool>("sendMessageAsync", message._native, this))
                return true;

            Debug.LogError($"{nameof(LiteRTConversation)}: Could not send message.");
            return false;
        }

        public async IAsyncEnumerable<string> StreamSendMessageAsync(LiteRTMessage message, [EnumeratorCancellation] CancellationToken token = default)
        {
            TaskCompletionSource<bool> statusTcs = new();
            ConcurrentQueue<string> partsQueue = new ();
            using SemaphoreSlim waitHandle = new(0, 1);

            void OnDone()
            {
                statusTcs.SetResult(true);
                waitHandle.Release();
            }

            void OnErred(string? _)
            {
                statusTcs.SetResult(false);
                waitHandle.Release();
            }

            void OnPartReceived(string part)
            {
                partsQueue.Enqueue(part);
                waitHandle.Release();
            }

            OnAsyncInferenceDone += OnDone;
            OnAsyncInferenceErred += OnErred;
            OnAsyncInferenceMessagePart += OnPartReceived;

            try
            {
                if (!SendMessageAsync(message))
                    yield break;

                while (!token.IsCancellationRequested)
                {
                    await waitHandle.WaitAsync(token);
                    if (token.IsCancellationRequested || statusTcs.Task.IsCompleted)
                        break;

                    while (partsQueue.TryDequeue(out string part))
                        yield return part;
                }
            }
            finally
            {
                OnAsyncInferenceDone -= OnDone;
                OnAsyncInferenceErred -= OnErred;
                OnAsyncInferenceMessagePart -= OnPartReceived;
            }
        }

        public bool CancelProcess()
        {
            ThrowIfDisposed();
            if (_wrapper.Call<bool>("cancelProcess"))
                return true;
            
            Debug.LogError($"{nameof(LiteRTConversation)}: Could not cancel process.");
            return false;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(LiteRTConversation));
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
