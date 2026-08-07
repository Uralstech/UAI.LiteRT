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

using System;
using System.Runtime.InteropServices;
using Uralstech.UAI.LiteRTLM;
using Uralstech.UAI.LiteRTLM.Native;

NativeAPI.litert_lm_set_min_log_level(LogSeverity.Info);

string modelPath = args[0];
string cacheDir = ":nocache";

IntPtr engineSettings = IntPtr.Zero;
IntPtr engine = IntPtr.Zero;

IntPtr thinkingConfig = IntPtr.Zero;
IntPtr conversationConfig = IntPtr.Zero;
IntPtr conversation = IntPtr.Zero;

IntPtr jsonResponse = IntPtr.Zero;
IntPtr benchmarkInfo = IntPtr.Zero;

try
{
    engineSettings = NativeAPI.EngineSettings.litert_lm_engine_settings_create(modelPath, BackendNames.GPU, null, null);
    NativeAPI.EngineSettings.litert_lm_engine_settings_set_enable_speculative_decoding(engineSettings, true);
    NativeAPI.EngineSettings.litert_lm_engine_settings_set_cache_dir(engineSettings, cacheDir);
    NativeAPI.EngineSettings.litert_lm_engine_settings_enable_benchmark(engineSettings);

    engine = NativeAPI.Engine.litert_lm_engine_create(engineSettings);

    thinkingConfig = NativeAPI.ThinkingConfig.litert_lm_thinking_config_create();
    NativeAPI.ThinkingConfig.litert_lm_thinking_config_set_enable_thinking(thinkingConfig, false);
    
    conversationConfig = NativeAPI.ConversationConfig.litert_lm_conversation_config_create();
    NativeAPI.ConversationConfig.litert_lm_conversation_config_set_thinking_config(conversationConfig, thinkingConfig);

    conversation = NativeAPI.Conversation.litert_lm_conversation_create(engine, conversationConfig);
    Console.WriteLine("Engine and conversation created.");
    
    const string message = "{\"role\":\"user\",\"content\":\"What is the tallest building in the world?\"}";
    jsonResponse = NativeAPI.Conversation.litert_lm_conversation_send_message(conversation, message, null, IntPtr.Zero);

    IntPtr responsePtr = NativeAPI.JsonResponse.litert_lm_json_response_get_string(jsonResponse);
    Console.WriteLine($"AI response: {Marshal.PtrToStringUTF8(responsePtr)}");
    
    benchmarkInfo = NativeAPI.Conversation.litert_lm_conversation_get_benchmark_info(conversation);
    if (benchmarkInfo == IntPtr.Zero)
    {
        Console.WriteLine("No benchmark info found.");
        return;
    }

    double prefillTps = NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_get_prefill_tokens_per_sec_at(benchmarkInfo, 0);
    double decodeTps = NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_get_decode_tokens_per_sec_at(benchmarkInfo, 0);
    
    Console.WriteLine($"Prefill: {prefillTps} t/s, decode: {decodeTps} t/s");
}
finally
{
    InvokeSafe(benchmarkInfo, NativeAPI.BenchmarkInfo.litert_lm_benchmark_info_delete);
    InvokeSafe(jsonResponse, NativeAPI.JsonResponse.litert_lm_json_response_delete);
    
    InvokeSafe(conversation, NativeAPI.Conversation.litert_lm_conversation_delete);
    InvokeSafe(conversationConfig, NativeAPI.ConversationConfig.litert_lm_conversation_config_delete);
    InvokeSafe(thinkingConfig, NativeAPI.ThinkingConfig.litert_lm_thinking_config_delete);
    
    InvokeSafe(engine, NativeAPI.Engine.litert_lm_engine_delete);
    InvokeSafe(engineSettings, NativeAPI.EngineSettings.litert_lm_engine_settings_delete);
}

static void InvokeSafe(IntPtr ptr, Action<IntPtr> action)
{
    if (ptr != IntPtr.Zero)
        action(ptr);
}