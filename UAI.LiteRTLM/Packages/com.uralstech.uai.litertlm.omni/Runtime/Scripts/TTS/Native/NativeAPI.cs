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

#nullable enable
namespace Uralstech.UAI.LiteRTLM.Omni.TTS.Native
{
    public static class NativeAPI
    {
        public const string LibLiteRTOmniTTS = "litert-omni-tts";
        
        public static class TTSEngineSettings
        {
            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr litert_omni_tts_engine_settings_create(ModelType modelType);

            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern void litert_omni_tts_engine_settings_delete(IntPtr settings);

            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern void litert_omni_tts_engine_settings_set_model_folder(IntPtr settings, [MarshalAs(UnmanagedType.LPUTF8Str)] string modelFolder);

            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern void litert_omni_tts_engine_settings_set_cache_dir(IntPtr settings, [MarshalAs(UnmanagedType.LPUTF8Str)] string cacheDir);
            
            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern void litert_omni_tts_engine_settings_set_backend(IntPtr settings, [MarshalAs(UnmanagedType.LPUTF8Str)] string backendStr);

            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern void litert_omni_tts_engine_settings_set_num_threads(IntPtr settings, int numThreads);
            
            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern void litert_omni_tts_engine_settings_set_max_frames(IntPtr settings, int maxFrames);
        }
        
        public static class TTSEngine
        {
            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr litert_omni_tts_engine_create(IntPtr settings);

            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern void litert_omni_tts_engine_delete(IntPtr engine);

            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr litert_omni_tts_engine_create_session(IntPtr engine);
        }
        
        public static class TTSSession
        {
            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern void litert_omni_tts_session_delete(IntPtr session);

            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr litert_omni_tts_session_synthesize(IntPtr session, [MarshalAs(UnmanagedType.LPUTF8Str)] string text);
        }
        
        public static class AudioOutput
        {
            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern void litert_omni_audio_output_delete(IntPtr audio);

            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr litert_omni_audio_output_get_pcm_samples(IntPtr audio);

            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern UIntPtr litert_omni_audio_output_get_num_pcm_samples(IntPtr audio);

            [DllImport(LibLiteRTOmniTTS, CallingConvention = CallingConvention.Cdecl)]
            public static extern int litert_omni_audio_output_get_sample_rate_hz(IntPtr audio);
        }
    }
}
