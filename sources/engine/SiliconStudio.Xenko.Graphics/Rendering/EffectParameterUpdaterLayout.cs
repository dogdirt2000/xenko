﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines a layout used by <see cref="EffectParameterUpdater"/> to update several <see cref="ResourceGroup"/> from a <see cref="ParameterCollection"/>.
    /// </summary>
    public class EffectParameterUpdaterLayout
    {
        internal ResourceGroupLayout[] ResourceGroupLayouts;

        internal DescriptorSetLayoutBuilder[] Layouts;
        internal ParameterCollectionLayout ParameterCollectionLayout = new ParameterCollectionLayout();

        public EffectParameterUpdaterLayout(GraphicsDevice graphicsDevice, Effect effect, DescriptorSetLayoutBuilder[] layouts)
        {
            Layouts = layouts;

            // Process constant buffers
            ResourceGroupLayouts = new ResourceGroupLayout[layouts.Length];
            for (int layoutIndex = 0; layoutIndex < layouts.Length; layoutIndex++)
            {
                var layout = layouts[layoutIndex];
                if (layout == null)
                    continue;

                ParameterCollectionLayout.ProcessResources(layout);

                EffectConstantBufferDescription cbuffer = null;

                for (int entryIndex = 0; entryIndex < layout.Entries.Count; ++entryIndex)
                {
                    var layoutEntry = layout.Entries[entryIndex];
                    if (layoutEntry.Class == EffectParameterClass.ConstantBuffer)
                    {
                        // For now we assume first cbuffer will be the main one
                        if (cbuffer == null)
                        {
                            cbuffer = effect.Bytecode.Reflection.ConstantBuffers.First(x => x.Name == layoutEntry.Key.Name);
                            ParameterCollectionLayout.ProcessConstantBuffer(cbuffer);
                        }
                    }
                }

                var resourceGroupDescription = new ResourceGroupDescription(layout, cbuffer);

                ResourceGroupLayouts[layoutIndex] = ResourceGroupLayout.New(graphicsDevice, resourceGroupDescription, effect.Bytecode);
            }
        }
    }
}