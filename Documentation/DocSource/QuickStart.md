# Quick Start

The example code provided in this quick start guide is for educational and demonstration purposes only.
It may not represent best practices for production use.

## Breaking Changes Notice

If you've just updated the package, it is recommended to check the [*changelogs*](https://github.com/Uralstech/UAI.LiteRTLM/releases) for information on breaking changes.

## Example

This documentation is still a WIP. This is an example script for a sustained conversational experience, tested with [gemma-3n-E2B-it-int4](https://huggingface.co/google/gemma-3n-E2B-it-litert-lm) on a Quest 3. Please refer to the reference manual for more information on each class.

Before running this script, push your LiteRT-LM model to your device at `/data/local/tmp/model.litertlm` using ADB, like:
`adb push /Path/to/your/model.litertlm /data/local/tmp/model.litertlm`

```csharp
using TMPro;
using UnityEngine;
using Uralstech.UAI.LiteRTLM;

public class test : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private TMP_InputField _field;

    private Engine _engine;
    private Conversation _conversation;

    protected async void Start()
    {
        _text.text = "Loading.";

        Engine.SetNativeLogSeverity(Engine.LogSeverity.Warning);
        _engine = await Engine.CreateAsync("/data/local/tmp/model.litertlm", Engine.Backend.GPU, maxTokens: 4000);

        using SamplerConfig samplerConfig = new(1f, topK: 64, topP: 0.95f);
        _conversation = _engine.CreateConversation(samplerConfig: samplerConfig);
        _text.text = "Ready!";
    }

    public async void SendHi()
    {
        if (_conversation == null)
            return;

        _text.text = string.Empty;

        string message = "You are a friendly assistant.\n---\n" + _field.text;

        using Message msg = Message.Of(message);
        Debug.Log($"Sending: {message}");

        await foreach (Message part in _conversation.StreamSendMessageAsync(msg))
        {
            _text.text += part.ToString();
            part.Dispose();
        }
    }

    public void Cancel()
    {
        if (_conversation == null)
            return;

        _conversation.CancelProcess();
    }

    protected void OnDestroy()
    {
        _conversation.Dispose();
        _engine.Dispose();
    }
}
```