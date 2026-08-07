using System.Text;
using Uralstech.UAI.LiteRTLM;

namespace MauiPlayground;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RunInferenceAsync();
        Console.WriteLine("Inference completed.");
    }

    private async Task RunInferenceAsync()
    {
        Accelerators.LoadNativeLibraries();
        LiteRTLMNativeLogging.SetMinLogLevel(LogSeverity.Verbose);
        
        #if ANDROID
        string modelPath = Path.Join(Android.App.Application.Context.GetExternalFilesDir(null)!.AbsolutePath, "model.litertlm");
        #else
        string modelPath = Path.Join(FileSystem.AppDataDirectory, "model.litertlm");
        #endif

        Console.WriteLine("Loading model: " + modelPath);
        
        using EngineSettings engineSettings = new(modelPath, BackendNames.GPU);
        //engineSettings.SetEnableSpeculativeDecoding(true);
        engineSettings.SetCacheDir(":nocache");
        engineSettings.EnableBenchmark();
        
        using Engine engine = new(engineSettings);
        using ThinkingConfig thinkingConfig = new();
        thinkingConfig.SetEnableThinking(false);
        
        using ConversationConfig conversationConfig = new();
        conversationConfig.SetThinkingConfig(thinkingConfig);
        
        using Conversation conversation = new(engine, conversationConfig);
        Console.WriteLine("Engine and conversation created.");
        
        const string message = "{\"role\":\"user\",\"content\":\"Give me 10 random dates in \\\"dd-yyyy-mm\\\" format.\"}";
        TaskCompletionSource<bool> completionSource = new();
        
        StringBuilder sb = new();
        void OnChunk(StreamChunk chunk)
        {
            Console.WriteLine("Got chunk:"
                      + $"\n\tText: {chunk.GetText()}"
                      + $"\n\tIsFinal: {chunk.IsFinal()}"
                      + $"\n\tError: {chunk.GetError()}"
            );
        
            string? text = chunk.GetText();
            int index = text?.IndexOf("text\":") ?? -1;
            if (index >= 0)
            {
                int start = index + 7;
                int end = text!.IndexOf('\"', start);
                
                sb.Append(text.Substring(start, end - start));
            }
            
            if (chunk.IsFinal())
                completionSource.TrySetResult(true);
        }
        
        int result = conversation.SendMessageStream(OnChunk, message);
        Console.WriteLine("Message sent: " + (result == 0));
        
        if (result == 0)
            await completionSource.Task;
        
        Console.WriteLine("result: " + sb);
        
        using BenchmarkInfo? benchmarkInfo = conversation.GetBenchmarkInfo();
        if (benchmarkInfo == null)
        {
            Console.WriteLine("No benchmark info found.");
            return;
        }
        
        BenchmarkInfo.Turn[] prefillTurns = benchmarkInfo.GetPrefillTurns();
        BenchmarkInfo.Turn[] decodeTurns = benchmarkInfo.GetDecodeTurns();

        Console.WriteLine("Benchmark info:"
                          + $"\n\tInitialization time: {benchmarkInfo.GetTotalInitTime()}"
                          + $"\n\tTime to first token: {benchmarkInfo.GetTimeToFirstToken()}"
                          + $"\n\tPrefill tokens per second: {prefillTurns[0].TokensPerSecond}, for total: {prefillTurns[0].TokenCount} tokens"
                          + $"\n\tDecode tokens per second: {decodeTurns[0].TokensPerSecond}, for total: {decodeTurns[0].TokenCount} tokens"
        );
    }
}
