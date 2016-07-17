﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Animations
{
    public enum AnimationKeyTangentType
    {
        /// <summary>
        /// Linear - the value is linearly calculated as V1 * (1 - t) + V2 * t
        /// </summary>
        Linear,        
    }

    [DataContract]
    [Display("KeyFrame")]
    public class AnimationKeyFrame<T> where T : struct
    {
        private T val;
        [DataMember(20)]
        [Display("Value")]
        public T Value { get { return val; } set { val = value; HasChanged = true; } }

        private float key;
        [DataMember(10)]
        [Display("Key")]
        public float Key { get { return key; } set { key = value; HasChanged = true; } }

        private AnimationKeyTangentType tangentType = AnimationKeyTangentType.Linear;
        [DataMember(30)]
        [Display("Tangent")]
        public AnimationKeyTangentType TangentType { get { return tangentType; } set { tangentType = value; HasChanged = true; } }

        [DataMemberIgnore]
        public bool HasChanged = true;
    }

    /// <summary>
    /// A node which describes a binary operation between two compute curves
    /// </summary>
    /// <typeparam name="T">Sampled data's type</typeparam>
    [DataContract(Inherited = true)]
    [Display("Animation", Expand = ExpandRule.Never)]
    public abstract class ComputeAnimationCurve<T> : Comparer<AnimationKeyFrame<T>>, IComputeCurve<T>  where T : struct
    {
        // TODO This class will hold an AnimationCurve<T> later
        //[DataMemberIgnore]
        //public AnimationCurve<T> Animation { get; set; } = new AnimationCurve<T>();
        
        [NotNullItems]
        public TrackingCollection<AnimationKeyFrame<T>> KeyFrames { get; set; } = new TrackingCollection<AnimationKeyFrame<T>>();

        // TODO This list will become AnimationCurve<T>
        private FastList<AnimationKeyFrame<T>> sortedKeys = new FastList<AnimationKeyFrame<T>>(); 

        private int framesCount = 0;
        private bool HasChanged()
        {
            if (framesCount != KeyFrames.Count)
                return true;

            for (var i = 0; i < framesCount; i++)
                if (KeyFrames[i].HasChanged)
                    return true;

            return false;
        }

        /// <inheritdoc/>
        public bool UpdateChanges()
        {
            if (!HasChanged())
                return false;

            sortedKeys.Clear();
            sortedKeys.AddRange(KeyFrames.ToArray());
            sortedKeys.Sort(this);

            framesCount = KeyFrames.Count;
            for (var i = 0; i < framesCount; i++)
                KeyFrames[i].HasChanged = false;
            return true;
        }

        /// <inheritdoc/>
        public override int Compare(AnimationKeyFrame<T> x, AnimationKeyFrame<T> y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return  1;

            return (x.Key < y.Key) ? -1 : (x.Key > y.Key) ? 1 : 0;
        }

        public abstract void Cubic(ref T value1, ref T value2, ref T value3, ref T value4, float t, out T result);

        public abstract void Linear(ref T value1, ref T value2, float t, out T result);

        /// <summary>
        /// Unoptimized sampler which searches all the keyframes in order. Intended to be used for baking purposes only
        /// </summary>
        /// <param name="t">Location t to sample at, between 0 and 1</param>
        /// <returns>Sampled and interpolated data value</returns>
        protected T SampleRaw(float t)
        {
            if (sortedKeys.Count <= 0)
            {
                UpdateChanges();

                if (sortedKeys.Count <= 0)
                    return new T();
            }

            var leftIndex = 0;
            while ((leftIndex < sortedKeys.Count - 1) && (sortedKeys[leftIndex + 1].Key <= t))
                leftIndex++;

            if ((leftIndex >= sortedKeys.Count - 1) || (sortedKeys[leftIndex].Key >= t))
                return sortedKeys[leftIndex].Value;

            var rightIndex = leftIndex + 1;
            if (sortedKeys[leftIndex].Key >= sortedKeys[rightIndex].Key)
                return sortedKeys[leftIndex].Value;

            // Lerp between the two values
            var lerpValue = (t - sortedKeys[leftIndex].Key) / (sortedKeys[rightIndex].Key - sortedKeys[leftIndex].Key);
            T result;

            var leftValue = sortedKeys[leftIndex].Value;
            var rightValue = sortedKeys[rightIndex].Value;

            switch (sortedKeys[leftIndex].TangentType)
            {
                case AnimationKeyTangentType.Linear:
                    Linear(ref leftValue, ref rightValue, lerpValue, out result);
                    break;

                default:
                    result = leftValue;
                    break;
            }
            return result;
        }

        /// <inheritdoc/>
        public T Evaluate(float location)
        {
            return SampleRaw(location);
        }
    }
}
