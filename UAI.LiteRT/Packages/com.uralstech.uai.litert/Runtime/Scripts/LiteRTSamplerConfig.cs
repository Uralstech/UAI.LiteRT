// Copyright 2025 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
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
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRT
{
    public record LiteRTSamplerConfig : IDisposable
    {
        public readonly double Temperature;
        public readonly double TopP;
        public readonly int TopK;
        public readonly int Seed;

        internal readonly AndroidJavaObject _native;
        private bool _disposed;

        public LiteRTSamplerConfig(double temperature = 1f, double topP = 0.95f, int topK = 64, int seed = 0)
        {
            Temperature = temperature;
            TopP = topP;
            TopK = topK;
            Seed = seed;

            _native = new AndroidJavaObject("com.google.ai.edge.litertlm.SamplerConfig", topK, topP, temperature, seed);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _native.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
