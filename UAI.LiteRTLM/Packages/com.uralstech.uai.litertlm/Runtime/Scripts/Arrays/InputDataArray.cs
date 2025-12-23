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
using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    /// <summary>
    /// An array of <see cref="InputData"/>.
    /// </summary>
    /// <inheritdoc/>
    public sealed class InputDataArray : JavaArrayList<InputData>
    {
        /// <summary>
        /// Creates a new <see cref="InputDataArray"/> object.
        /// </summary>
        /// <inheritdoc cref="JavaArrayList{T}(IReadOnlyList{T}, bool)"/>
        public InputDataArray(IReadOnlyList<InputData> elements, bool handleChildDispose = true) : base(elements, handleChildDispose) { }

        /// <inheritdoc/>
        protected override InputData ElementFactory(AndroidJavaObject native) => throw new NotSupportedException();

        /// <inheritdoc/>
        protected override InputData ElementFactory(InputData other) => throw new NotSupportedException();
        
        public static implicit operator InputDataArray(InputData[] current) => new(current, true);
        public static implicit operator InputDataArray(List<InputData> current) => new(current, true);
    }
}