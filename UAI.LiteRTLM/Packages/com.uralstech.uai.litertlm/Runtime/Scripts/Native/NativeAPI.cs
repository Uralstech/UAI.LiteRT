using System;
using System.Runtime.InteropServices;

#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    public static class NativeAPI
    {
        public const string LibLitertLM = "litert-lm";
        
        [DllImport(LibLitertLM)]
        public static extern void litert_lm_set_min_log_level([MarshalAs(UnmanagedType.I4)] LogLevel level);
        
        /// <summary>Callback for streaming responses.</summary>
        /// <param name="callbackData">A pointer to user-defined data passed to the stream function.</param>
        /// <param name="chunk">The piece of text from the stream.</param>
        /// <param name="isFinal"><see langword="true"/> if this is the last chunk in the stream.</param>
        /// <param name="errorMsg">String with an error message, or <see langword="null"/> on success.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StreamCallback(IntPtr callbackData, [MarshalAs(UnmanagedType.LPUTF8Str)] string chunk, [MarshalAs(UnmanagedType.I1)] bool isFinal, [MarshalAs(UnmanagedType.LPUTF8Str)] string? errorMsg);
        
        public static class SamplerParams
        {
            /// <summary>
            /// Creates LiteRT LM Sampler Parameters with a specific sampler type.
            /// The caller is responsible for destroying the parameters using
            /// <see cref="litert_lm_sampler_params_delete"/>.
            /// </summary>
            /// <param name="type">The sampler type to use.</param>
            /// <returns>A pointer to the created parameters, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_sampler_params_create(SamplerType type);

            /// <summary>Destroys LiteRT LM Sampler Parameters.</summary>
            /// <param name="params">The parameters to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_sampler_params_delete(IntPtr @params);
            
            /// <summary>Sets the top-k value.</summary>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_sampler_params_set_top_k(IntPtr @params, int topK);

            /// <summary>Sets the top-p value.</summary>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_sampler_params_set_top_p(IntPtr @params, float topP);

            /// <summary>Sets the temperature.</summary>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_sampler_params_set_temperature(IntPtr @params, float temperature);

            /// <summary>Sets the seed.</summary>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_sampler_params_set_seed(IntPtr @params, int seed);
        }

        public static class SessionConfig
        {
            /// <summary>
            /// Creates a LiteRT LM Session Config.
            /// The caller is responsible for destroying the config using
            /// <see cref="litert_lm_session_config_delete"/>.
            /// </summary>
            /// <returns>A pointer to the created config, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_session_config_create();

            /// <summary>Destroys a LiteRT LM Session Config.</summary>
            /// <param name="config">The config to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_session_config_delete(IntPtr config);
            
            /// <summary>Sets the maximum number of output tokens per decode step for this session.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="maxOutputTokens">The maximum number of output tokens.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_session_config_set_max_output_tokens(IntPtr config, int maxOutputTokens);

            /// <summary>Sets whether to apply prompt template for this session.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="applyPromptTemplate">Whether to apply prompt template.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_session_config_set_apply_prompt_template(IntPtr config, [MarshalAs(UnmanagedType.I1)] bool applyPromptTemplate);

            /// <summary>Sets the sampler parameters for this session config.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="samplerParams">The sampler parameters to use.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_session_config_set_sampler_params(IntPtr config, IntPtr samplerParams);
                
            /// <summary>Sets the path to the LoRA weights file.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="loraPath">The path to the text LoRA weights file.</param>
            /// <returns>0 on success, non-zero on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_session_config_set_lora_path(IntPtr config, [MarshalAs(UnmanagedType.LPUTF8Str)] string loraPath);

            /// <summary>Sets the path to the Audio LoRA weights file.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="audioLoraPath">The path to the audio LoRA weights file.</param>
            /// <returns>0 on success, non-zero on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_session_config_set_audio_lora_path(IntPtr config, [MarshalAs(UnmanagedType.LPUTF8Str)] string audioLoraPath);
        }

        public static class ConversationConfig
        {
            /// <summary>
            /// Creates a LiteRT LM Conversation Config.
            /// The caller is responsible for destroying the config using
            /// <see cref="litert_lm_conversation_config_delete"/>.
            /// </summary>
            /// <returns>A pointer to the created config, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_conversation_config_create();
            
            /// <summary>Destroys a LiteRT LM Conversation Config.</summary>
            /// <param name="config">The config to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_config_delete(IntPtr config);
            
            /// <summary>Sets the session config for this conversation config.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="sessionConfig">The session config to use.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_config_set_session_config(IntPtr config, IntPtr sessionConfig);
            
            /// <summary>Sets the system message for this conversation config.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="systemMessageJson">The system message in JSON format.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_config_set_system_message(IntPtr config, [MarshalAs(UnmanagedType.LPUTF8Str)] string systemMessageJson);
            
            /// <summary>Sets the tools for this conversation config.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="toolsJson">The tools description in JSON array format.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_config_set_tools(IntPtr config, [MarshalAs(UnmanagedType.LPUTF8Str)] string toolsJson);
            
            /// <summary>Sets the initial messages for this conversation config.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="messagesJson">The initial messages in JSON array format.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_config_set_messages(IntPtr config, [MarshalAs(UnmanagedType.LPUTF8Str)] string messagesJson);
            
            /// <summary>Sets the extra context for the conversation preface.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="extraContextJson">A JSON string representing the extra context object.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_config_set_extra_context(IntPtr config, [MarshalAs(UnmanagedType.LPUTF8Str)] string extraContextJson);
            
            /// <summary>Sets whether to enable constrained decoding for this conversation config.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="enableConstrainedDecoding">Whether to enable constrained decoding.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_config_set_enable_constrained_decoding(IntPtr config, [MarshalAs(UnmanagedType.I1)] bool enableConstrainedDecoding);
            
            /// <summary>Sets whether to filter channel content from the KV cache.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="filterChannelContentFromKvCache">Whether to filter channel content.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_config_set_filter_channel_content_from_kv_cache(IntPtr config, [MarshalAs(UnmanagedType.I1)] bool filterChannelContentFromKvCache);
            
            /// <summary>Sets whether to stream tool call tokens.</summary>
            /// <param name="config">The config to modify.</param>
            /// <param name="streamToolCalls">Whether to stream tool call tokens.</param>
            /// <param name="channelName">The channel name to use for tool call tokens.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_config_set_stream_tool_calls(IntPtr config, [MarshalAs(UnmanagedType.I1)] bool streamToolCalls, [MarshalAs(UnmanagedType.LPUTF8Str)] string channelName);
        }
        
        public static class ConversationOptionalArgs
        {
            /// <summary>
            /// Creates a LiteRT LM Conversation Optional Args. The caller
            /// is responsible for destroying the optional args using
            /// <see cref="litert_lm_conversation_optional_args_delete"/>.
            /// </summary>
            /// <returns>A pointer to the created optional args, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_conversation_optional_args_create();

            /// <summary>Destroys a LiteRT LM Conversation Optional Args.</summary>
            /// <returns>The optional args to destroy.</returns>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_optional_args_delete(IntPtr optionalArgs);
            
            /// <summary>Sets the visual token budget for the conversation optional args.</summary>
            /// <param name="optionalArgs">The optional args to modify.</param>
            /// <param name="visualTokenBudget">The visual token budget.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_optional_args_set_visual_token_budget(IntPtr optionalArgs, int visualTokenBudget);

            /// <summary>Sets the maximum number of output tokens for the conversation optional args.</summary>
            /// <param name="optionalArgs">The optional args to modify.</param>
            /// <param name="maxOutputTokens">The maximum number of output tokens.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_optional_args_set_max_output_tokens(IntPtr optionalArgs, int maxOutputTokens);
        }
        
        public static class InputData
        {
            /// <summary>
            /// Creates a LiteRT LM Input Data. The caller is responsible for destroying
            /// the input data using <see cref="litert_lm_input_data_delete"/>.
            ///</summary>
            /// <param name="type">The type of the input data.</param>
            /// <param name="data">The data pointer. The data is copied internally.</param>
            /// <param name="size">The size of the data in bytes.</param>
            /// <returns>A pointer to the created input data, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_input_data_create(InputDataType type, IntPtr data, UIntPtr size);

            /// <summary>Destroys a LiteRT LM Input Data.</summary>
            /// <param name="inputData">The input data to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_input_data_delete(IntPtr inputData);
        }
        
        public static class EngineSettings
        {
            /// <summary>
            /// Creates LiteRT LM Engine Settings. The caller is responsible for destroying
            /// the settings using <see cref="litert_lm_engine_settings_delete"/>.
            /// </summary>
            /// <param name="modelPath">The path to the model file.</param>
            /// <param name="backendStr">The backend to use (e.g., "cpu", "gpu").</param>
            /// <param name="visionBackendStr">The vision backend to use, or <see langword="null"/> if not set.</param>
            /// <param name="audioBackendStr">The audio backend to use, or <see langword="null"/> if not set.</param>
            /// <returns>A pointer to the created settings, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_engine_settings_create([MarshalAs(UnmanagedType.LPUTF8Str)] string modelPath, [MarshalAs(UnmanagedType.LPUTF8Str)] string backendStr, [MarshalAs(UnmanagedType.LPUTF8Str)] string? visionBackendStr, [MarshalAs(UnmanagedType.LPUTF8Str)] string? audioBackendStr);
        
            /// <summary>
            /// Creates LiteRT LM Engine Settings from a raw file descriptor. The engine
            /// takes ownership of the file descriptor and will close it when done.
            /// The caller is responsible for destroying the settings using
            /// <see cref="litert_lm_engine_settings_delete"/>.
            /// </summary>
            /// <param name="fd">The file descriptor of the model.</param>
            /// <param name="backendStr">The backend to use (e.g., "cpu", "gpu").</param>
            /// <param name="visionBackendStr">The vision backend to use, or <see langword="null"/> if not set.</param>
            /// <param name="audioBackendStr">The audio backend to use, or <see langword="null"/> if not set.</param>
            /// <returns>A pointer to the created settings, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_engine_settings_create_from_raw_file_descriptor(int fd, [MarshalAs(UnmanagedType.LPUTF8Str)] string backendStr, [MarshalAs(UnmanagedType.LPUTF8Str)] string? visionBackendStr, [MarshalAs(UnmanagedType.LPUTF8Str)] string? audioBackendStr);
        
            /// <summary>Destroys LiteRT LM Engine Settings.</summary>
            /// <param name="settings">The settings to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_delete(IntPtr settings);
        
            /// <summary>Sets the maximum number of tokens for the engine.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="maxNumTokens">The maximum number of tokens.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_max_num_tokens(IntPtr settings, int maxNumTokens);
        
            /// <summary>Sets the number of threads for the CPU backend.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="numThreads">The number of threads.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_num_threads(IntPtr settings, int numThreads);
        
            /// <summary>Sets the number of threads for the audio CPU backend.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="numThreads">The number of threads.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_audio_num_threads(IntPtr settings, int numThreads);
        
            /// <summary>
            /// Sets whether the engine should load different sections of the litertlm file
            /// in parallel. Defaults to true.
            /// </summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="parallelFileSectionLoading">Whether to load in parallel.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_parallel_file_section_loading(IntPtr settings, [MarshalAs(UnmanagedType.I1)] bool parallelFileSectionLoading);
        
            /// <summary>
            /// Sets the maximum number of images for the engine.
            /// This is only used for the legacy implementation of the engine.
            /// </summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="maxNumImages">The maximum number of images.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_max_num_images(IntPtr settings, int maxNumImages);
        
            /// <summary>Sets the cache directory for the engine.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="cacheDir">The cache directory.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_cache_dir(IntPtr settings, [MarshalAs(UnmanagedType.LPUTF8Str)] string cacheDir);
        
            /// <summary>Sets the LiteRT dispatch library directory for NPU backend.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="libDir">The dispatch library directory.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_litert_dispatch_lib_dir(IntPtr settings, [MarshalAs(UnmanagedType.LPUTF8Str)] string libDir);
        
            /// <summary>Sets the activation data type.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="activationDataType">The activation data type.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_activation_data_type(IntPtr settings, [MarshalAs(UnmanagedType.I4)] ActivationDataType activationDataType);
        
            /// <summary>
            /// Sets the prefill chunk size for the engine. Only applicable for CPU backend
            /// with dynamic models.
            /// </summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="prefillChunkSize">The prefill chunk size.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_prefill_chunk_size(IntPtr settings, int prefillChunkSize);
        
            /// <summary>Enables benchmarking for the engine.</summary>
            /// <param name="settings">The engine settings.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_enable_benchmark(IntPtr settings);
        
            /// <summary>Sets the number of prefill tokens for benchmarking.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="numPrefillTokens">The number of prefill tokens.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_num_prefill_tokens(IntPtr settings, int numPrefillTokens);
        
            /// <summary>Sets the number of decode tokens for benchmarking.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="numDecodeTokens">The number of decode tokens.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_num_decode_tokens(IntPtr settings, int numDecodeTokens);
        
            /// <summary>Sets whether to enable speculative decoding.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="enableSpeculativeDecoding">Whether to enable speculative decoding.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_enable_speculative_decoding(IntPtr settings, [MarshalAs(UnmanagedType.I1)] bool enableSpeculativeDecoding);
        
            /// <summary>Sets the LoRA rank for the engine.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="loraRank">The LoRA rank.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_lora_rank(IntPtr settings, int loraRank);
        
            /// <summary>Sets the supported LoRA ranks for the engine.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="loraRanks">An array of supported LoRA ranks.</param>
            /// <param name="numRanks">The number of ranks in the array.</param>
            /// <returns>0 on success, non-zero on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_engine_settings_set_supported_lora_ranks(IntPtr settings, int[] loraRanks, UIntPtr numRanks);
        
            /// <summary>Sets the Audio LoRA rank for the engine.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="loraRank">The Audio LoRA rank.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_settings_set_audio_lora_rank(IntPtr settings, int loraRank);
        
            /// <summary>Sets the supported Audio LoRA ranks for the engine.</summary>
            /// <param name="settings">The engine settings.</param>
            /// <param name="loraRanks">An array of supported Audio LoRA ranks.</param>
            /// <param name="numRanks">The number of ranks in the array.</param>
            /// <returns>0 on success, non-zero on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_engine_settings_set_supported_audio_lora_ranks(IntPtr settings, int[] loraRanks, UIntPtr numRanks);
        }
        
        public static class Engine
        {
            /// <summary>
            /// Creates a LiteRT LM Engine from the given settings. The caller is responsible
            /// for destroying the engine using <see cref="litert_lm_engine_delete"/>.
            /// </summary>
            /// <param name="settings">The engine settings.</param>
            /// <returns>A pointer to the created engine, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_engine_create(IntPtr settings);

            /// <summary>Destroys a LiteRT LM Engine.</summary>
            /// <param name="engine">The engine to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_engine_delete(IntPtr engine);
            
            /// <summary>
            /// Creates a LiteRT LM Session. The caller is responsible for destroying the
            /// session using <see cref="Session.litert_lm_session_delete"/>.
            /// </summary>
            /// <param name="engine">The engine to create the session from.</param>
            /// <param name="config">The session config of the session. If <see cref="IntPtr.Zero"/>, use the default session config.</param>
            /// <returns>A pointer to the created session, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_engine_create_session(IntPtr engine, IntPtr config);
            
            /// <summary>Tokenizes text using the engine's tokenizer.</summary>
            /// <param name="engine">The engine instance.</param>
            /// <param name="text">The UTF-8 string to tokenize.</param>
            /// <returns>
            /// A pointer to the tokenize result, or <see cref="IntPtr.Zero"/> on failure.
            /// The caller is responsible for deleting the result using <see cref="TokenizeResult.litert_lm_tokenize_result_delete"/>.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_engine_tokenize(IntPtr engine, [MarshalAs(UnmanagedType.LPUTF8Str)] string text);
            
            /// <summary>Detokenizes token ids using the engine's tokenizer.</summary>
            /// <param name="engine">The engine instance.</param>
            /// <param name="tokens">An array of token ids to detokenize.</param>
            /// <param name="numTokens">The number of token ids in the array.</param>
            /// <returns>
            /// A pointer to the detokenize result, or <see cref="IntPtr.Zero"/> on failure.
            /// The caller is responsible for deleting the result using
            /// <see cref="DetokenizeResult.litert_lm_detokenize_result_delete"/>.
            /// </returns>
            [DllImport(LibLitertLM)] 
            public static extern IntPtr litert_lm_engine_detokenize(IntPtr engine, int[] tokens, UIntPtr numTokens);
            
            /// <summary>Returns the configured start token (BOS), if any.</summary>
            /// <param name="engine">The engine instance.</param>
            /// <returns>
            /// A pointer to the start token, or <see cref="IntPtr.Zero"/> if none configured.
            /// The caller is responsible for deleting the result using
            /// <see cref="TokenUnion.litert_lm_token_union_delete"/>.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_engine_get_start_token(IntPtr engine);

            /// <summary>Returns the configured stop tokens (EOS).</summary>
            /// <param name="engine">The engine instance.</param>
            /// <returns>
            /// A pointer to the stop tokens collection, or <see cref="IntPtr.Zero"/> if none configured.
            /// The caller is responsible for deleting the result using
            /// <see cref="TokenUnions.litert_lm_token_unions_delete"/>.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_engine_get_stop_tokens(IntPtr engine);
        }
        
        public static class Session
        {
            /// <summary>Destroys a LiteRT LM Session.</summary>
            /// <param name="session">The session to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_session_delete(IntPtr session);
            
            /// <summary>Cancels the current processing in the session.</summary>
            /// <param name="session">The session to cancel processing on.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_session_cancel_process(IntPtr session);
            
            /// <summary>
            /// Adds the input prompt/query to the model for starting the prefilling
            /// process. This is a blocking call and the function will return when the
            /// prefill process is done.
            /// </summary>
            /// <param name="session">The session to use.</param>
            /// <param name="inputs">An array of InputData structs representing the multimodal input.</param>
            /// <param name="numInputs">The number of InputData structs in the array.</param>
            /// <returns>0 on success, non-zero on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_session_run_prefill(IntPtr session, IntPtr[] inputs, UIntPtr numInputs);
            
            /// <summary>
            /// Starts the decoding process for the model to predict the response based
            /// on the input prompt/query added after using <see cref="litert_lm_session_run_prefill"/>.
            /// This is a blocking call and the function will return when the decoding
            /// process is done.
            /// </summary>
            /// <param name="session">The session to use.</param>
            /// <returns>
            /// A pointer to the responses, or <see cref="IntPtr.Zero"/> on failure. The caller is
            /// responsible for deleting the responses using <see cref="Responses.litert_lm_responses_delete"/>.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_session_run_decode(IntPtr session);
            
            /// <summary>Scores the target text after the prefill process is done.</summary>
            /// <param name="session">The session to use.</param>
            /// <param name="targetText">An array of target text strings to score.</param>
            /// <param name="numTargets">The number of strings in the targetText array.</param>
            /// <param name="storeTokenLengths">Whether to store the token lengths of the target texts in the responses.</param>
            /// <returns>
            /// A pointer to the responses, or <see cref="IntPtr.Zero"/> on failure. The caller is
            /// responsible for deleting the responses using <see cref="Responses.litert_lm_responses_delete"/>.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_session_run_text_scoring(IntPtr session, string[] targetText, UIntPtr numTargets, [MarshalAs(UnmanagedType.I1)] bool storeTokenLengths);
            
            /// <summary>Generates content from the input prompt.</summary>
            /// <param name="session">The session to use for generation.</param>
            /// <param name="inputs">An array of LiteRtLmInputData structs representing the multimodal input.</param>
            /// <param name="numInputs">The number of LiteRtLmInputData structs in the array.</param>
            /// <returns>
            /// A pointer to the responses, or <see cref="IntPtr.Zero"/> on failure. The caller is
            /// responsible for deleting the responses using <see cref="Responses.litert_lm_responses_delete"/>.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_session_generate_content(IntPtr session, IntPtr[] inputs, UIntPtr numInputs);
            
            /// <summary>
            /// Retrieves the benchmark information from the session. The caller is
            /// responsible for destroying the benchmark info using
            /// <see cref="BenchmarkInfo.litert_lm_benchmark_info_delete"/>.
            /// </summary>
            /// <param name="session">The session to get the benchmark info from.</param>
            /// <returns>A pointer to the benchmark info, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_session_get_benchmark_info(IntPtr session);

            /// <summary>
            /// Starts the decoding process for the model to predict the response based
            /// on the input prompt/query added after using <see cref="litert_lm_session_run_prefill"/>.
            /// This is a non-blocking call that will stream responses via a callback.
            /// </summary>
            /// <param name="session">The session to use.</param>
            /// <param name="callback">The callback function to receive response chunks.</param>
            /// <param name="callbackData">A pointer to user data that will be passed to the callback.</param>
            /// <returns>0 on success, non-zero on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_session_run_decode_async(IntPtr session, StreamCallback callback, IntPtr callbackData);

            /// <summary>
            /// Generates content from the input prompt and streams the response via a
            /// callback. This is a non-blocking call that will invoke the callback from a
            /// background thread for each chunk.
            /// </summary>
            /// <param name="session">The session to use for generation.</param>
            /// <param name="inputs">An array of LiteRtLmInputData structs representing the multimodal input.</param>
            /// <param name="numInputs">The number of LiteRtLmInputData structs in the array.</param>
            /// <param name="callback">The callback function to receive response chunks.</param>
            /// <param name="callbackData">A pointer to user data that will be passed to the callback.</param>
            /// <returns>0 on success, non-zero on failure to start the stream.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_session_generate_content_stream(IntPtr session, IntPtr[] inputs, UIntPtr numInputs, StreamCallback callback, IntPtr callbackData);
        }
        
        public static class Responses
        {
            /// <summary>Destroys a LiteRT LM Responses object.</summary>
            /// <param name="responses">The responses to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_responses_delete(IntPtr responses);
            
            /// <summary>Returns the number of response candidates.</summary>
            /// <param name="responses">The responses object.</param>
            /// <returns>The number of candidates.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_responses_get_num_candidates(IntPtr responses);
            
            /// <summary>Returns the response text at a given index.</summary>
            /// <param name="responses">The responses object.</param>
            /// <param name="index">The index of the response.</param>
            /// <returns>The response text. Returns <see langword="null"/> if index is out of bounds.</returns>
            [DllImport(LibLitertLM)]
            [return: MarshalAs(UnmanagedType.LPUTF8Str)]
            public static extern string? litert_lm_responses_get_response_text_at(IntPtr responses, int index);
            
            /// <summary>Returns whether the response contains a score at the given index.</summary>
            /// <param name="responses">The responses object.</param>
            /// <param name="index">The index of the response.</param>
            /// <returns><see langword="true"/> if the score is available at the given index, <see langword="false"/> otherwise.</returns>
            [DllImport(LibLitertLM)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool litert_lm_responses_has_score_at(IntPtr responses, int index);
            
            /// <summary>Returns the score at a given index.</summary>
            /// <param name="responses">The responses object.</param>
            /// <param name="index">The index of the response.</param>
            /// <returns>The score. Returns 0.0f if index is out of bounds or no score is present.</returns>
            [DllImport(LibLitertLM)]
            public static extern float litert_lm_responses_get_score_at(IntPtr responses, int index);
            
            /// <summary>Returns whether the response contains a token length at the given index.</summary>
            /// <param name="responses">The responses object.</param>
            /// <param name="index">The index of the response.</param>
            /// <returns><see langword="true"/> if the token length is available at the given index, <see langword="false"/> otherwise.</returns>
            [DllImport(LibLitertLM)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool litert_lm_responses_has_token_length_at(IntPtr responses, int index);
            
            /// <summary>Returns the token length at a given index.</summary>
            /// <param name="responses">The responses object.</param>
            /// <param name="index">The index of the response.</param>
            /// <returns>
            /// The token length. Returns 0 if index is out of bounds or no token
            /// length is present.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_responses_get_token_length_at(IntPtr responses, int index);
            
            /// <summary>Returns whether the response contains token scores at the given index.</summary>
            /// <param name="responses">The responses object.</param>
            /// <param name="index">The index of the response.</param>
            /// <returns><see langword="true"/> if token scores are available at the given index, <see langword="false"/> otherwise.</returns>
            [DllImport(LibLitertLM)]
            [return: MarshalAs(UnmanagedType.I1)]
            public static extern bool litert_lm_responses_has_token_scores_at(IntPtr responses, int index);
            
            /// <summary>Returns the number of tokens for which scores are present at a given index.</summary>
            /// <param name="responses">The responses object.</param>
            /// <param name="index">The index of the response.</param>
            /// <returns>
            /// The number of token scores. Returns 0 if index is out of bounds or no
            /// token scores are present.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_responses_get_num_token_scores_at(IntPtr responses, int index);
            
            /// <summary>Returns the token scores at a given index.</summary>
            /// <param name="responses">The responses object.</param>
            /// <param name="index">The index of the response.</param>
            /// <returns>
            /// A pointer to the internal array of token scores. Returns <see cref="IntPtr.Zero"/> if
            /// index is out of bounds or no token scores are present.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_responses_get_token_scores_at(IntPtr responses, int index);
        }
        
        public static class BenchmarkInfo
        {
            /// <summary>Destroys a LiteRT LM Benchmark Info object.</summary>
            /// <param name="benchmarkInfo">The benchmark info to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_benchmark_info_delete(IntPtr benchmarkInfo);
            
            /// <summary>Returns the time to the first token in seconds.</summary>
            /// <remarks>
            /// Note that the first time to token doesn't include the time for
            /// initialization. It is the sum of the prefill time for the first turn and
            /// the time spent for decoding the first token.
            /// </remarks>
            /// <param name="benchmarkInfo">The benchmark info object.</param>
            /// <returns>The time to the first token in seconds.</returns>
            [DllImport(LibLitertLM)]
            public static extern double litert_lm_benchmark_info_get_time_to_first_token(IntPtr benchmarkInfo);
            
            /// <summary>Returns the total initialization time in seconds.</summary>
            /// <param name="benchmarkInfo">The benchmark info object.</param>
            /// <returns>The total initialization time in seconds.</returns>
            [DllImport(LibLitertLM)]
            public static extern double litert_lm_benchmark_info_get_total_init_time_in_second(IntPtr benchmarkInfo);
            
            /// <summary>Returns the number of prefill turns.</summary>
            /// <param name="benchmarkInfo">The benchmark info object.</param>
            /// <returns>The number of prefill turns.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_benchmark_info_get_num_prefill_turns(IntPtr benchmarkInfo);
            
            /// <summary>Returns the number of decode turns.</summary>
            /// <param name="benchmarkInfo">The benchmark info object.</param>
            /// <returns>The number of decode turns.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_benchmark_info_get_num_decode_turns(IntPtr benchmarkInfo);
            
            /// <summary>Returns the prefill token count at a given turn index.</summary>
            /// <param name="benchmarkInfo">The benchmark info object.</param>
            /// <param name="index">The index of the prefill turn.</param>
            /// <returns>The prefill token count.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_benchmark_info_get_prefill_token_count_at(IntPtr benchmarkInfo, int index);
            
            /// <summary>Returns the decode token count at a given turn index.</summary>
            /// <param name="benchmarkInfo">The benchmark info object.</param>
            /// <param name="index">The index of the decode turn.</param>
            /// <returns>The decode token count.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_benchmark_info_get_decode_token_count_at(IntPtr benchmarkInfo, int index);
            
            /// <summary>Returns the prefill tokens per second at a given turn index.</summary>
            /// <param name="benchmarkInfo">The benchmark info object.</param>
            /// <param name="index">The index of the prefill turn.</param>
            /// <returns>The prefill tokens per second.</returns>
            [DllImport(LibLitertLM)]
            public static extern double litert_lm_benchmark_info_get_prefill_tokens_per_sec_at(IntPtr benchmarkInfo, int index);
            
            /// <summary>Returns the decode tokens per second at a given turn index.</summary>
            /// <param name="benchmarkInfo">The benchmark info object.</param>
            /// <param name="index">The index of the decode turn.</param>
            /// <returns>The decode tokens per second.</returns>
            [DllImport(LibLitertLM)]
            public static extern double litert_lm_benchmark_info_get_decode_tokens_per_sec_at(IntPtr benchmarkInfo, int index);
        }
        
        public static class Conversation
        {
            /// <summary>
            /// Creates a LiteRT LM Conversation. The caller is responsible for destroying
            /// the conversation using <see cref="litert_lm_conversation_delete"/>.
            /// </summary>
            /// <param name="engine">The engine to create the conversation from.</param>
            /// <param name="config">The conversation config to use. If <see cref="IntPtr.Zero"/>, the default config will be used.</param>
            /// <returns>A pointer to the created conversation, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_conversation_create(IntPtr engine, IntPtr config);

            /// <summary>Destroys a LiteRT LM Conversation.</summary>
            /// <param name="conversation">The conversation to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_delete(IntPtr conversation);

            /// <summary>
            /// Clones a LiteRT LM Conversation, duplicating its prefilled state.
            /// The caller is responsible for destroying the cloned conversation using
            /// <see cref="litert_lm_conversation_delete"/>.
            /// </summary>
            /// <param name="conversation">The conversation to clone.</param>
            /// <returns>A pointer to the cloned conversation, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_conversation_clone(IntPtr conversation);

            /// <summary>
            /// Sends a message to the conversation and returns the response.
            /// This is a blocking call.
            /// </summary>
            /// <param name="conversation">The conversation to use.</param>
            /// <param name="messageJson">A JSON string representing the message to send.</param>
            /// <param name="extraContext">A JSON string representing the extra context to use.</param>
            /// <param name="optionalArgs">A pointer to the optional arguments to use.</param>
            /// <returns>
            /// A pointer to the JSON response, or <see cref="IntPtr.Zero"/> on failure.
            /// The caller is responsible for deleting the response using <see cref="JsonResponse.litert_lm_json_response_delete"/>.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_conversation_send_message(IntPtr conversation, [MarshalAs(UnmanagedType.LPUTF8Str)] string messageJson, [MarshalAs(UnmanagedType.LPUTF8Str)] string extraContext, IntPtr optionalArgs);
            
            /// <summary>
            /// Sends a message to the conversation and streams the response via a
            /// callback. This is a non-blocking call that will invoke the callback from a
            /// background thread for each chunk.
            /// </summary>
            /// <param name="conversation">The conversation to use.</param>
            /// <param name="messageJson">A JSON string representing the message to send.</param>
            /// <param name="extraContext">A JSON string representing the extra context to use.</param>
            /// <param name="optionalArgs">A pointer to the optional arguments to use.</param>
            /// <param name="callback">The callback function to receive response chunks.</param>
            /// <param name="callbackData">A pointer to user data that will be passed to the callback.</param>
            /// <returns>0 on success, non-zero on failure to start the stream.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_conversation_send_message_stream(IntPtr conversation, [MarshalAs(UnmanagedType.LPUTF8Str)] string messageJson, [MarshalAs(UnmanagedType.LPUTF8Str)] string extraContext, IntPtr optionalArgs, StreamCallback callback, IntPtr callbackData);

            /// <summary>
            /// Renders the message into a string according to the template.
            /// </summary>
            /// <remarks>
            /// This function does not need to be called for actual message sending, as the
            /// <see cref="litert_lm_conversation_send_message"/> and <see cref="litert_lm_conversation_send_message_stream"/>
            /// functions will handle rendering internally.
            /// </remarks>
            /// <param name="conversation">The conversation instance.</param>
            /// <param name="messageJson">A JSON string representing the message to render.</param>
            /// <returns>A pointer to the rendered string, or <see langword="null"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            [return: MarshalAs(UnmanagedType.LPUTF8Str)]
            public static extern string? litert_lm_conversation_render_message_to_string(IntPtr conversation, [MarshalAs(UnmanagedType.LPUTF8Str)] string messageJson);

            /// <summary>
            /// Renders the preface into a string according to the template.
            /// </summary>
            /// <param name="conversation">The conversation instance.</param>
            /// <returns>A pointer to the rendered string, or langword="null"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            [return: MarshalAs(UnmanagedType.LPUTF8Str)]
            public static extern string? litert_lm_conversation_render_preface_to_string(IntPtr conversation);

            /// <summary>Cancels the ongoing inference process, for asynchronous inference.</summary>
            /// <param name="conversation">The conversation to cancel the inference for.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_conversation_cancel_process(IntPtr conversation);

            /// <summary>
            /// Retrieves the benchmark information from the conversation. The caller is
            /// responsible for destroying the benchmark info using
            /// <see cref="BenchmarkInfo.litert_lm_benchmark_info_delete"/>.
            /// </summary>
            /// <param name="conversation">The conversation to get the benchmark info from.</param>
            /// <returns>A pointer to the benchmark info, or <see cref="IntPtr.Zero"/> on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_conversation_get_benchmark_info(IntPtr conversation);

            /// <summary>Gets the number of tokens in the conversation KV Cache (prefill + decode).</summary>
            /// <returns>The number of tokens, or a negative value on failure.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_conversation_get_token_count(IntPtr conversation);
        }
        
        public static class JsonResponse
        {
            /// <summary>Destroys a LiteRT LM Json Response object.</summary>
            /// <param name="response">The response to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_json_response_delete(IntPtr response);

            /// <summary>Returns the JSON response string from a response object.</summary>
            /// <param name="response">The response object.</param>
            /// <returns>The response JSON string. Returns <see langword="null"/> if <paramref name="response"/> is <see cref="IntPtr.Zero"/>.</returns>
            [DllImport(LibLitertLM)]
            [return: MarshalAs(UnmanagedType.LPUTF8Str)]
            public static extern string? litert_lm_json_response_get_string(IntPtr response);
        }
        
        public static class TokenizeResult
        {
            /// <summary>Destroys a LiteRT LM Tokenize Result.</summary>
            /// <param name="result">The tokenize result to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_tokenize_result_delete(IntPtr result);

            /// <summary>Returns the token ids from a tokenize result.</summary>
            /// <param name="result">The tokenize result.</param>
            /// <returns>
            /// A pointer to the internal array of token ids. The returned pointer
            /// is valid only for the lifetime of the <paramref name="result"/> object.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_tokenize_result_get_tokens(IntPtr result);

            /// <summary>Returns the number of token ids from a tokenize result.</summary>
            /// <param name="result">The tokenize result.</param>
            /// <returns>The number of token ids.</returns>
            [DllImport(LibLitertLM)]
            public static extern UIntPtr litert_lm_tokenize_result_get_num_tokens(IntPtr result);
        }
        
        public static class DetokenizeResult
        {
            /// <summary>Destroys a LiteRT LM Detokenize Result.</summary>
            /// <param name="result">The detokenize result to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_detokenize_result_delete(IntPtr result);

            /// <summary>Returns the string from a detokenize result.</summary>
            /// <param name="result">The detokenize result.</param>
            /// <returns>The detokenized string.</returns>
            [DllImport(LibLitertLM)]
            [return: MarshalAs(UnmanagedType.LPUTF8Str)]
            public static extern string litert_lm_detokenize_result_get_string(IntPtr result);
        }
        
        public static class TokenUnion
        {
            /// <summary>Destroys a LiteRT LM Token Union.</summary>
            /// <param name="tokenUnion">The token union to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_token_union_delete(IntPtr tokenUnion);

            /// <summary>Returns the type of the token union.</summary>
            /// <param name="tokenUnion">The token union.</param>
            /// <returns>The type of the token union.</returns>
            [DllImport(LibLitertLM)]
            public static extern TokenUnionType litert_lm_token_union_get_type(IntPtr tokenUnion);

            /// <summary>Returns the string value from a token union.</summary>
            /// <param name="tokenUnion">The token union.</param>
            /// <returns>The string value, or <see langword="null"/> if the type is not <see cref="TokenUnionType.String"/>.</returns>
            [DllImport(LibLitertLM)]
            [return: MarshalAs(UnmanagedType.LPUTF8Str)]
            public static extern string? litert_lm_token_union_get_string(IntPtr tokenUnion);

            /// <summary>Returns the token ids from a token union.</summary>
            /// <param name="tokenUnion">The token union.</param>
            /// <param name="outTokens">A pointer to receive the internal array of token ids. The received pointer is valid only for the lifetime of the TokenUnion object.</param>
            /// <param name="outNumTokens">A pointer to receive the number of token ids.</param>
            /// <returns>0 on success, non-zero if the type is not <see cref="TokenUnionType.Ids"/>.</returns>
            [DllImport(LibLitertLM)]
            public static extern int litert_lm_token_union_get_ids(IntPtr tokenUnion, ref IntPtr outTokens, ref UIntPtr outNumTokens);
        }
        
        public static class TokenUnions
        {
            /// <summary>Destroys a LiteRT LM Token Unions object.</summary>
            /// <param name="tokens">The token unions object to destroy.</param>
            [DllImport(LibLitertLM)]
            public static extern void litert_lm_token_unions_delete(IntPtr tokens);

            /// <summary>Returns the number of token unions in the collection.</summary>
            /// <param name="tokens">The token unions object.</param>
            /// <returns>The number of token unions.</returns>
            [DllImport(LibLitertLM)]
            public static extern UIntPtr litert_lm_token_unions_get_num_tokens(IntPtr tokens);

            /// <summary>Returns the token union at a given index from a collection.</summary>
            /// <param name="tokens">The token unions collection.</param>
            /// <param name="index">The index of the token union.</param>
            /// <returns>
            /// A pointer to the token union at the given index, or <see cref="IntPtr.Zero"/> if the index
            /// is out of bounds. The caller is responsible for deleting the result using
            /// <see cref="TokenUnion.litert_lm_token_union_delete"/>.
            /// </returns>
            [DllImport(LibLitertLM)]
            public static extern IntPtr litert_lm_token_unions_get_token_at(IntPtr tokens, UIntPtr index);
        }
    }
}