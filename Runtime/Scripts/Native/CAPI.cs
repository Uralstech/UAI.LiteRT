#nullable enable
using System;
using System.Runtime.InteropServices;

namespace Uralstech.UAI.LiteRTLM
{
    public static class CAPI
    {
        public const string LibLiteRTLM = "litert-lm";

        // Represents the type of a TokenUnion.
        public enum LiteRtLmTokenUnionType : int
        {
            kLiteRtLmTokenUnionTypeString = 0,
            kLiteRtLmTokenUnionTypeIds = 1,
        }

        // Represents the type of sampler.
        public enum LiteRtLmSamplerType : int
        {
            // Probabilistically pick among the top k tokens.
            kLiteRtLmSamplerTypeTopK = 1,

            // Probabilistically pick among the tokens such that the sum is greater
            // than or equal to p tokens after first performing top-k sampling.
            kLiteRtLmSamplerTypeTopP = 2,

            // Pick the token with maximum logit (i.e., argmax).
            kLiteRtLmSamplerTypeGreedy = 3,
        }

        // Creates LiteRT LM Sampler Parameters with a specific sampler type.
        // The caller is responsible for destroying the parameters using
        // `litert_lm_sampler_params_delete`.
        //
        // @param type The sampler type to use.
        // @return A pointer to the created parameters, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_sampler_params_create(
            LiteRtLmSamplerType type);

        // Destroys LiteRT LM Sampler Parameters.
        //
        // @param params The parameters to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_sampler_params_delete(IntPtr @params);

