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
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    /// <summary>
    /// Represents a conversation with the LiteRT-LM model.
    /// </summary>
    /// <remarks>
    /// This object manages a native wrapper for a <c>com.google.ai.edge.litertlm.Conversation</c> object and must be disposed after usage
    /// to close the <c>Conversation</c> object and to release the wrapper object.
    /// </remarks>
    public class Conversation : IDisposable
    {
        private readonly AndroidJavaObject _wrapper;
        private bool _disposed;

        internal Conversation(AndroidJavaObject wrapper)
        {
            _wrapper = wrapper;
        }

        /// <summary>
        /// Sends a message to the model and returns the response. This is a synchronous call.
        /// </summary>
        /// <param name="message">The message to send to the model.</param>
        /// <returns>The model's response message or <see langword="null"/> if the call failed</returns>
        public Message? SendMessage(Message message)
        {
            ThrowIfDisposed();

            if (_wrapper.Call<AndroidJavaObject>("sendMessage", message._native) is AndroidJavaObject result)
                return new Message(result);

            Debug.LogError($"{nameof(Conversation)}: Could not send message.");
            return null;
        }

        /// <summary>
        /// Sends a message to the model and returns the response aysnc with callbacks.
        /// </summary>
        /// <param name="message">The message to send to the model.</param>
        /// <param name="callbacks">The callback to receive the streaming responses.</param>
        /// <returns>Returns <see langword="true"/> if the call succeeded; <see langword="false"/> otherwise.</returns>
        public bool SendMessageAsync(Message message, MessageCallbacks callbacks)
        {
            ThrowIfDisposed();

            if (_wrapper.Call<bool>("sendMessageAsync", message._native, callbacks))
                return true;

            Debug.LogError($"{nameof(Conversation)}: Could not send message.");
            return false;
        }
        
        /// <summary>
        /// Sends a message to the model and returns the partial response messages as an <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="CancelProcess"/> is automatically called if this method is cancelled using <paramref name="token"/>.
        /// </remarks>
        /// <param name="message">The message to send to the model.</param>
        /// <param name="callbacks">Callback object to use in processing. Creates new if not provided.</param>
        /// <returns>Returns the streamed <see cref="Message"/> objects. Their disposal is the responsibility of the consumer.</returns>
        public async IAsyncEnumerable<Message> StreamSendMessageAsync(Message message, MessageCallbacks? callbacks = null,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            ThrowIfDisposed();
            Channel<Message> channel = Channel.CreateUnbounded<Message>();

            void OnDone() => channel.Writer.TryComplete();
            void OnError(AndroidJavaObject _, string? error) => channel.Writer.TryComplete(new Exception(error ?? "Unknown error during inference."));
            void OnMessage(Message message)
            {
                Message copy = new(message);
                channel.Writer.TryWrite(copy);
            }

            callbacks ??= new MessageCallbacks();
            callbacks.OnDone += OnDone;
            callbacks.OnError += OnError;
            callbacks.OnMessage += OnMessage;

            try
            {
                if (!SendMessageAsync(message, callbacks))
                {
                    channel.Writer.TryComplete();
                    yield break;
                }

                using CancellationTokenRegistration _ = token.Register(static (convo) => ((Conversation)convo).CancelProcess(), this);
                await foreach (Message part in channel.Reader.ReadAllAsync(token))
                    yield return part;
            }
            finally
            {
                callbacks.OnDone -= OnDone;
                callbacks.OnError -= OnError;
                callbacks.OnMessage -= OnMessage;
            }    
        }

        /// <summary>
        /// Cancels any ongoing inference process.
        /// </summary>
        public bool CancelProcess()
        {
            ThrowIfDisposed();
            if (_wrapper.Call<bool>("cancelProcess"))
                return true;
            
            Debug.LogError($"{nameof(Conversation)}: Could not cancel process.");
            return false;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Conversation));
        }

        /// <inheritdoc/>
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
