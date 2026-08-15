# Quick Start

The example code in this quick start guide is provided for educational and demonstration purposes only.
It may not represent best practices for production use.

## What is this?

`com.uralstech.uai.litertlm.omni` is a *very* early Unity/C# wrapper for LiteRT-LM's Omni API.

The Omni API itself is a *very* early implementation of a framework for working with LLM-based [ASR and TTS models](https://github.com/google-ai-edge/LiteRT-LM/issues/3058#issuecomment-5194213241).

Due to the ***highly*** unstable, and I mean ***seriously unstable*** nature of the API, this package currently has several significant limitations:

- Inference is currently CPU-only and supported on Android (arm64), macOS (arm64), iOS (arm64, sim_arm64), and Windows (x64).
- It currently supports only Qwen-3 TTS derivatives compiled to fp32 LiteRT files. The model data must also undergo additional conversion to match the format expected by the engine. See the [prebuilt](https://huggingface.co/uralstech/Qwen3-TTS-12Hz-0.6B-Base-litert-lm-omni) model for an example.
- The package is not currently versioned using git tags, so there are no stable version identifiers to depend on.

## Download

### Unity Package Manager

1. Open the Unity Package Manager window (`Window` -> `Package Manager`)
2. Select the `+` icon and `Add package from git URL...`
3. Paste the package URL and press enter:
    - `https://github.com/Uralstech/UAI.LiteRTLM.git?path=UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm.omni`

### GitHub Clone

1. Clone or download the repository from the desired branch (master, preview/unstable) or tag
2. Drag the package folder `UAI.LiteRTLM/UAI.LiteRTLM/Packages/com.uralstech.uai.litertlm.omni` into your Unity project's `Packages` folder

## Usage

The package only provides an extremely low-level wrapper for a custom C wrapper of the Omni API.
The source code of the C wrapper is available here: <https://github.com/Uralstech/LiteRT-LM/tree/main/c/tts>

The wrapper methods are not currently documented, but most are straightforward to understand from their names.

To run the example below, first download a model such as [Qwen3-TTS-12Hz](https://huggingface.co/uralstech/Qwen3-TTS-12Hz-0.6B-Base-litert-lm-omni). Place all of the model files in a folder named `TTS` under `Application.persistentDataPath`.

```csharp
using System;
using System.IO;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Uralstech.UAI.LiteRTLM.Omni.TTS;
using Uralstech.UAI.LiteRTLM.Omni.TTS.Native;

namespace DefaultNamespace
{
    public class TTSTest : MonoBehaviour
    {
        IntPtr _engine = IntPtr.Zero;
        IntPtr _session = IntPtr.Zero;
        
        private void Awake()
        {
            string modelFolder = Path.Join(Application.persistentDataPath, "TTS");
            string cacheDir = Path.Join(Application.temporaryCachePath, "TTS");
            Directory.CreateDirectory(cacheDir);
            
            IntPtr settings = IntPtr.Zero;

            try
            {
                float start = Time.realtimeSinceStartup;
                int threadCount = JobsUtility.JobWorkerMaximumCount;

                settings = NativeAPI.TTSEngineSettings.litert_omni_tts_engine_settings_create(ModelType.Qwen3TTS);
                NativeAPI.TTSEngineSettings.litert_omni_tts_engine_settings_set_model_folder(settings, modelFolder);
                NativeAPI.TTSEngineSettings.litert_omni_tts_engine_settings_set_cache_dir(settings, cacheDir);

                NativeAPI.TTSEngineSettings.litert_omni_tts_engine_settings_set_num_threads(settings, threadCount);
                NativeAPI.TTSEngineSettings.litert_omni_tts_engine_settings_set_max_frames(settings, maxFrames: 60); // for perf
                if (settings == IntPtr.Zero) return;

                _engine = NativeAPI.TTSEngine.litert_omni_tts_engine_create(settings);
                if (_engine == IntPtr.Zero) return;

                _session = NativeAPI.TTSEngine.litert_omni_tts_engine_create_session(_engine);
                Debug.Log($"TTS loaded in: {Time.realtimeSinceStartup - start}s");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                DisposeNative();
            }
            finally
            {
                if (settings != IntPtr.Zero)
                    NativeAPI.TTSEngineSettings.litert_omni_tts_engine_settings_delete(settings);
            }
        }

        private void OnDestroy() => DisposeNative();

        private void DisposeNative()
        {
            if (_session != IntPtr.Zero)
                NativeAPI.TTSSession.litert_omni_tts_session_delete(_session);
                
            if (_engine != IntPtr.Zero)
                NativeAPI.TTSEngine.litert_omni_tts_engine_delete(_engine);
                
            _session = IntPtr.Zero;
            _engine = IntPtr.Zero;
        }

        public unsafe void Talk()
        {
            IntPtr audio = IntPtr.Zero;

            try
            {
                float start = Time.realtimeSinceStartup;
                
                audio = NativeAPI.TTSSession.litert_omni_tts_session_synthesize(_session, "Hello! how are you?");
                if (audio == IntPtr.Zero) return;

                int sampleRate = NativeAPI.AudioOutput.litert_omni_audio_output_get_sample_rate_hz(audio);
                int size = (int)NativeAPI.AudioOutput.litert_omni_audio_output_get_num_pcm_samples(audio);
                IntPtr data = NativeAPI.AudioOutput.litert_omni_audio_output_get_pcm_samples(audio);
                
                Debug.Log($"TTS completed in: {Time.realtimeSinceStartup - start}s, data: {size} bytes");
                if (size == 0) return;

                AudioClip clip = AudioClip.Create("TTS-Test", size, 1, sampleRate, false);
                if (!clip.SetData(new ReadOnlySpan<float>((void*)data, size), 0))
                {
                    Debug.LogError("Could not set clip data.");
                    Destroy(clip);
                    return;
                }
                
                GetComponent<AudioSource>().PlayOneShot(clip);
            }
            finally
            {
                if (audio != IntPtr.Zero)
                    NativeAPI.AudioOutput.litert_omni_audio_output_delete(audio);
            }
        }
    }
}
```