        // Sets the top-k value.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_sampler_params_set_top_k(IntPtr @params,
            int top_k);

        // Sets the top-p value.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_sampler_params_set_top_p(IntPtr @params,
            float top_p);

        // Sets the temperature.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_sampler_params_set_temperature(IntPtr @params,
            float temperature);

        // Sets the seed.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_sampler_params_set_seed(IntPtr @params,
            int seed);

        // Creates a LiteRT LM Session Config.
        // The caller is responsible for destroying the config using
        // `litert_lm_session_config_delete`.
        // @return A pointer to the created config, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_session_config_create();

        // Sets the maximum number of output tokens per decode step for this session.
        // @param config The config to modify.
        // @param max_output_tokens The maximum number of output tokens.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_session_config_set_max_output_tokens(
            IntPtr config, int max_output_tokens);

        // Sets whether to apply prompt template for this session.
        // @param config The config to modify.
        // @param apply_prompt_template Whether to apply prompt template.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_session_config_set_apply_prompt_template(
            IntPtr config, [MarshalAs(UnmanagedType.I1)] bool apply_prompt_template);

        // Sets the sampler parameters for this session config.
        // @param config The config to modify.
        // @param sampler_params The sampler parameters to use.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_session_config_set_sampler_params(
            IntPtr config, IntPtr sampler_params);

        // Destroys a LiteRT LM Session Config.
        // @param config The config to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_session_config_delete(IntPtr config);

        // Sets the path to the LoRA weights file.
        // @param config The config to modify.
        // @param lora_path The path to the text LoRA weights file.
        // @return 0 on success, non-zero on failure.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_session_config_set_lora_path(IntPtr config,
            string? lora_path);

        // Sets the path to the Audio LoRA weights file.
        // @param config The config to modify.
        // @param audio_lora_path The path to the audio LoRA weights file.
        // @return 0 on success, non-zero on failure.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_session_config_set_audio_lora_path(IntPtr config,
            string? audio_lora_path);

        // Creates a LiteRT LM Conversation Config.
        // The caller is responsible for destroying the config using
        // `litert_lm_conversation_config_delete`.
        // @return A pointer to the created config, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_conversation_config_create();

        // Sets the session config for this conversation config.
        // @param config The config to modify.
        // @param session_config The session config to use.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_config_set_session_config(
            IntPtr config,
            IntPtr session_config);

        // Sets the system message for this conversation config.
        // @param config The config to modify.
        // @param system_message_json The system message in JSON format.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_config_set_system_message(
            IntPtr config, string? system_message_json);

        // Sets the tools for this conversation config.
        // @param config The config to modify.
        // @param tools_json The tools description in JSON array format.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_config_set_tools(IntPtr config,
            string? tools_json);

        // Sets the initial messages for this conversation config.
        // @param config The config to modify.
        // @param messages_json The initial messages in JSON array format.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_config_set_messages(
            IntPtr config, string? messages_json);

        // Sets the extra context for the conversation preface.
        // @param config The config to modify.
        // @param extra_context_json A JSON string representing the extra context
        // object.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_config_set_extra_context(
            IntPtr config, string? extra_context_json);

        // Sets whether to enable constrained decoding for this conversation config.
        // @param config The config to modify.
        // @param enable_constrained_decoding Whether to enable constrained decoding.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_config_set_enable_constrained_decoding(
            IntPtr config, [MarshalAs(UnmanagedType.I1)] bool enable_constrained_decoding);

        // Sets whether to filter channel content from the KV cache.
        // @param config The config to modify.
        // @param filter_channel_content_from_kv_cache Whether to filter channel
        // content.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_config_set_filter_channel_content_from_kv_cache(
            IntPtr config,
            [MarshalAs(UnmanagedType.I1)] bool filter_channel_content_from_kv_cache);

        // Sets whether to stream tool call tokens.
        // @param config The config to modify.
        // @param stream_tool_calls Whether to stream tool call tokens.
        // @param channel_name The channel name to use for tool call tokens.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_config_set_stream_tool_calls(
            IntPtr config, [MarshalAs(UnmanagedType.I1)] bool stream_tool_calls,
            string? channel_name);

        // Destroys a LiteRT LM Conversation Config.
        // @param config The config to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_config_delete(IntPtr config);

        // Creates a LiteRT LM Conversation Optional Args. The caller is responsible
        // for destroying the optional args using
        // `litert_lm_conversation_optional_args_delete`.
        // @return A pointer to the created optional args, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_conversation_optional_args_create();

        // Destroys a LiteRT LM Conversation Optional Args.
        // @param optional_args The optional args to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_optional_args_delete(
            IntPtr optional_args);

        // Sets the visual token budget for the conversation optional args.
        // @param optional_args The optional args to modify.
        // @param visual_token_budget The visual token budget.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_optional_args_set_visual_token_budget(
            IntPtr optional_args, int visual_token_budget);

        // Sets the maximum number of output tokens for the conversation optional args.
        // @param optional_args The optional args to modify.
        // @param max_output_tokens The maximum number of output tokens.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_optional_args_set_max_output_tokens(
            IntPtr optional_args, int max_output_tokens);

        // Sets the minimum log level for the LiteRT LM library.
        // Log levels are: 0=VERBOSE, 1=DEBUG, 2=INFO, 3=WARNING, 4=ERROR, 5=FATAL,
        // 1000=SILENT.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_set_min_log_level(int level);

        // Represents the type of input data.
        public enum LiteRtLmInputDataType : int
        {
            kLiteRtLmInputDataTypeText,
            kLiteRtLmInputDataTypeImage,
            kLiteRtLmInputDataTypeImageEnd,
            kLiteRtLmInputDataTypeAudio,
            kLiteRtLmInputDataTypeAudioEnd,
        }

        // Creates a LiteRT LM Input Data. The caller is responsible for destroying
        // the input data using `litert_lm_input_data_delete`.
        //
        // @param type The type of the input data.
        // @param data The data pointer. For kLiteRtLmInputDataTypeText, it's a UTF-8
        // string.
        //             For image/audio types, it's a pointer to the raw bytes.
        //             The data is copied internally.
        // @param size The size of the data in bytes.
        // @return A pointer to the created input data, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_input_data_create(LiteRtLmInputDataType type,
            IntPtr data, nuint size);

        // Destroys a LiteRT LM Input Data.
        //
        // @param input_data The input data to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_input_data_delete(IntPtr input_data);

        // Creates LiteRT LM Engine Settings. The caller is responsible for destroying
        // the settings using `litert_lm_engine_settings_delete`.
        //
        // @param model_path The path to the model file.
        // @param backend_str The backend to use (e.g., "cpu", "gpu").
        // @param vision_backend_str The vision backend to use, or NULL if not set.
        // @param audio_backend_str The audio backend to use, or NULL if not set.
        // @return A pointer to the created settings, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_engine_settings_create(
            string? model_path, string? backend_str,
            string? vision_backend_str, string? audio_backend_str);

        // Creates LiteRT LM Engine Settings from a raw file descriptor. The engine
        // takes ownership of the file descriptor and will close it when done.
        // The caller is responsible for destroying the settings using
        // `litert_lm_engine_settings_delete`.
        //
        // @param fd The file descriptor of the model.
        // @param backend_str The backend to use (e.g., "cpu", "gpu").
        // @param vision_backend_str The vision backend to use, or NULL if not set.
        // @param audio_backend_str The audio backend to use, or NULL if not set.
        // @return A pointer to the created settings, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_engine_settings_create_from_raw_file_descriptor(
            int fd, string? backend_str, string? vision_backend_str,
            string? audio_backend_str);

        // Destroys LiteRT LM Engine Settings.
        //
        // @param settings The settings to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_delete(IntPtr settings);

        // Sets the maximum number of tokens for the engine.
        //
        // @param settings The engine settings.
        // @param max_num_tokens The maximum number of tokens.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_max_num_tokens(
            IntPtr settings, int max_num_tokens);

        // Sets the number of threads for the CPU backend.
        //
        // @param settings The engine settings.
        // @param num_threads The number of threads.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_num_threads(IntPtr settings,
            int num_threads);

        // Sets the number of threads for the audio CPU backend.
        //
        // @param settings The engine settings.
        // @param num_threads The number of threads.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_audio_num_threads(
            IntPtr settings, int num_threads);

        // Sets whether the engine should load different sections of the litertlm file
        // in parallel. Defaults to true.
        //
        // @param settings The engine settings.
        // @param parallel_file_section_loading Whether to load in parallel.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_parallel_file_section_loading(
            IntPtr settings, [MarshalAs(UnmanagedType.I1)] bool parallel_file_section_loading);

        // Sets the maximum number of images for the engine.
        //
        // This is only used for the legacy implementation of the engine.
        //
        // @param settings The engine settings.
        // @param max_num_images The maximum number of images.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_max_num_images(
            IntPtr settings, int max_num_images);

        // Sets the cache directory for the engine.
        //
        // @param settings The engine settings.
        // @param cache_dir The cache directory.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_cache_dir(IntPtr settings,
            string? cache_dir);

        // Sets the LiteRT dispatch library directory for NPU backend.
        //
        // @param settings The engine settings.
        // @param lib_dir The dispatch library directory.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_litert_dispatch_lib_dir(
            IntPtr settings, string? lib_dir);

        // Sets the activation data type.
        //
        // @param settings The engine settings.
        // @param activation_data_type_int The activation data type. See
        // `ActivationDataType` in executor_settings_base.h for the possible values
        // (e.g., 0 for F32, 1 for F16, 2 for I16, 3 for I8).
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_activation_data_type(
            IntPtr settings, int activation_data_type_int);

        // Sets the prefill chunk size for the engine. Only applicable for CPU backend
        // with dynamic models.
        //
        // @param settings The engine settings.
        // @param prefill_chunk_size The prefill chunk size.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_prefill_chunk_size(
            IntPtr settings, int prefill_chunk_size);

        // Enables benchmarking for the engine.
        //
        // @param settings The engine settings.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_enable_benchmark(
            IntPtr settings);

        // Sets the number of prefill tokens for benchmarking.
        //
        // @param settings The engine settings.
        // @param num_prefill_tokens The number of prefill tokens.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_num_prefill_tokens(
            IntPtr settings, int num_prefill_tokens);

        // Sets the number of decode tokens for benchmarking.
        //
        // @param settings The engine settings.
        // @param num_decode_tokens The number of decode tokens.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_num_decode_tokens(
            IntPtr settings, int num_decode_tokens);

        // Sets whether to enable speculative decoding.
        //
        // @param settings The engine settings.
        // @param enable_speculative_decoding Whether to enable speculative decoding.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_enable_speculative_decoding(
            IntPtr settings, [MarshalAs(UnmanagedType.I1)] bool enable_speculative_decoding);

        // Sets the LoRA rank for the engine.
        //
        // @param settings The engine settings.
        // @param lora_rank The LoRA rank.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_lora_rank(IntPtr settings,
            int lora_rank);

        // Sets the supported LoRA ranks for the engine.
        //
        // @param settings The engine settings.
        // @param lora_ranks An array of supported LoRA ranks.
        // @param num_ranks The number of ranks in the array.
        // @return 0 on success, non-zero on failure.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_engine_settings_set_supported_lora_ranks(
            IntPtr settings, IntPtr lora_ranks, nuint num_ranks);

        // Sets the Audio LoRA rank for the engine.
        //
        // @param settings The engine settings.
        // @param lora_rank The Audio LoRA rank.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_settings_set_audio_lora_rank(
            IntPtr settings, int lora_rank);

        // Sets the supported Audio LoRA ranks for the engine.
        //
        // @param settings The engine settings.
        // @param lora_ranks An array of supported Audio LoRA ranks.
        // @param num_ranks The number of ranks in the array.
        // @return 0 on success, non-zero on failure.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_engine_settings_set_supported_audio_lora_ranks(
            IntPtr settings, IntPtr lora_ranks, nuint num_ranks);

        // Creates a LiteRT LM Engine from the given settings. The caller is responsible
        // for destroying the engine using `litert_lm_engine_delete`.
        //
        // @param settings The engine settings.
        // @return A pointer to the created engine, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_engine_create(IntPtr settings);

        // Destroys a LiteRT LM Engine.
        //
        // @param engine The engine to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_engine_delete(IntPtr engine);

        // Creates a LiteRT LM Session. The caller is responsible for destroying the
        // session using `litert_lm_session_delete`.
        //
        // @param engine The engine to create the session from.
        // @param config The session config of the session. If NULL, use the default
        // session config.
        // @return A pointer to the created session, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_engine_create_session(IntPtr engine,
            IntPtr config);

        // Destroys a LiteRT LM Session.
        //
        // @param session The session to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_session_delete(IntPtr session);

        // Cancels the current processing in the session.
        //
        // @param session The session to cancel processing on.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_session_cancel_process(IntPtr session);

        // Adds the input prompt/query to the model for starting the prefilling
        // process. This is a blocking call and the function will return when the
        // prefill process is done.
        //
        // @param session The session to use.
        // @param inputs An array of InputData structs representing the multimodal
        //   input.
        // @param num_inputs The number of InputData structs in the array.
        // @return 0 on success, non-zero on failure.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_session_run_prefill(IntPtr session,
            IntPtr[] inputs,
            nuint num_inputs);

        // Starts the decoding process for the model to predict the response based
        // on the input prompt/query added after using litert_lm_session_run_prefill.
        // This is a blocking call and the function will return when the decoding
        // process is done.
        //
        // @param session The session to use.
        // @return A pointer to the responses, or NULL on failure. The caller is
        //   responsible for deleting the responses using `litert_lm_responses_delete`.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_session_run_decode(IntPtr session);

        // Scores the target text after the prefill process is done.
        //
        // @param session The session to use.
        // @param target_text An array of target text strings to score.
        // @param num_targets The number of strings in the target_text array.
        // @param store_token_lengths Whether to store the token lengths of the target
        //   texts in the responses.
        // @return A pointer to the responses, or NULL on failure. The caller is
        //   responsible for deleting the responses using `litert_lm_responses_delete`.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_session_run_text_scoring(IntPtr session,
            string?[] target_text,
            nuint num_targets,
            [MarshalAs(UnmanagedType.I1)] bool store_token_lengths);

        // Generates content from the input prompt.
        //
        // @param session The session to use for generation.
        // @param inputs An array of LiteRtLmInputData structs representing the
        // multimodal
        //   input.
        // @param num_inputs The number of LiteRtLmInputData structs in the array.
        // @return A pointer to the responses, or NULL on failure. The caller is
        //   responsible for deleting the responses using `litert_lm_responses_delete`.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_session_generate_content(
            IntPtr session, IntPtr[] inputs,
            nuint num_inputs);

        // Destroys a LiteRT LM Responses object.
        //
        // @param responses The responses to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_responses_delete(IntPtr responses);

        // Returns the number of response candidates.
        //
        // @param responses The responses object.
        // @return The number of candidates.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_responses_get_num_candidates(IntPtr responses);

        // Returns the response text at a given index.
        //
        // @param responses The responses object.
        // @param index The index of the response.
        // @return The response text. The returned string is owned by the `responses`
        //   object and is valid only for its lifetime. Returns NULL if index is out of
        //   bounds.
        [DllImport(LibLiteRTLM)]
        public static extern string? litert_lm_responses_get_response_text_at(
            IntPtr responses, int index);

        // Returns whether the response contains a score at the given index.
        //
        // @param responses The responses object.
        // @param index The index of the response.
        // @return true if the score is available at the given index, false otherwise.
        [DllImport(LibLiteRTLM)]
        [return: MarshalAs(UnmanagedType.I1)] 
        public static extern bool litert_lm_responses_has_score_at(IntPtr responses,
            int index);

        // Returns the score at a given index.
        //
        // @param responses The responses object.
        // @param index The index of the response.
        // @return The score. Returns 0.0f if index is out of bounds or no score is
        //   present.
        [DllImport(LibLiteRTLM)]
        public static extern float litert_lm_responses_get_score_at(IntPtr responses,
            int index);

        // Returns whether the response contains a token length at the given index.
        //
        // @param responses The responses object.
        // @param index The index of the response.
        // @return true if the token length is available at the given index, false
        //   otherwise.
        [DllImport(LibLiteRTLM)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool litert_lm_responses_has_token_length_at(IntPtr responses,
            int index);

        // Returns the token length at a given index.
        //
        // @param responses The responses object.
        // @param index The index of the response.
        // @return The token length. Returns 0 if index is out of bounds or no token
        //   length is present.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_responses_get_token_length_at(IntPtr responses,
            int index);

        // Returns whether the response contains token scores at the given index.
        //
        // @param responses The responses object.
        // @param index The index of the response.
        // @return true if token scores are available at the given index, false
        // otherwise.
        [DllImport(LibLiteRTLM)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool litert_lm_responses_has_token_scores_at(IntPtr responses,
            int index);

        // Returns the number of tokens for which scores are present at a given index.
        //
        // @param responses The responses object.
        // @param index The index of the response.
        // @return The number of token scores. Returns 0 if index is out of bounds or no
        //   token scores are present.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_responses_get_num_token_scores_at(
            IntPtr responses, int index);

        // Returns the token scores at a given index.
        //
        // @param responses The responses object.
        // @param index The index of the response.
        // @return A pointer to the internal array of token scores. Returns NULL if
        // index
        //   is out of bounds or no token scores are present.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_responses_get_token_scores_at(
            IntPtr responses, int index);

        // Retrieves the benchmark information from the session. The caller is
        // responsible for destroying the benchmark info using
        // `litert_lm_benchmark_info_delete`.
        //
        // @param session The session to get the benchmark info from.
        // @return A pointer to the benchmark info, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_session_get_benchmark_info(
            IntPtr session);

        // Destroys a LiteRT LM Benchmark Info object.
        //
        // @param benchmark_info The benchmark info to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_benchmark_info_delete(IntPtr benchmark_info);

        // Returns the time to the first token in seconds.
        //
        // Note that the first time to token doesn't include the time for
        // initialization. It is the sum of the prefill time for the first turn and
        // the time spent for decoding the first token.
        //
        // @param benchmark_info The benchmark info object.
        // @return The time to the first token in seconds.
        [DllImport(LibLiteRTLM)]
        public static extern double litert_lm_benchmark_info_get_time_to_first_token(
            IntPtr benchmark_info);

        // Returns the total initialization time in seconds.
        //
        // @param benchmark_info The benchmark info object.
        // @return The total initialization time in seconds.
        [DllImport(LibLiteRTLM)]
        public static extern double litert_lm_benchmark_info_get_total_init_time_in_second(
            IntPtr benchmark_info);

        // Returns the number of prefill turns.
        //
        // @param benchmark_info The benchmark info object.
        // @return The number of prefill turns.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_benchmark_info_get_num_prefill_turns(
            IntPtr benchmark_info);

        // Returns the number of decode turns.
        //
        // @param benchmark_info The benchmark info object.
        // @return The number of decode turns.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_benchmark_info_get_num_decode_turns(
            IntPtr benchmark_info);

        // Returns the prefill token count at a given turn index.
        //
        // @param benchmark_info The benchmark info object.
        // @param index The index of the prefill turn.
        // @return The prefill token count.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_benchmark_info_get_prefill_token_count_at(
            IntPtr benchmark_info, int index);

        // Returns the decode token count at a given turn index.
        //
        // @param benchmark_info The benchmark info object.
        // @param index The index of the decode turn.
        // @return The decode token count.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_benchmark_info_get_decode_token_count_at(
            IntPtr benchmark_info, int index);

        // Returns the prefill tokens per second at a given turn index.
        //
        // @param benchmark_info The benchmark info object.
        // @param index The index of the prefill turn.
        // @return The prefill tokens per second.
        [DllImport(LibLiteRTLM)]
        public static extern double litert_lm_benchmark_info_get_prefill_tokens_per_sec_at(
            IntPtr benchmark_info, int index);

        // Returns the decode tokens per second at a given turn index.
        //
        // @param benchmark_info The benchmark info object.
        // @param index The index of the decode turn.
        // @return The decode tokens per second.
        [DllImport(LibLiteRTLM)]
        public static extern double litert_lm_benchmark_info_get_decode_tokens_per_sec_at(
            IntPtr benchmark_info, int index);

        // Callback for streaming responses.
        // `callback_data` is a pointer to user-defined data passed to the stream
        // function. `chunk` is the piece of text from the stream. It's only valid for
        // the duration of the call. `is_final` is true if this is the last chunk in the
        // stream. `error_msg` is a null-terminated string with an error message, or
        // NULL on success.
        public delegate void LiteRtLmStreamCallback(IntPtr callback_data, string? chunk,
            [MarshalAs(UnmanagedType.I1)] bool is_final, string? error_msg);

        // Starts the decoding process for the model to predict the response based
        // on the input prompt/query added after using litert_lm_session_run_prefill.
        // This is a non-blocking call that will stream responses via a callback.
        //
        // @param session The session to use.
        // @param callback The callback function to receive response chunks.
        // @param callback_data A pointer to user data that will be passed to the
        // callback.
        // @return 0 on success, non-zero on failure.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_session_run_decode_async(IntPtr session,
            LiteRtLmStreamCallback callback,
            IntPtr callback_data);

        // Generates content from the input prompt and streams the response via a
        // callback. This is a non-blocking call that will invoke the callback from a
        // background thread for each chunk.
        //
        // @param session The session to use for generation.
        // @param inputs An array of LiteRtLmInputData structs representing the
        // multimodal
        //   input.
        // @param num_inputs The number of LiteRtLmInputData structs in the array.
        // @param callback The callback function to receive response chunks.
        // @param callback_data A pointer to user data that will be passed to the
        // callback.
        // @return 0 on success, non-zero on failure to start the stream.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_session_generate_content_stream(
            IntPtr session, IntPtr[] inputs,
            nuint num_inputs, LiteRtLmStreamCallback callback, IntPtr callback_data);

        // Creates a LiteRT LM Conversation. The caller is responsible for destroying
        // the conversation using `litert_lm_conversation_delete`.
        //
        // @param engine The engine to create the conversation from.
        // @param config The conversation config to use. If NULL, the default config
        //   will be used.
        // @return A pointer to the created conversation, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_conversation_create(
            IntPtr engine, IntPtr config);

        // Destroys a LiteRT LM Conversation.
        //
        // @param conversation The conversation to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_delete(IntPtr conversation);

        // Clones a LiteRT LM Conversation, duplicating its prefilled state.
        // The caller is responsible for destroying the cloned conversation using
        // `litert_lm_conversation_delete`.
        //
        // @param conversation The conversation to clone.
        // @return A pointer to the cloned conversation, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_conversation_clone(
            IntPtr conversation);

        // Sends a message to the conversation and returns the response.
        // This is a blocking call.
        //
        // @param conversation The conversation to use.
        // @param message_json A JSON string representing the message to send.
        // @param extra_context A JSON string representing the extra context to use.
        // @param optional_args A pointer to the optional arguments to use.
        // @return A pointer to the JSON response, or NULL on failure. The caller is
        //   responsible for deleting the response using
        //   `litert_lm_json_response_delete`.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_conversation_send_message(
            IntPtr conversation, string? message_json,
            string? extra_context,
            IntPtr optional_args);

        // Destroys a LiteRT LM Json Response object.
        //
        // @param response The response to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_json_response_delete(IntPtr response);

        // Returns the JSON response string from a response object.
        //
        // @param response The response object.
        // @return The response JSON string. The returned string is owned by the
        //   `response` object and is valid only for its lifetime. Returns NULL if
        //   response is NULL.
        [DllImport(LibLiteRTLM)]
        public static extern string? litert_lm_json_response_get_string(
            IntPtr response);

        // Sends a message to the conversation and streams the response via a
        // callback. This is a non-blocking call that will invoke the callback from a
        // background thread for each chunk.
        //
        // @param conversation The conversation to use.
        // @param message_json A JSON string representing the message to send.
        // @param extra_context A JSON string representing the extra context to use.
        // @param optional_args A pointer to the optional arguments to use.
        // @param callback The callback function to receive response chunks.
        // @param callback_data A pointer to user data that will be passed to the
        // callback.
        // @return 0 on success, non-zero on failure to start the stream.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_conversation_send_message_stream(
            IntPtr conversation, string? message_json,
            string? extra_context,
            IntPtr optional_args,
            LiteRtLmStreamCallback callback, IntPtr callback_data);

        // Renders the message into a string according to the template.
        //
        // This function does not need to be called for actual message sending, as the
        // `litert_lm_conversation_send_message` and
        // `litert_lm_conversation_send_message_stream` functions will handle rendering
        // internally.
        //
        // @param conversation The conversation instance.
        // @param message_json A JSON string representing the message to render.
        // @return A pointer to the rendered string, or NULL on failure. The returned
        //   string is owned by the `conversation` object and is valid until the next
        //   call to this function or until the conversation is deleted.
        [DllImport(LibLiteRTLM)]
        public static extern string? litert_lm_conversation_render_message_to_string(
            IntPtr conversation, string? message_json);

        // Renders the preface into a string according to the template.
        //
        // @param conversation The conversation instance.
        // @return A pointer to the rendered string, or NULL on failure. The returned
        //   string is owned by the `conversation` object and is valid until the next
        //   call to this function or until the conversation is deleted.
        [DllImport(LibLiteRTLM)]
        public static extern string? litert_lm_conversation_render_preface_to_string(
            IntPtr conversation);

        // Cancels the ongoing inference process, for asynchronous inference.
        //
        // @param conversation The conversation to cancel the inference for.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_conversation_cancel_process(IntPtr conversation);

        // Retrieves the benchmark information from the conversation. The caller is
        // responsible for destroying the benchmark info using
        // `litert_lm_benchmark_info_delete`.
        //
        // @param conversation The conversation to get the benchmark info from.
        // @return A pointer to the benchmark info, or NULL on failure.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_conversation_get_benchmark_info(
            IntPtr conversation);

        // Gets the number of tokens in the conversation KV Cache (prefill + decode).
        // Returns the number of tokens, or a negative value on failure.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_conversation_get_token_count(IntPtr conversation);

        // Tokenizes text using the engine's tokenizer.
        //
        // @param engine The engine instance.
        // @param text The UTF-8 string to tokenize.
        // @return A pointer to the tokenize result, or NULL on failure.
        //   The caller is responsible for deleting the result using
        //   `litert_lm_tokenize_result_delete`.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_engine_tokenize(IntPtr engine,
            string? text);

        // Destroys a LiteRT LM Tokenize Result.
        //
        // @param result The tokenize result to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_tokenize_result_delete(IntPtr result);

        // Returns the token ids from a tokenize result.
        //
        // @param result The tokenize result.
        // @return A pointer to the internal array of token ids. The returned pointer
        //   is valid only for the lifetime of the `result` object.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_tokenize_result_get_tokens(
            IntPtr result);

        // Returns the number of token ids from a tokenize result.
        //
        // @param result The tokenize result.
        // @return The number of token ids.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_tokenize_result_get_num_tokens(
            IntPtr result);

        // Detokenizes token ids using the engine's tokenizer.
        //
        // @param engine The engine instance.
        // @param tokens An array of token ids to detokenize.
        // @param num_tokens The number of token ids in the array.
        // @return A pointer to the detokenize result, or NULL on failure.
        //   The caller is responsible for deleting the result using
        //   `litert_lm_detokenize_result_delete`.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_engine_detokenize(IntPtr engine,
            IntPtr tokens,
            nuint num_tokens);

        // Destroys a LiteRT LM Detokenize Result.
        //
        // @param result The detokenize result to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_detokenize_result_delete(IntPtr result);

        // Returns the string from a detokenize result.
        //
        // @param result The detokenize result.
        // @return The detokenized UTF-8 string. The returned string is owned by the
        //   `result` object and is valid only for its lifetime.
        [DllImport(LibLiteRTLM)]
        public static extern string? litert_lm_detokenize_result_get_string(
            IntPtr result);

        // Destroys a LiteRT LM Token Union.
        //
        // @param token_union The token union to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_token_union_delete(IntPtr token_union);

        // Returns the type of the token union.
        //
        // @param token_union The token union.
        // @return The type of the token union.
        [DllImport(LibLiteRTLM)]
        public static extern LiteRtLmTokenUnionType litert_lm_token_union_get_type(
            IntPtr token_union);

        // Returns the string value from a token union.
        //
        // @param token_union The token union.
        // @return The string value, or NULL if the type is not
        //   kLiteRtLmTokenUnionTypeString. The returned string is owned by the
        //   `token_union` object and is valid only for its lifetime.
        [DllImport(LibLiteRTLM)]
        public static extern string? litert_lm_token_union_get_string(
            IntPtr token_union);

        // Returns the token ids from a token union.
        //
        // @param token_union The token union.
        // @param out_tokens A pointer to receive the internal array of token ids.
        //   The received pointer is valid only for the lifetime of the `token_union`
        //   object.
        // @param out_num_tokens A pointer to receive the number of token ids.
        // @return 0 on success, non-zero if the type is not kLiteRtLmTokenUnionTypeIds.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_token_union_get_ids(IntPtr token_union,
            ref IntPtr out_tokens,
            ref nuint out_num_tokens);

        // Destroys a LiteRT LM Token Unions object.
        //
        // @param tokens The token unions object to destroy.
        [DllImport(LibLiteRTLM)]
        public static extern void litert_lm_token_unions_delete(IntPtr tokens);

        // Returns the number of token unions in the collection.
        //
        // @param tokens The token unions object.
        // @return The number of token unions.
        [DllImport(LibLiteRTLM)]
        public static extern int litert_lm_token_unions_get_num_tokens(IntPtr tokens);

        // Returns the token union at a given index from a collection.
        //
        // @param tokens The token unions collection.
        // @param index The index of the token union.
        // @return A pointer to the token union at the given index, or NULL if the index
        //   is out of bounds. The caller is responsible for deleting the result using
        //   `litert_lm_token_union_delete`.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_token_unions_get_token_at(
            IntPtr tokens, nuint index);

        // Returns the configured start token (BOS), if any.
        //
        // @param engine The engine instance.
        // @return A pointer to the start token, or NULL if none configured. The caller
        //   is responsible for deleting the result using
        //   `litert_lm_token_union_delete`.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_engine_get_start_token(IntPtr engine);

        // Returns the configured stop tokens (EOS).
        //
        // @param engine The engine instance.
        // @return A pointer to the stop tokens collection, or NULL if none configured.
        //   The caller is responsible for deleting the result using
        //   `litert_lm_token_unions_delete`.
        [DllImport(LibLiteRTLM)]
        public static extern IntPtr litert_lm_engine_get_stop_tokens(IntPtr engine);
    }
}