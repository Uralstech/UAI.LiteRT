# Qwen 3 TTS Conversion

This folder contains the scripts required to convert Qwen3 TTS LiteRT models into the format expected by the LiteRT-LM Omni TTS Engine.

The main conversion script is `convert.py`.

## 1. Download the model

Download the required model files from Hugging Face using the `hf` CLI:

```sh
hf download litert-community/Qwen3-TTS-12Hz-0.6B-Base \
  tables/text_embedding_fp16.npy                      \
  tables/codec_embedding_fp32.npy                     \
  tables/mtp_embeddings_fp16.npy                      \
  tables/text_projection_fp32.npz                     \
  voices/demo_speaker.npy                             \
  mtp_fp32.tflite                                     \
  tokenizer.json                                      \
  talker_int4.tflite                                  \
  codec_decoder_fp32.tflite                           \
  --local-dir ./Qwen3-TTS-12Hz-0.6B-Base
```

This will download the model into `./Qwen3-TTS-12Hz-0.6B-Base`.

## 2. Set up the Python environment

The conversion script has been tested with Python 3.13. TensorFlow supports Python versions 3.10 through 3.13, so any of these versions should work.

Create and activate a virtual environment:

```sh
python3.13 -m venv .env
source .env/bin/activate
```

Then install the required dependencies:

```sh
pip install -r requirements.txt
```

## 3. Convert the model

Run `convert.py` and provide the downloaded model directory as the input:

```sh
python convert.py -i ./Qwen3-TTS-12Hz-0.6B-Base
```

The converted files will be generated in:

```
converted/
```

**All files in the `converted` directory are required by the LiteRT-LM Omni TTS Engine.**
Make sure to include the complete directory when deploying the converted model.

## Verified model

The conversion script has been verified with:

* [litert-community/Qwen3-TTS-12Hz-0.6B-Base](https://huggingface.co/litert-community/Qwen3-TTS-12Hz-0.6B-Base)

The resulting converted model is available here:

* [uralstech/Qwen3-TTS-12Hz-0.6B-Base-litert-lm-omni](https://huggingface.co/uralstech/Qwen3-TTS-12Hz-0.6B-Base-litert-lm-omni)

## Voice cloning

You can build a custom voice for the TTS using the Qwen3-TTS speaker encoder.
This voice-cloning workflow has been tested in Google Colab.

The speaker embedding extraction is based on the original Google AI Edge
[`extract_speaker_embedding.py`](https://github.com/john-rocky/litert-samples/blob/a1f5edf3948e9575824bf6ea568fc90ad6a8fef0/compiled_model_api/text_to_speech_lm/conversion/extract_speaker_embedding.py)
implementation by [john-rocky](https://github.com/john-rocky).

Install the required dependencies:

```
!pip install qwen-tts==0.1.1 transformers==4.57.3 torchaudio librosa soundfile sox onnxruntime
```

Then provide a reference audio file and extract the speaker embedding:

```python
import torch
import librosa
import numpy as np
from qwen_tts import Qwen3TTSModel

MODEL_ID = "Qwen/Qwen3-TTS-12Hz-0.6B-Base"
AUDIO_SRC = "/content/source.wav"
DST = "/content/demo_speaker.bin"

wrapper = Qwen3TTSModel.from_pretrained(MODEL_ID, device_map='cpu', dtype=torch.float32)
audio, sample_rate = librosa.load(AUDIO_SRC, sr=24000, mono=True)

embedding = wrapper.model.extract_speaker_embedding(audio=audio.astype(np.float32), sr=sample_rate)
weights = embedding.detach().numpy().astype(np.float32).reshape(-1)
weights.tofile(DST)

try:
    from google.colab import files
    files.download(DST)
finally:
    pass
```

The resulting `demo_speaker.bin` contains the speaker embedding as 1024 raw float32 values (4096 bytes)
and can be used as the custom speaker embedding for the TTS pipeline.