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

using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    /// <summary>
    /// Configuration for the sampling process.
    /// </summary>
    /// <remarks>
    /// This object manages a native <c>com.google.ai.edge.litertlm.SamplerConfig</c> object and must be disposed after usage.
    /// </remarks>
    public sealed class SamplerConfig : JavaObject
    {
        /// <summary>
        /// The temperature to use for sampling.
        /// </summary>
        public readonly double Temperature;

        /// <summary>
        /// The cumulative probability threshold for nucleus sampling.
        /// </summary>
        public readonly double TopP;

        /// <summary>
        /// The number of top logits used during sampling.
        /// </summary>
        public readonly int TopK;

        /// <summary>
        /// The seed to use for randomization. Default to 0 (same default as engine code).
        /// </summary>
        public readonly int Seed;

        /// <inheritdoc/>
        public override AndroidJavaObject Handle { get; }

        /// <summary>
        /// Creates a new <see cref="SamplerConfig"/> object.
        /// </summary>
        /// <param name="temperature">The temperature to use for sampling.</param>
        /// <param name="topP">The cumulative probability threshold for nucleus sampling.</param>
        /// <param name="topK">The number of top logits used during sampling.</param>
        /// <param name="seed">The seed to use for randomization. Default to 0 (same default as engine code).</param>
        public SamplerConfig(double temperature = 1f, double topP = 0.95f, int topK = 64, int seed = 0)
        {
            Temperature = temperature;
            TopP = topP;
            TopK = topK;
            Seed = seed;

            Handle = new AndroidJavaObject("com.google.ai.edge.litertlm.SamplerConfig", topK, topP, temperature, seed);
        }
    }
}
