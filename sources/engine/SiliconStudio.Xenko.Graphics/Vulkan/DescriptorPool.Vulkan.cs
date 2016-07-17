﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN && !SILICONSTUDIO_XENKO_GRAPHICS_NO_DESCRIPTOR_COPIES
using System;
using SharpVulkan;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class DescriptorPool
    {
        private uint[] allocatedTypeCounts;
        private uint allocatedSetCount;

        internal SharpVulkan.DescriptorPool NativeDescriptorPool;

        public void Reset()
        {
            GraphicsDevice.descriptorPools.RecycleObject(GraphicsDevice.NextFenceValue, NativeDescriptorPool);
            NativeDescriptorPool = GraphicsDevice.descriptorPools.GetObject();

            allocatedSetCount = 0;
            for (int i = 0; i < DescriptorSetLayout.DescriptorTypeCount; i++)
            {
                allocatedTypeCounts[i] = 0;
            }
        }

        private DescriptorPool(GraphicsDevice graphicsDevice, DescriptorTypeCount[] counts) : base(graphicsDevice)
        {
            Recreate();
        }

        internal unsafe SharpVulkan.DescriptorSet AllocateDescriptorSet(DescriptorSetLayout descriptorSetLayout)
        {
            // Keep track of descriptor pool usage
            bool isPoolExhausted = ++allocatedSetCount > GraphicsDevice.MaxDescriptorSetCount;
            for (int i = 0; i < DescriptorSetLayout.DescriptorTypeCount; i++)
            {
                allocatedTypeCounts[i] += descriptorSetLayout.TypeCounts[i];
                if (allocatedTypeCounts[i] > GraphicsDevice.MaxDescriptorTypeCounts[i])
                {
                    isPoolExhausted = true;
                    break;
                }
            }

            if (isPoolExhausted)
            {
                return SharpVulkan.DescriptorSet.Null;
            }

            // Allocate new descriptor set
            var nativeLayoutCopy = descriptorSetLayout.NativeLayout;
            var allocateInfo = new DescriptorSetAllocateInfo
            {
                StructureType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = NativeDescriptorPool,
                DescriptorSetCount = 1,
                SetLayouts = new IntPtr(&nativeLayoutCopy)
            };

            SharpVulkan.DescriptorSet descriptorSet;
            GraphicsDevice.NativeDevice.AllocateDescriptorSets(ref allocateInfo, &descriptorSet);
            return descriptorSet;
        }

        private void Recreate()
        {
            NativeDescriptorPool = GraphicsDevice.descriptorPools.GetObject();
            
            allocatedTypeCounts = new uint[DescriptorSetLayout.DescriptorTypeCount];
            allocatedSetCount = 0;
        }

        /// <inheritdoc/>
        protected internal override bool OnRecreate()
        {
            Recreate();
            return true;
        }

        /// <inheritdoc/>
        protected internal override void OnDestroyed()
        {
            GraphicsDevice.descriptorPools.RecycleObject(GraphicsDevice.NextFenceValue, NativeDescriptorPool);

            base.OnDestroyed();
        }
    }
}
#endif