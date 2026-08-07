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
using System.Text;
using System.Threading.Tasks;
using Uralstech.UAI.LiteRTLM;

LiteRTLMNativeLogging.SetMinLogLevel(LogSeverity.Verbose);
Accelerators.LoadNativeLibraries();

string modelPath = args[0];
Console.WriteLine("Loading model: " + modelPath);

using EngineSettings engineSettings = new(modelPath, BackendNames.GPU);
engineSettings.SetEnableSpeculativeDecoding(true);
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