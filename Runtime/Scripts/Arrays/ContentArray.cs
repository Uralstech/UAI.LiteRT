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

using System.Collections.Generic;
using UnityEngine;

#nullable enable
namespace Uralstech.UAI.LiteRTLM
{
    /// <summary>
    /// An array of <see cref="Content"/>s.
    /// </summary>
    /// <remarks>
    /// This object manages a native <c>java.util.ArrayList</c> object and must be disposed after usage
    /// OR must be managed by a <see cref="Message"/> to handle its disposal.
    /// </remarks>
    public sealed class ContentArray : JavaArrayList<Content>
    {
        /// <summary>
        /// Creates a new <see cref="ContentArray"/> object.
        /// </summary>
        /// <inheritdoc cref="JavaArrayList{T}(IReadOnlyList{T}, bool)"/>
        public ContentArray(IReadOnlyList<Content> elements, bool handleChildDispose = true) : base(elements, handleChildDispose) { }

        /// <summary>
        /// Creates a new <see cref="ContentArray"/> from an existing one.
        /// </summary>
        /// <remarks>
        /// This creates a semi-deep copy of <paramref name="other"/>. A new <see cref="AndroidJavaObject"/>
        /// which refers to the same native Kotlin object as <paramref name="other"/> is created, and a shallow
        /// copy of each of <paramref name="other"/>'s elements is added into a new array and stored as <see cref="JavaArrayList{T}.Elements"/>.
        /// The new instance's <see cref="JavaArrayList{T}.HandleElementsDispose"/> is set to <see langword="true"/>.
        /// 
        /// For more detail on how the elements are shallow copied, see <see cref="Content(Content)"/>.
        /// </remarks>
        public ContentArray(JavaArrayList<Content> other) : base(other) { }

        /// <inheritdoc cref="JavaArrayList{T}(AndroidJavaObject, int)"/>
        internal ContentArray(AndroidJavaObject native, int size) : base(native, size) { }

        /// <inheritdoc/>
        protected override Content ElementFactory(AndroidJavaObject native) => new(native);

        /// <inheritdoc/>
        protected override Content ElementFactory(Content other) => new(other);
        
        public static implicit operator ContentArray(Content[] current) => new(current, true);
        public static implicit operator ContentArray(List<Content> current) => new(current, true);
    }
}