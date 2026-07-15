// Copyright 2026 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
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

using AOT;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Uralstech.UAI.LiteRTLM.Native;

#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    internal static class StreamCallbackHandler
    {
        private static int s_idCounter;
        private static readonly ConcurrentDictionary<IntPtr, StreamCallback> s_callbacks = new();

        private static readonly NativeAPI.StreamCallback s_globalCallbackListenerInst =
            GlobalStreamCallbackListener;
        
        public static readonly IntPtr GlobalStreamCallbackListenerPtr =
            UnsafeUtils.MarshalDelegate(s_globalCallbackListenerInst);

        public static IntPtr Register(StreamCallback callback)
        {
            IntPtr key = (IntPtr)Interlocked.Increment(ref s_idCounter);
            s_callbacks[key] = callback;
            return key;
        }

        public static void Deregister(IntPtr key) =>
            s_callbacks.TryRemove(key, out _);

        [MonoPInvokeCallback(typeof(NativeAPI.StreamCallback))]
        private static void GlobalStreamCallbackListener(IntPtr key, IntPtr chunk)
        {
            StreamChunk? managedChunk = null;
            bool isFinal = false;
            
            try
            {
                managedChunk = new StreamChunk(chunk);
                isFinal = managedChunk.IsFinal();
                
                if (s_callbacks.TryGetValue(key, out StreamCallback? callback))
                    callback?.Invoke(managedChunk);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                if (isFinal)
                    s_callbacks.TryRemove(key, out _);
                
                managedChunk?.Invalidate();
            }
        }
    }

    internal static class Extensions
    {
        public static IntPtr[] GetPtrs<T>(this ReadOnlySpan<T> handles)
            where T : LiteRTLMNativeHandle
        {
            int length = handles.Length;
            IntPtr[] handlePtrs = new IntPtr[length];

            for (int i = 0; i < length; i++)
                handlePtrs[i] = handles[i];
            
            return handlePtrs;
        }
    }

    public static class LiteRTLMNativeLogging
    {
        /// <summary>Sets the minimum log level for the LiteRT LM library.</summary>
        public static void SetMinLogLevel(LogSeverity level) =>
            NativeAPI.litert_lm_set_min_log_level(level);
    }
    
    /// <summary>Callback for streaming responses.</summary>
    /// <param name="chunk">The stream chunk object. It's only valid for the duration of the call.</param>
    public delegate void StreamCallback(StreamChunk chunk);
    
    public abstract class LiteRTLMNativeHandle : IDisposable
    {
        protected IntPtr Native { get; init; }
        private int _disposed = 0;
        
        protected abstract void ReleaseUnmanagedResources();
        
        public static implicit operator IntPtr(LiteRTLMNativeHandle? handle) => handle?.Native ?? IntPtr.Zero;
        
        /// <summary>Destroys the native LiteRT LM object.</summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
            
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~LiteRTLMNativeHandle()
        {
            try
            {
                Debug.LogWarning($"{GetType().Name}: Object not disposed using IDisposable interface.");
                ReleaseUnmanagedResources();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        protected void ThrowIfDisposed()
        {
            if (Volatile.Read(ref _disposed) == 1)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
    
    public sealed class SamplerParams : LiteRTLMNativeHandle
    {
        /// <summary>
        /// Creates a managed wrapper around LiteRT LM sampler parameters for a specific sampler type.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose()"/>.
        /// </summary>
        /// <param name="samplerType">The sampler type to use.</param>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public SamplerParams(SamplerType samplerType)
        {
            Native = NativeAPI.SamplerParams.litert_lm_sampler_params_create(samplerType);
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native sampler parameters.");
        }

        /// <summary>Sets the top-k value.</summary>
        public void SetTopK(int topK)
        {
            ThrowIfDisposed();
            NativeAPI.SamplerParams.litert_lm_sampler_params_set_top_k(Native, topK);
        }

        /// <summary>Sets the top-p value.</summary>
        public void SetTopP(float topP)
        {
            ThrowIfDisposed();
            NativeAPI.SamplerParams.litert_lm_sampler_params_set_top_p(Native, topP);
        }

        /// <summary>Sets the temperature.</summary>
        public void SetTemperature(float temperature)
        {
            ThrowIfDisposed();
            NativeAPI.SamplerParams.litert_lm_sampler_params_set_temperature(Native, temperature);
        }

        /// <summary>Sets the seed.</summary>
        public void SetSeed(int seed)
        {
            ThrowIfDisposed();
            NativeAPI.SamplerParams.litert_lm_sampler_params_set_seed(Native, seed);
        }

        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.SamplerParams.litert_lm_sampler_params_delete(Native);
        }
    }

    public sealed class SessionConfig : LiteRTLMNativeHandle
    {
        /// <summary>
        /// Creates a managed wrapper around a LiteRT LM session configuration.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose()"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public SessionConfig()
        {
            Native = NativeAPI.SessionConfig.litert_lm_session_config_create();
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native session config.");
        }

        /// <summary>Sets the maximum number of output tokens per decode step for this session.</summary>
        /// <param name="maxOutputTokens">The maximum number of output tokens.</param>
        public void SetMaxOutputTokens(int maxOutputTokens)
        {
            ThrowIfDisposed();
            NativeAPI.SessionConfig.litert_lm_session_config_set_max_output_tokens(Native, maxOutputTokens);
        }
        
        /// <summary>Sets whether to apply the prompt template for this session.</summary>
        /// <param name="applyPromptTemplate">Whether to apply the prompt template.</param>
        public void SetApplyPromptTemplate(bool applyPromptTemplate)
        {
            ThrowIfDisposed();
            NativeAPI.SessionConfig.litert_lm_session_config_set_apply_prompt_template(Native, applyPromptTemplate);
        }
        
        /// <summary>Sets the sampler parameters for this session configuration.</summary>
        /// <param name="samplerParams">The sampler parameters to use.</param>
        public void SetSamplerParams(SamplerParams samplerParams)
        {
            ThrowIfDisposed();
            NativeAPI.SessionConfig.litert_lm_session_config_set_sampler_params(Native, samplerParams);
        }
        
        /// <summary>Sets the path to the LoRA weights file.</summary>
        /// <param name="loraPath">The path to the text LoRA weights file.</param>
        /// <returns>0 on success, non-zero on failure.</returns>
        public int SetLoraPath(string loraPath)
        {
            ThrowIfDisposed();
            return NativeAPI.SessionConfig.litert_lm_session_config_set_lora_path(Native, loraPath);
        }
        
        /// <summary>Sets the path to the audio LoRA weights file.</summary>
        /// <param name="audioLoraPath">The path to the audio LoRA weights file.</param>
        /// <returns>0 on success, non-zero on failure.</returns>
        public int SetAudioLoraPath(string audioLoraPath)
        {
            ThrowIfDisposed();
            return NativeAPI.SessionConfig.litert_lm_session_config_set_audio_lora_path(Native, audioLoraPath);
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.SessionConfig.litert_lm_session_config_delete(Native);
        }
    }

    public sealed class ConversationConfig : LiteRTLMNativeHandle
    {
        /// <summary>
        /// Creates a managed wrapper around a LiteRT LM conversation configuration.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose()"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public ConversationConfig()
        {
            Native = NativeAPI.ConversationConfig.litert_lm_conversation_config_create();
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native conversation config.");
        }

        /// <summary>Sets the session config for this conversation configuration.</summary>
        /// <param name="sessionConfig">The session config to use.</param>
        public void SetSessionConfig(SessionConfig sessionConfig)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationConfig.litert_lm_conversation_config_set_session_config(Native, sessionConfig);
        }

        /// <summary>Sets the system message for this conversation configuration.</summary>
        /// <param name="systemMessageJson">The system message in JSON format.</param>
        public void SetSystemMessage(string systemMessageJson)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationConfig.litert_lm_conversation_config_set_system_message(Native, systemMessageJson);
        }

        /// <summary>Sets the tools for this conversation configuration.</summary>
        /// <param name="toolsJson">The tools description in JSON array format.</param>
        public void SetTools(string toolsJson)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationConfig.litert_lm_conversation_config_set_tools(Native, toolsJson);
        }
        
        /// <summary>Sets the initial messages for this conversation configuration.</summary>
        /// <param name="messagesJson">The initial messages in JSON array format.</param>
        public void SetMessages(string messagesJson)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationConfig.litert_lm_conversation_config_set_messages(Native, messagesJson);
        }
        
        /// <summary>Sets the extra context for the conversation preface.</summary>
        /// <param name="extraContextJson">A JSON string representing the extra context object.</param>
        public void SetExtraContext(string extraContextJson)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationConfig.litert_lm_conversation_config_set_extra_context(Native, extraContextJson);
        }
        
        /// <summary>Sets whether to enable constrained decoding for this conversation configuration.</summary>
        /// <param name="enableConstrainedDecoding">Whether to enable constrained decoding.</param>
        public void SetEnableConstrainedDecoding(bool enableConstrainedDecoding)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationConfig.litert_lm_conversation_config_set_enable_constrained_decoding(Native, enableConstrainedDecoding);
        }
        
        /// <summary>Sets whether to filter channel content from the KV cache.</summary>
        /// <param name="filterChannelContentFromKvCache">Whether to filter channel content.</param>
        public void SetFilterChannelContentFromKvCache(bool filterChannelContentFromKvCache)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationConfig.litert_lm_conversation_config_set_filter_channel_content_from_kv_cache(Native, filterChannelContentFromKvCache);
        }
        
        /// <summary>Sets whether to stream tool call tokens.</summary>
        /// <param name="streamToolCalls">Whether to stream tool call tokens.</param>
        /// <param name="channelName">The channel name to use for tool call tokens.</param>
        public void SetStreamToolCalls(bool streamToolCalls, string channelName)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationConfig.litert_lm_conversation_config_set_stream_tool_calls(Native, streamToolCalls, channelName);
        }

        /// <summary>Sets the thinking config for this conversation config.</summary>
        /// <param name="thinkingConfig">
        /// The thinking config to set. If <see langword="null"/>,
        /// clears any previously set thinking config.
        /// </param>
        public void SetThinkingConfig(ThinkingConfig? thinkingConfig)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationConfig.litert_lm_conversation_config_set_thinking_config(Native, thinkingConfig);
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.ConversationConfig.litert_lm_conversation_config_delete(Native);
        }
    }

    public sealed class ThinkingConfig : LiteRTLMNativeHandle
    {
        /// <summary>
        /// Creates a managed wrapper around a LiteRT LM thinking configuration.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose()"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public ThinkingConfig()
        {
            Native = NativeAPI.ThinkingConfig.litert_lm_thinking_config_create();
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native thinking config.");
        }

        /// <summary>Sets whether thinking/reasoning generation is enabled.</summary>
        /// <param name="enableThinking">Whether thinking is enabled.</param>
        public void SetEnableThinking(bool enableThinking)
        {
            ThrowIfDisposed();
            NativeAPI.ThinkingConfig.litert_lm_thinking_config_set_enable_thinking(Native, enableThinking);
        }

        /// <summary>Sets the thinking token budget.</summary>
        /// <param name="thinkingTokenBudget">
        /// Budget for token-by-token reasoning generation (-1 for infinite).
        /// </param>
        public void SetThinkingTokenBudget(int thinkingTokenBudget)
        {
            ThrowIfDisposed();
            NativeAPI.ThinkingConfig.litert_lm_thinking_config_set_thinking_token_budget(Native, thinkingTokenBudget);
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.ThinkingConfig.litert_lm_thinking_config_delete(Native);
        }
    }

    public sealed class RepetitionPenaltyConfig : LiteRTLMNativeHandle
    {
        /// <summary>
        /// Creates a managed wrapper around a LiteRT LM repetition penalty configuration.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose()"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public RepetitionPenaltyConfig()
        {
            Native = NativeAPI.RepetitionPenaltyConfig.litert_lm_repetition_penalty_config_create();
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native repetition penalty config.");
        }

        /// <summary>Sets the repetition penalty for the repetition penalty config.</summary>
        /// <param name="repetitionPenalty">A multiplicative penalty for any token already generated.</param>
        public void SetRepetitionPenalty(float repetitionPenalty)
        {
            ThrowIfDisposed();
            NativeAPI.RepetitionPenaltyConfig.litert_lm_repetition_penalty_config_set_repetition_penalty(Native, repetitionPenalty);
        }
        
        /// <summary>Sets the presence penalty for the repetition penalty config.</summary>
        /// <param name="presencePenalty">A scalar subtracted from a logit if a token has appeared at least once.</param>
        public void SetPresencePenalty(float presencePenalty)
        {
            ThrowIfDisposed();
            NativeAPI.RepetitionPenaltyConfig.litert_lm_repetition_penalty_config_set_presence_penalty(Native, presencePenalty);
        }
        
        /// Sets the frequency penalty for the repetition penalty config.
        /// <param name="frequencyPenalty">A scalar subtracted from a token's logit scaled by previous appearances.</param>
        public void SetFrequencyPenalty(float frequencyPenalty)
        {
            ThrowIfDisposed();
            NativeAPI.RepetitionPenaltyConfig.litert_lm_repetition_penalty_config_set_frequency_penalty(Native, frequencyPenalty);
        }

        /// <summary>Sets the window size for the repetition penalty config.</summary>
        /// <param name="windowSize">The maximum number of recent tokens to consider.</param>
        public void SetWindowSize(int windowSize)
        {
            ThrowIfDisposed();
            NativeAPI.RepetitionPenaltyConfig.litert_lm_repetition_penalty_config_set_window_size(Native, windowSize);
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.RepetitionPenaltyConfig.litert_lm_repetition_penalty_config_delete(Native);
        }
    }
    
    public sealed class ConversationOptionalArgs : LiteRTLMNativeHandle
    {
        /// <summary>
        /// Creates a managed wrapper around the optional arguments for conversation APIs.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose()"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public ConversationOptionalArgs()
        {
            Native = NativeAPI.ConversationOptionalArgs.litert_lm_conversation_optional_args_create();
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native conversation optional args.");
        }

        /// <summary>Sets the repetition penalty configuration for the conversation optional args.</summary>
        /// <param name="repetitionPenaltyConfig">The repetition penalty config to set. If <see langword="null"/>, clears any previously set repetition penalty config.</param>
        public void SetRepetitionPenaltyConfig(RepetitionPenaltyConfig? repetitionPenaltyConfig)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationOptionalArgs.litert_lm_conversation_optional_args_set_repetition_penalty_config(Native, repetitionPenaltyConfig);
        }

        /// <summary>Sets the visual token budget for the conversation optional arguments.</summary>
        /// <param name="visualTokenBudget">The visual token budget.</param>
        public void SetVisualTokenBudget(int visualTokenBudget)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationOptionalArgs.litert_lm_conversation_optional_args_set_visual_token_budget(
                Native, visualTokenBudget);
        }
        
        /// <summary>Sets the maximum number of output tokens for the conversation optional arguments.</summary>
        /// <param name="maxOutputTokens">The maximum number of output tokens.</param>
        public void SetMaxOutputTokens(int maxOutputTokens)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationOptionalArgs.litert_lm_conversation_optional_args_set_max_output_tokens(
                Native, maxOutputTokens);
        }

        /// <summary>Sets the thinking config for the conversation optional args.</summary>
        /// <param name="thinkingConfig">The thinking config to set. If <see langword="null"/>, clears any previously set thinking config.</param>
        public void SetThinkingConfig(ThinkingConfig? thinkingConfig)
        {
            ThrowIfDisposed();
            NativeAPI.ConversationOptionalArgs.litert_lm_conversation_optional_args_set_thinking_config(Native, thinkingConfig);
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.ConversationOptionalArgs.litert_lm_conversation_optional_args_delete(Native);
        }
    }

    public sealed class InputData : LiteRTLMNativeHandle
    {
        /// <summary>
        /// Creates a managed wrapper around LiteRT LM input data.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose()"/>.
        /// </summary>
        /// <param name="dataType">The type of the input data.</param>
        /// <param name="nativeData">The data buffer. The data is copied internally.</param>
        /// <param name="size">The size of the data in bytes.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="nativeData"/> or <paramref name="size"/> is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public InputData(InputDataType dataType, IntPtr nativeData, UIntPtr size)
        {
            if (nativeData == IntPtr.Zero || size == UIntPtr.Zero)
                throw new ArgumentException("Invalid data and/or size.");
            
            Native = NativeAPI.InputData.litert_lm_input_data_create(dataType, nativeData, size);
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native input data.");
        }

        /// <summary>
        /// Creates a managed wrapper around text input data.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose()"/>.
        /// </summary>
        /// <param name="data">The text data.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="data"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public InputData(string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("Data cannot be null or empty.", nameof(data));
            
            using UnsafeUtils.TempMem tempMem = UnsafeUtils.AllocateStringUTF8(data);
            Native = NativeAPI.InputData.litert_lm_input_data_create(InputDataType.Text, tempMem.Ptr, tempMem.Size);
            
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native input data.");
        }

        /// <summary>
        /// Creates a managed wrapper around binary input data.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose()"/>.
        /// </summary>
        /// <param name="dataType">The type of the input data.</param>
        /// <param name="data">The data buffer.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="data"/> is empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public unsafe InputData(InputDataType dataType, ReadOnlySpan<byte> data)
        {
            if (data.IsEmpty)
                throw new ArgumentException("Data cannot be empty.", nameof(data));
            
            fixed (byte* dataPtr = data)
                Native = NativeAPI.InputData.litert_lm_input_data_create(dataType, (IntPtr)dataPtr, (UIntPtr)data.Length);
            
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native input data.");
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.InputData.litert_lm_input_data_delete(Native);
        }
    }

    public sealed class EngineSettings : LiteRTLMNativeHandle
    {
        /// <summary>
        /// Creates managed engine settings from a model path.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose"/>.
        /// </summary>
        /// <remarks>See <see cref="BackendNames"/> for valid backend strings.</remarks>
        /// <param name="modelPath">The path to the model file.</param>
        /// <param name="backend">The backend to use (for example, "cpu" or "gpu").</param>
        /// <param name="visionBackend">The vision backend to use, or <see langword="null"/> if not set.</param>
        /// <param name="audioBackend">The audio backend to use, or <see langword="null"/> if not set.</param>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public EngineSettings(string modelPath, string backend,
            string? visionBackend = null, string? audioBackend = null)
        {
            if (string.IsNullOrEmpty(modelPath))
                throw new ArgumentException("Model path cannot be null or empty.", nameof(modelPath));
            
            if (string.IsNullOrEmpty(backend))
                throw new ArgumentException("Backend cannot be null or empty.", nameof(backend));
            
            Native = NativeAPI.EngineSettings.litert_lm_engine_settings_create(modelPath, backend, visionBackend, audioBackend);
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native engine settings.");
        }
        
        /// <summary>
        /// Creates managed engine settings from a raw file descriptor.
        /// The engine takes ownership of the file descriptor and closes it when done.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose"/>.
        /// </summary>
        /// <remarks>See <see cref="BackendNames"/> for valid backend strings.</remarks>
        /// <param name="fd">The file descriptor of the model.</param>
        /// <param name="backend">The backend to use (for example, "cpu" or "gpu").</param>
        /// <param name="visionBackend">The vision backend to use, or <see langword="null"/> if not set.</param>
        /// <param name="audioBackend">The audio backend to use, or <see langword="null"/> if not set.</param>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public EngineSettings(int fd, string backend,
            string? visionBackend = null, string? audioBackend = null)
        {
            if (string.IsNullOrEmpty(backend))
                throw new ArgumentException("Backend cannot be null or empty.", nameof(backend));
            
            Native = NativeAPI.EngineSettings.litert_lm_engine_settings_create_from_raw_file_descriptor(fd, backend, visionBackend, audioBackend);
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native engine settings.");
        }

        /// <summary>Sets the maximum number of tokens for the engine.</summary>
        /// <param name="maxNumTokens">The maximum number of tokens.</param>
        public void SetMaxNumTokens(int maxNumTokens)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_max_num_tokens(Native, maxNumTokens);
        }
        
        /// <summary>Sets the number of threads for the CPU backend.</summary>
        /// <param name="numThreads">The number of threads.</param>
        public void SetNumThreads(int numThreads)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_num_threads(Native, numThreads);
        }
        
        /// <summary>Sets the number of threads for the audio CPU backend.</summary>
        /// <param name="numThreads">The number of threads.</param>
        public void SetAudioNumThreads(int numThreads)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_audio_num_threads(Native, numThreads);
        }
        
        /// <summary>
        /// Sets whether the engine should load different sections of the litertlm file
        /// in parallel. Defaults to <see langword="true"/>.
        /// </summary>
        /// <param name="parallelFileSectionLoading">Whether to load in parallel.</param>
        public void SetParallelFileSectionLoading(bool parallelFileSectionLoading)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_parallel_file_section_loading(Native, parallelFileSectionLoading);
        }
        
        /// <summary>
        /// Sets the maximum number of images for the engine.
        /// This is only used for the legacy implementation of the engine.
        /// </summary>
        /// <param name="maxNumImages">The maximum number of images.</param>
        public void SetMaxNumImages(int maxNumImages)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_max_num_images(Native, maxNumImages);
        }

        /// <summary>Sets the cache directory for the engine.</summary>
        /// <param name="cacheDir">The cache directory.</param>
        public void SetCacheDir(string cacheDir)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_cache_dir(Native, cacheDir);
        }
        
        /// <summary>Sets the LiteRT dispatch library directory for the NPU backend.</summary>
        /// <param name="libDir">The dispatch library directory.</param>
        public void SetLitertDispatchLibDir(string libDir)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_litert_dispatch_lib_dir(Native, libDir);
        }
        
        /// <summary>Sets the activation data type.</summary>
        /// <param name="activationDataType">The activation data type.</param>
        public void SetActivationDataType(ActivationDataType activationDataType)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_activation_data_type(Native, activationDataType);
        }
        
        /// <summary>
        /// Sets the prefill chunk size for the engine. Only applicable for the CPU backend
        /// with dynamic models.
        /// </summary>
        /// <param name="prefillChunkSize">The prefill chunk size.</param>
        public void SetPrefillChunkSize(int prefillChunkSize)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_prefill_chunk_size(Native, prefillChunkSize);
        }
        
        /// <summary>Enables benchmarking for the engine.</summary>
        public void EnableBenchmark()
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_enable_benchmark(Native);
        }
        
        /// <summary>Sets the number of prefill tokens for benchmarking.</summary>
        /// <param name="numPrefillTokens">The number of prefill tokens.</param>
        public void SetNumPrefillTokens(int numPrefillTokens)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_num_prefill_tokens(Native, numPrefillTokens);
        }
        
        /// <summary>Sets the number of decode tokens for benchmarking.</summary>
        /// <param name="numDecodeTokens">The number of decode tokens.</param>
        public void SetNumDecodeTokens(int numDecodeTokens)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_num_decode_tokens(Native, numDecodeTokens);
        }
        
        /// <summary>Sets whether to enable speculative decoding.</summary>
        /// <param name="enableSpeculativeDecoding">Whether to enable speculative decoding.</param>
        public void SetEnableSpeculativeDecoding(bool enableSpeculativeDecoding)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_enable_speculative_decoding(Native, enableSpeculativeDecoding);
        }
        
        /// <summary>Sets the LoRA rank for the engine.</summary>
        /// <param name="loraRank">The LoRA rank.</param>
        public void SetLoraRank(int loraRank)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_lora_rank(Native, loraRank);
        }
        
        /// <summary>Sets the supported LoRA ranks for the engine.</summary>
        /// <param name="loraRanks">An array of supported LoRA ranks.</param>
        /// <returns>0 on success, non-zero on failure.</returns>
        public int SetSupportedLoraRanks(int[] loraRanks)
        {
            ThrowIfDisposed();
            return NativeAPI.EngineSettings.litert_lm_engine_settings_set_supported_lora_ranks(Native, loraRanks, (UIntPtr)loraRanks.Length);
        }
        
        /// <summary>Sets the Audio LoRA rank for the engine.</summary>
        /// <param name="loraRank">The Audio LoRA rank.</param>
        public void SetAudioLoraRank(int loraRank)
        {
            ThrowIfDisposed();
            NativeAPI.EngineSettings.litert_lm_engine_settings_set_audio_lora_rank(Native, loraRank);
        }
        
        /// <summary>Sets the supported Audio LoRA ranks for the engine.</summary>
        /// <param name="loraRanks">An array of supported Audio LoRA ranks.</param>
        /// <returns>0 on success, non-zero on failure.</returns>
        public int SetSupportedAudioLoraRanks(int[] loraRanks)
        {
            ThrowIfDisposed();
            return NativeAPI.EngineSettings.litert_lm_engine_settings_set_supported_audio_lora_ranks(Native, loraRanks, (UIntPtr)loraRanks.Length);
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.EngineSettings.litert_lm_engine_settings_delete(Native);
        }
    }

    public sealed class Engine : LiteRTLMNativeHandle
    {
        /// <summary>
        /// Creates a managed wrapper around a LiteRT LM engine from the given settings.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose"/>.
        /// </summary>
        /// <param name="settings">The engine settings.</param>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public Engine(EngineSettings settings)
        {
            Native = NativeAPI.Engine.litert_lm_engine_create(settings);
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native engine.");
        }

        /// <summary>
        /// Creates a managed wrapper around a LiteRT LM session.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose"/>.
        /// </summary>
        /// <param name="config">The session configuration to use. If <see langword="null"/>, the default session configuration is used.</param>
        /// <returns>The created session wrapper, or <see langword="null"/> on failure.</returns>
        public Session? CreateSession(SessionConfig? config = null)
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Engine.litert_lm_engine_create_session(Native, config);
            return ptr != IntPtr.Zero ? new Session(ptr) : null;
        }

        /// <summary>Tokenizes text using the engine's tokenizer.</summary>
        /// <param name="text">The UTF-8 string to tokenize.</param>
        /// <returns>
        /// The tokenize result wrapper, or <see langword="null"/> on failure.
        /// The caller is responsible for disposing the returned wrapper.
        /// </returns>
        public TokenizeResult? Tokenize(string text)
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Engine.litert_lm_engine_tokenize(Native, text);
            return ptr != IntPtr.Zero ? new TokenizeResult(ptr) : null;
        }
        
        /// <summary>Detokenizes token ids using the engine's tokenizer.</summary>
        /// <param name="tokens">An array of token ids to detokenize.</param>
        /// <returns>
        /// The detokenize result wrapper, or <see langword="null"/> on failure.
        /// The caller is responsible for disposing the returned wrapper.
        /// </returns>
        public DetokenizeResult? Detokenize(int[] tokens)
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Engine.litert_lm_engine_detokenize(Native, tokens, (UIntPtr)tokens.Length);
            return ptr != IntPtr.Zero ? new DetokenizeResult(ptr) : null;
        }

        /// <summary>Returns the configured start token (BOS), if any.</summary>
        /// <returns>
        /// The start token wrapper, or <see langword="null"/> if none is configured.
        /// The caller is responsible for disposing the returned wrapper.
        /// </returns>
        public TokenUnion? GetStartToken()
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Engine.litert_lm_engine_get_start_token(Native);
            return ptr != IntPtr.Zero ? new TokenUnion(ptr) : null;
        }

        /// <summary>Returns the configured stop tokens (EOS).</summary>
        /// <returns>
        /// The stop tokens wrapper, or <see langword="null"/> if none are configured.
        /// The caller is responsible for disposing the returned wrapper.
        /// </returns>
        public TokenUnions? GetStopTokens()
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Engine.litert_lm_engine_get_stop_tokens(Native);
            return ptr != IntPtr.Zero ? new TokenUnions(ptr) : null;
        }

        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.Engine.litert_lm_engine_delete(Native);
        }
    }
    
    public sealed class Session : LiteRTLMNativeHandle
    {
        internal Session(IntPtr native)
        {
            Native = native;
        }

        /// <summary>Cancels the current processing in the session.</summary>
        public void CancelProcess()
        {
            ThrowIfDisposed();
            NativeAPI.Session.litert_lm_session_cancel_process(Native);
        }

        /// <summary>
        /// Adds the input prompt or query to the model and starts the prefill process.
        /// This is a blocking call and returns when prefill is complete.
        /// </summary>
        /// <param name="inputs">An array of input wrappers representing multimodal input.</param>
        /// <returns>0 on success, non-zero on failure.</returns>
        public int RunPrefill(ReadOnlySpan<InputData> inputs)
        {
            ThrowIfDisposed();
            return NativeAPI.Session.litert_lm_session_run_prefill(Native, inputs.GetPtrs(), (UIntPtr)inputs.Length);
        }

        /// <summary>
        /// Starts decoding for the model to predict a response based on the input prompt or query
        /// added after calling <see cref="RunPrefill"/>.
        /// This is a blocking call and returns when decoding is complete.
        /// </summary>
        /// <returns>
        /// The responses wrapper, or <see langword="null"/> on failure.
        /// The caller is responsible for disposing the returned wrapper.
        /// </returns>
        public Responses? RunDecode()
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Session.litert_lm_session_run_decode(Native);
            return ptr != IntPtr.Zero ? new Responses(ptr) : null;
        }

        /// <summary>Scores the target text after the prefill process is complete.</summary>
        /// <param name="targetText">An array of target text strings to score.</param>
        /// <param name="storeTokenLengths">Whether to store the token lengths of the target texts in the responses.</param>
        /// <returns>
        /// The responses wrapper, or <see langword="null"/> on failure.
        /// The caller is responsible for disposing the returned wrapper.
        /// </returns>
        public Responses? RunTextScoring(string[] targetText, bool storeTokenLengths)
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Session.litert_lm_session_run_text_scoring(Native, targetText, (UIntPtr)targetText.Length, storeTokenLengths);
            return ptr != IntPtr.Zero ? new Responses(ptr) : null;
        }

        /// <summary>Generates content from the input prompt.</summary>
        /// <param name="inputs">An array of input wrappers representing multimodal input.</param>
        /// <returns>
        /// The responses wrapper, or <see langword="null"/> on failure.
        /// The caller is responsible for disposing the returned wrapper.
        /// </returns>
        public Responses? GenerateContent(ReadOnlySpan<InputData> inputs)
        {
            ThrowIfDisposed();
            IntPtr ptr = NativeAPI.Session.litert_lm_session_generate_content(Native, inputs.GetPtrs(), (UIntPtr)inputs.Length);
            return ptr != IntPtr.Zero ? new Responses(ptr) : null;
        }

        /// <summary>
        /// Retrieves benchmark information for the session.
        /// The caller is responsible for disposing the returned wrapper.
        /// </summary>
        /// <returns>The benchmark information wrapper, or <see langword="null"/> on failure.</returns>
        public BenchmarkInfo? GetBenchmarkInfo()
        {
            ThrowIfDisposed();

            IntPtr ptr = NativeAPI.Session.litert_lm_session_get_benchmark_info(Native);
            return ptr != IntPtr.Zero ? new BenchmarkInfo(ptr) : null;
        }

        /// <summary>
        /// Starts decoding for the model to predict a response based on the input prompt or query
        /// added after calling <see cref="RunPrefill"/>.
        /// This is a non-blocking call that streams response chunks through a callback.
        /// </summary>
        /// <param name="callback">The callback function (<see cref="StreamCallback"/>) that receives response chunks.</param>
        /// <returns>0 on success, non-zero on failure.</returns>
        public int RunDecodeAsync(StreamCallback callback)
        {
            ThrowIfDisposed();
            
            IntPtr callbackData = StreamCallbackHandler.Register(callback);
            int result = NativeAPI.Session.litert_lm_session_run_decode_async(Native,
                StreamCallbackHandler.GlobalStreamCallbackListenerPtr, callbackData);
            
            if (result != 0)
                StreamCallbackHandler.Deregister(callbackData);
            return result;
        }

        /// <summary>
        /// Generates content from the input prompt and streams the response through a callback.
        /// This is a non-blocking call and invokes the callback from a background thread for each chunk.
        /// </summary>
        /// <param name="inputs">An array of input wrappers representing multimodal input.</param>
        /// <param name="callback">The callback function (<see cref="StreamCallback"/>) that receives response chunks.</param>
        /// <returns>0 on success, non-zero on failure to start the stream.</returns>
        public int GenerateContentStream(ReadOnlySpan<InputData> inputs, StreamCallback callback)
        {
            ThrowIfDisposed();
            
            IntPtr callbackData = StreamCallbackHandler.Register(callback);
            int result = NativeAPI.Session.litert_lm_session_generate_content_stream(Native,
                inputs.GetPtrs(), (UIntPtr)inputs.Length, StreamCallbackHandler.GlobalStreamCallbackListenerPtr, callbackData);
            
            if (result != 0)
                StreamCallbackHandler.Deregister(callbackData);
            return result;
        }

        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.Session.litert_lm_session_delete(Native);
        }
    }
    
    public sealed class Responses : LiteRTLMNativeHandle, IReadOnlyList<Responses.Candidate>
    {
        /// <param name="Text">The response text.</param>
        /// <param name="HasScore">Does the response have a valid <see cref="Score"/>?</param>
        /// <param name="Score">The response score.</param>
        /// <param name="HasTokenLength">Does the response have a valid <see cref="TokenLength"/>?</param>
        /// <param name="TokenLength">The token length of this response.</param>
        /// <param name="HasTokenScores">Does the response have a valid <see cref="TokenScores"/>?</param>
        /// <param name="TokenScores">The token scores for this response.</param>
        public record Candidate(
            string Text,
            bool HasScore, float Score,
            bool HasTokenLength, int TokenLength,
            bool HasTokenScores, float[]? TokenScores
        );
        
        /// <inheritdoc/>
        public int Count => GetNumCandidates();
        
        /// <inheritdoc/>
        public Candidate this[int index] => GetCandidateAt(index);
        
        internal Responses(IntPtr native)
        {
            Native = native;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The returned enumerator is only valid for the
        /// lifetime of this <see cref="Responses"/> object.
        /// </remarks>
        public IEnumerator<Candidate> GetEnumerator()
        {
            int length = GetNumCandidates();
            for (int i = 0; i < length; i++)
                yield return GetCandidateAt(i);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The returned enumerator is only valid for the
        /// lifetime of this <see cref="Responses"/> object.
        /// </remarks>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private Candidate GetCandidateAt(int index)
        {
            string response = GetResponseTextAt(index);
                
            bool hasScore = HasScoreAt(index);
            float score = hasScore ? GetScoreAt(index) : 0.0f;

            bool hasTokenLength = HasTokenLengthAt(index);
            int tokenLength = hasTokenLength ? GetTokenLengthAt(index) : 0;
                
            bool hasTokenScores = HasTokenScoresAt(index);
            float[]? tokenScores = hasTokenScores
                ? UnsafeUtils.CopyFrom<float>(
                    GetTokenScoresAt(index),
                    GetNumTokenScoresAt(index)
                ) : null;
                
            return new Candidate(
                response,
                hasScore, score,
                hasTokenLength, tokenLength,
                hasTokenScores, tokenScores
            );
        }

        /// <summary>Returns the number of response candidates.</summary>
        /// <returns>The number of candidates.</returns>
        private int GetNumCandidates()
        {
            ThrowIfDisposed();
            return NativeAPI.Responses.litert_lm_responses_get_num_candidates(Native);
        }

        /// <summary>Returns the response text at a given index.</summary>
        /// <param name="index">The index of the response.</param>
        /// <returns>The response text.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
        private string GetResponseTextAt(int index)
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Responses.litert_lm_responses_get_response_text_at(Native, index);
            return ptr != IntPtr.Zero ? UnsafeUtils.MarshalStringUTF8(ptr) : throw new IndexOutOfRangeException();
        }

        /// <summary>Returns whether the response contains a score at the given index.</summary>
        /// <param name="index">The index of the response.</param>
        /// <returns><see langword="true"/> if the score is available at the given index, <see langword="false"/> otherwise.</returns>
        private bool HasScoreAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.Responses.litert_lm_responses_has_score_at(Native, index);
        }
        
        /// <summary>Returns the score at a given index.</summary>
        /// <param name="index">The index of the response.</param>
        /// <returns>The score. Returns 0.0f if the index is out of bounds or no score is present.</returns>
        private float GetScoreAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.Responses.litert_lm_responses_get_score_at(Native, index);
        }
        
        /// <summary>Returns whether the response contains a token length at the given index.</summary>
        /// <param name="index">The index of the response.</param>
        /// <returns><see langword="true"/> if the token length is available at the given index, <see langword="false"/> otherwise.</returns>
        private bool HasTokenLengthAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.Responses.litert_lm_responses_has_token_length_at(Native, index);
        }
        
        /// <summary>Returns the token length at a given index.</summary>
        /// <param name="index">The index of the response.</param>
        /// <returns>
        /// The token length. Returns 0 if the index is out of bounds or no token
        /// length is present.
        /// </returns>
        private int GetTokenLengthAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.Responses.litert_lm_responses_get_token_length_at(Native, index);
        }
        
        /// <summary>Returns whether the response contains token scores at the given index.</summary>
        /// <param name="index">The index of the response.</param>
        /// <returns><see langword="true"/> if token scores are available at the given index, <see langword="false"/> otherwise.</returns>
        private bool HasTokenScoresAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.Responses.litert_lm_responses_has_token_scores_at(Native, index);
        }
        
        /// <summary>Returns the number of token scores available for a candidate at a given index.</summary>
        /// <param name="index">The index of the response.</param>
        /// <returns>
        /// The number of token scores. Returns 0 if the index is out of bounds or no
        /// token scores are present.
        /// </returns>
        private int GetNumTokenScoresAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.Responses.litert_lm_responses_get_num_token_scores_at(Native, index);
        }

        /// <summary>Returns the token scores for a candidate at a given index.</summary>
        /// <param name="index">The index of the response.</param>
        /// <returns>
        /// The internal token score buffer. The returned data is valid only for the
        /// lifetime of this <see cref="Responses"/> object.
        /// </returns>
        private IntPtr GetTokenScoresAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.Responses.litert_lm_responses_get_token_scores_at(Native, index);
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.Responses.litert_lm_responses_delete(Native);
        }
    }
    
    public sealed class BenchmarkInfo : LiteRTLMNativeHandle
    {
        /// <summary>Benchmarks for a prefill or decode turn.</summary>
        public readonly struct Turn
        {
            /// <summary>The prefill/decode token count for this turn.</summary>
            public readonly int TokenCount;
            
            /// <summary>The prefill/decode tokens per second for this turn.</summary>
            public readonly double TokensPerSecond;

            public Turn(int tokenCount, double tokensPerSecond)
            {
                TokenCount = tokenCount;
                TokensPerSecond = tokensPerSecond;
            }
        }
        
        internal BenchmarkInfo(IntPtr native)
        {
            Native = native;
        }

        /// <summary>Returns the time to the first token in seconds.</summary>
        /// <remarks>
        /// Note that the time to first token does not include initialization time.
        /// It is the sum of the prefill time for the first turn and the time spent
        /// decoding the first token.
        /// </remarks>
        /// <returns>The time to the first token in seconds.</returns>
        public double GetTimeToFirstToken()
        {
            ThrowIfDisposed();
            return NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_get_time_to_first_token(Native);
        }

        /// <summary>Returns the total initialization time in seconds.</summary>
        /// <returns>The total initialization time in seconds.</returns>
        public double GetTotalInitTime()
        {
            ThrowIfDisposed();
            return NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_get_total_init_time_in_second(Native);
        }

        /// <summary>Returns the benchmark information for each prefill turn.</summary>
        public Turn[] GetPrefillTurns()
        {
            ThrowIfDisposed();
            int length = GetNumPrefillTurns();
            Turn[] turns = new Turn[length];

            for (int i = 0; i < length; i++)
            {
                turns[i] = new Turn(
                    GetPrefillTokenCountAt(i),
                    GetPrefillTokensPerSecAt(i)
                );
            }

            return turns;
        }
        
        /// <summary>Returns the benchmark information for each decode turn.</summary>
        public Turn[] GetDecodeTurns()
        {
            ThrowIfDisposed();
            int length = GetNumDecodeTurns();
            Turn[] turns = new Turn[length];

            for (int i = 0; i < length; i++)
            {
                turns[i] = new Turn(
                    GetDecodeTokenCountAt(i),
                    GetDecodeTokensPerSecAt(i)
                );
            }

            return turns;
        }
        
        /// <summary>Returns the number of prefill turns.</summary>
        /// <returns>The number of prefill turns.</returns>
        private int GetNumPrefillTurns()
        {
            ThrowIfDisposed();
            return NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_get_num_prefill_turns(Native);
        }
        
        /// <summary>Returns the prefill token count at a given turn index.</summary>
        /// <param name="index">The index of the prefill turn.</param>
        /// <returns>The prefill token count.</returns>
        private int GetPrefillTokenCountAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_get_prefill_token_count_at(Native, index);
        }
        
        /// <summary>Returns the prefill tokens per second at a given turn index.</summary>
        /// <param name="index">The index of the prefill turn.</param>
        /// <returns>The prefill tokens per second.</returns>
        private double GetPrefillTokensPerSecAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_get_prefill_tokens_per_sec_at(Native, index);
        }

        /// <summary>Returns the number of decode turns.</summary>
        /// <returns>The number of decode turns.</returns>
        private int GetNumDecodeTurns()
        {
            ThrowIfDisposed();
            return NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_get_num_decode_turns(Native);
        }
        
        /// <summary>Returns the decode token count at a given turn index.</summary>
        /// <param name="index">The index of the decode turn.</param>
        /// <returns>The decode token count.</returns>
        private int GetDecodeTokenCountAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_get_decode_token_count_at(Native, index);
        }
        
        /// <summary>Returns the decode tokens per second at a given turn index.</summary>
        /// <param name="index">The index of the decode turn.</param>
        /// <returns>The decode tokens per second.</returns>
        private double GetDecodeTokensPerSecAt(int index)
        {
            ThrowIfDisposed();
            return NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_get_decode_tokens_per_sec_at(Native, index);
        }

        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_delete(Native);
        }
    }

    public sealed class StreamChunk
    {
        private readonly IntPtr _native;
        private bool _isValid = true;
        
        public StreamChunk(IntPtr native)
        {
            _native = native;
        }
        
        public static implicit operator IntPtr(StreamChunk? handle) => handle?._native ?? IntPtr.Zero;

        /// <summary>Gets the text content of the chunk.</summary>
        /// <returns>Returns <see langword="null"/> if there is no text content in this chunk (e.g. if it is an error or metadata-only chunk).</returns>
        public string? GetText()
        {
            ThrowIfInvalid();

            IntPtr ptr = NativeAPI.StreamChunk.litert_lm_stream_chunk_get_text(_native);
            return ptr != IntPtr.Zero ? UnsafeUtils.MarshalStringUTF8(ptr) : null;
        }

        /// <summary>Returns <see langword="true"/> if this is the final chunk of the stream.</summary>
        public bool IsFinal()
        {
            ThrowIfInvalid();
            return NativeAPI.StreamChunk.litert_lm_stream_chunk_is_final(_native);
        }
        
        /// <summary>Gets the error message associated with this chunk, if any.</summary>
        /// <remarks>Returns <see langword="null"/> if there is no error.</remarks>
        public string? GetError()
        {
            ThrowIfInvalid();

            IntPtr ptr = NativeAPI.StreamChunk.litert_lm_stream_chunk_get_error(_native);
            return ptr != IntPtr.Zero ? UnsafeUtils.MarshalStringUTF8(ptr) : null;
        }

        internal void Invalidate() =>
            _isValid = false;

        private void ThrowIfInvalid()
        {
            if (!_isValid)
                throw new ObjectDisposedException(nameof(StreamChunk));
        }
    }
    
    public sealed class Conversation : LiteRTLMNativeHandle
    {
        /// <summary>
        /// Creates a managed wrapper around a LiteRT LM conversation.
        /// The caller is responsible for disposing the wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose"/>.
        /// </summary>
        /// <param name="engine">The engine to create the conversation from.</param>
        /// <param name="config">The conversation configuration to use. If <see langword="null"/>, the default configuration is used.</param>
        /// <exception cref="InvalidOperationException">Thrown if the native object could not be created.</exception>
        public Conversation(Engine engine, ConversationConfig? config = null)
        {
            Native = NativeAPI.Conversation.litert_lm_conversation_create(engine, config);
            if (Native == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create native conversation.");
        }

        private Conversation(IntPtr native)
        {
            Native = native;
        }

        /// <summary>
        /// Clones a LiteRT LM conversation, duplicating its prefilled state.
        /// The caller is responsible for disposing the cloned wrapper using
        /// <see cref="LiteRTLMNativeHandle.Dispose"/>.
        /// </summary>
        /// <returns>The cloned conversation wrapper, or <see langword="null"/> on failure.</returns>
        public Conversation? Clone()
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Conversation.litert_lm_conversation_clone(Native);
            return ptr != IntPtr.Zero ? new Conversation(ptr) : null;
        }

        /// <summary>
        /// Sends a message to the conversation and returns the response.
        /// This is a blocking call.
        /// </summary>
        /// <param name="messageJson">A JSON string representing the message to send.</param>
        /// <param name="extraContext">A JSON string representing the extra context to use.</param>
        /// <param name="optionalArgs">The optional arguments to use.</param>
        /// <returns>
        /// A JSON response wrapper, or <see langword="null"/> on failure.
        /// The caller is responsible for disposing the returned wrapper.
        /// </returns>
        public JsonResponse? SendMessage(string messageJson,
            string? extraContext = null, ConversationOptionalArgs? optionalArgs = null)
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Conversation.litert_lm_conversation_send_message(Native, messageJson, extraContext, optionalArgs);
            return ptr != IntPtr.Zero ? new JsonResponse(ptr) : null;
        }

        /// <summary>
        /// Sends a message to the conversation and streams the response through a
        /// callback. This is a non-blocking call that invokes the callback from a
        /// background thread for each chunk.
        /// </summary>
        /// <param name="messageJson">A JSON string representing the message to send.</param>
        /// <param name="extraContext">A JSON string representing the extra context to use.</param>
        /// <param name="optionalArgs">The optional arguments to use.</param>
        /// <param name="callback">The callback function (<see cref="StreamCallback"/>) that receives response chunks.</param>
        /// <returns>0 on success, non-zero on failure to start the stream.</returns>
        public int SendMessageStream(StreamCallback callback, string messageJson,
            string? extraContext = null, ConversationOptionalArgs? optionalArgs = null)
        {
            ThrowIfDisposed();
            
            IntPtr callbackData = StreamCallbackHandler.Register(callback);
            int result = NativeAPI.Conversation.litert_lm_conversation_send_message_stream(Native,
                messageJson, extraContext, optionalArgs, StreamCallbackHandler.GlobalStreamCallbackListenerPtr, callbackData);
            
            if (result != 0)
                StreamCallbackHandler.Deregister(callbackData);
            return result;
        }

        /// <summary>Renders the message into a string according to the template.</summary>
        /// <remarks>
        /// This function does not need to be called for actual message sending, as
        /// <see cref="SendMessage"/> and <see cref="SendMessageStream"/> render
        /// messages internally.
        /// </remarks>
        /// <param name="messageJson">A JSON string representing the message to render.</param>
        /// <returns>The rendered string, or <see langword="null"/> on failure.</returns>
        public string? RenderMessageToString(string messageJson)
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Conversation.litert_lm_conversation_render_message_to_string(Native, messageJson);
            return ptr != IntPtr.Zero ? UnsafeUtils.MarshalStringUTF8(ptr) : null;
        }
        
        /// <summary>Renders the preface into a string according to the template.</summary>
        /// <returns>The rendered string, or <see langword="null"/> on failure.</returns>
        public string? RenderPrefaceToString()
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Conversation.litert_lm_conversation_render_preface_to_string(Native);
            return ptr != IntPtr.Zero ? UnsafeUtils.MarshalStringUTF8(ptr) : null;
        }

        /// <summary>Cancels the ongoing inference process for asynchronous inference.</summary>
        public void CancelProcess()
        {
            ThrowIfDisposed();
            NativeAPI.Conversation.litert_lm_conversation_cancel_process(Native);
        }

        /// <summary>
        /// Retrieves benchmark information from the conversation.
        /// The caller is responsible for disposing the returned wrapper.
        /// </summary>
        /// <returns>The benchmark information wrapper, or <see langword="null"/> on failure.</returns>
        public BenchmarkInfo? GetBenchmarkInfo()
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.Conversation.litert_lm_conversation_get_benchmark_info(Native);
            return ptr != IntPtr.Zero ? new BenchmarkInfo(ptr) : null;
        }

        /// <summary>Gets the number of tokens in the conversation KV cache (prefill + decode).</summary>
        /// <returns>The number of tokens, or a negative value on failure.</returns>
        public int GetTokenCount()
        {
            ThrowIfDisposed();
            return NativeAPI.Conversation.litert_lm_conversation_get_token_count(Native);
        }

        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.Conversation.litert_lm_conversation_delete(Native);
        }
    }

    public sealed class JsonResponse : LiteRTLMNativeHandle
    {
        internal JsonResponse(IntPtr native)
        {
            Native = native;
        }

        /// <summary>Returns the JSON response string.</summary>
        /// <returns>The response JSON string.</returns>
        public string GetString()
        {
            ThrowIfDisposed();
            IntPtr ptr = NativeAPI.JsonResponse.litert_lm_json_response_get_string(Native);
            return UnsafeUtils.MarshalStringUTF8(ptr);
        }

        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.JsonResponse.litert_lm_json_response_delete(Native);
        }
    }

    public sealed class TokenizeResult : LiteRTLMNativeHandle
    {
        internal TokenizeResult(IntPtr native)
        {
            Native = native;
        }

        /// <summary>Returns a span of the token ids.</summary>
        /// <remarks>
        /// The returned span is only valid for the lifetime of this
        /// <see cref="TokenizeResult"/> object.
        /// </remarks>
        public unsafe ReadOnlySpan<int> AsReadOnlySpan()
        {
            int length = (int)GetNumTokens();
            IntPtr ptr = GetTokens();
            
            return new ReadOnlySpan<int>((void*)ptr, length);
        }

        /// <summary>Copies the token ids into <paramref name="span"/>.</summary>
        /// <returns>The number of copied elements.</returns>
        public long CopyTo(Span<int> span)
        {
            UIntPtr length = GetNumTokens();
            IntPtr ptr = GetTokens();
            
            return UnsafeUtils.CopyTo(ptr, length, span);
        }

        /// <summary>Returns the token ids from a tokenize result.</summary>
        /// <returns>
        /// The internal token buffer. The returned data is valid only for the
        /// lifetime of this <see cref="TokenizeResult"/> object.
        /// </returns>
        private IntPtr GetTokens()
        {
            ThrowIfDisposed();
            return NativeAPI.TokenizeResult.litert_lm_tokenize_result_get_tokens(Native);
        }
        
        /// <summary>Returns the number of token ids from a tokenize result.</summary>
        /// <returns>The number of token ids.</returns>
        private UIntPtr GetNumTokens()
        {
            ThrowIfDisposed();
            return NativeAPI.TokenizeResult.litert_lm_tokenize_result_get_num_tokens(Native);
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.TokenizeResult.litert_lm_tokenize_result_delete(Native);
        }
    }
    
    public sealed class DetokenizeResult : LiteRTLMNativeHandle
    {
        internal DetokenizeResult(IntPtr native)
        {
            Native = native;
        }

        /// <summary>Returns the string.</summary>
        /// <returns>The detokenized string.</returns>
        public string GetString()
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.DetokenizeResult.litert_lm_detokenize_result_get_string(Native);
            return UnsafeUtils.MarshalStringUTF8(ptr);
        }

        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.DetokenizeResult.litert_lm_detokenize_result_delete(Native);
        }
    }

    public sealed class TokenUnion : LiteRTLMNativeHandle
    {
        internal TokenUnion(IntPtr native)
        {
            Native = native;
        }

        /// <summary>Returns the type of the token union.</summary>
        /// <returns>The type of the token union.</returns>
        public TokenUnionType GetUnionType()
        {
            ThrowIfDisposed();
            return NativeAPI.TokenUnion.litert_lm_token_union_get_type(Native);
        }

        /// <summary>Returns the string value from a token union.</summary>
        /// <returns>
        /// The string value, or <see langword="null"/> if the token union does not
        /// contain a string value.
        /// </returns>
        public string? GetString()
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.TokenUnion.litert_lm_token_union_get_string(Native);
            return ptr != IntPtr.Zero ? UnsafeUtils.MarshalStringUTF8(ptr) : null;
        }

        /// <summary>Returns the token ids from a token union.</summary>
        /// <param name="tokenIds">The token IDs.</param>
        /// <returns>0 on success, non-zero if the token union does not contain token ids.</returns>
        public int GetIds(out int[]? tokenIds)
        {
            ThrowIfDisposed();
            
            int result = NativeAPI.TokenUnion.litert_lm_token_union_get_ids(Native, out IntPtr ptr, out UIntPtr length);
            tokenIds = result == 0 ? UnsafeUtils.CopyFrom<int>(ptr, (int)length) : null;
            return result;
        }

        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.TokenUnion.litert_lm_token_union_delete(Native);
        }
    }

    public sealed class TokenUnions : LiteRTLMNativeHandle
    {
        internal TokenUnions(IntPtr native)
        {
            Native = native;
        }

        /// <summary>Gets the token unions in this collection.</summary>
        /// <returns>
        /// The token union wrappers in the collection. The caller is responsible for
        /// disposing each returned wrapper.
        /// </returns>
        public TokenUnion[] GetTokenUnions()
        {
            int length = (int)GetNumTokens();
            TokenUnion[] result = new TokenUnion[length];

            for (int i = 0; i < length; i++)
                result[i] = GetTokenAt((UIntPtr)i)!;

            return result;
        }

        /// <summary>Returns the number of token unions in the collection.</summary>
        /// <returns>The number of token unions.</returns>
        private UIntPtr GetNumTokens()
        {
            ThrowIfDisposed();
            return NativeAPI.TokenUnions.litert_lm_token_unions_get_num_tokens(Native);
        }

        /// <summary>Returns the token union at a given index from a collection.</summary>
        /// <param name="index">The index of the token union.</param>
        /// <returns>
        /// The token union wrapper at the given index, or <see langword="null"/> if the index
        /// is out of bounds. The caller is responsible for disposing the returned wrapper.
        /// </returns>
        private TokenUnion? GetTokenAt(UIntPtr index)
        {
            ThrowIfDisposed();
            
            IntPtr ptr = NativeAPI.TokenUnions.litert_lm_token_unions_get_token_at(Native, index);
            return ptr != IntPtr.Zero ? new TokenUnion(ptr) : null;
        }
        
        protected override void ReleaseUnmanagedResources()
        {
            if (Native != IntPtr.Zero)
                NativeAPI.TokenUnions.litert_lm_token_unions_delete(Native);
        }
    }
}