// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// An array of <see cref="ShaderSource"/> used only in shader mixin compositions.
    /// </summary>
    [DataContract("ShaderArraySource")]
    public sealed class ShaderArraySource : ShaderSource, IEnumerable<ShaderSource>, IEquatable<ShaderArraySource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderArraySource"/> class.
        /// </summary>
        public ShaderArraySource()
        {
            Values = new ShaderSourceCollection();
        }

        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        /// <value>The values.</value>
        public ShaderSourceCollection Values { get; set; }

        /// <summary>
        /// Adds the specified composition.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public void Add(ShaderSource composition)
        {
            Values.Add(composition);
        }

        public override object Clone()
        {
            return new ShaderArraySource { Values = new ShaderSourceCollection(Values.Select(x => (ShaderSource)x.Clone())) };
        }

        public IEnumerator<ShaderSource> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return string.Format("[{0}]", Values != null ? string.Join(", ", Values) : string.Empty);
        }

        public bool Equals(ShaderArraySource other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Values.Equals(other.Values);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ShaderArraySource && Equals((ShaderArraySource)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 0;
                if (Values != null)
                {
                    foreach (var current in Values)
                        hashCode = (hashCode*397) ^ (current?.GetHashCode() ?? 0);
                }
                return hashCode;
            }
        }

        public static bool operator ==(ShaderArraySource left, ShaderArraySource right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ShaderArraySource left, ShaderArraySource right)
        {
            return !Equals(left, right);
        }
    }
}