﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// The reflection data describing the parameters of a shader.
    /// </summary>
    [DataContract]
    public class EffectReflection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EffectReflection"/> class.
        /// </summary>
        public EffectReflection()
        {
            SamplerStates = new List<EffectSamplerStateBinding>();
            ResourceBindings = new FastList<EffectResourceBindingDescription>();
            ConstantBuffers = new List<EffectConstantBufferDescription>();
            ShaderStreamOutputDeclarations = new List<ShaderStreamOutputDeclarationEntry>();
        }

        /// <summary>
        /// Gets or sets the sampler states.
        /// </summary>
        /// <value>The sampler states.</value>
        public List<EffectSamplerStateBinding> SamplerStates { get; set; }

        /// <summary>
        /// Gets the parameter binding descriptions.
        /// </summary>
        /// <value>The resource bindings.</value>
        public FastList<EffectResourceBindingDescription> ResourceBindings { get; set; }

        /// <summary>
        /// Gets the constant buffer descriptions (if any).
        /// </summary>
        /// <value>The constant buffers.</value>
        public List<EffectConstantBufferDescription> ConstantBuffers { get; set; }

        /// <summary>
        /// Gets or sets the stream output declarations.
        /// </summary>
        /// <value>The stream output declarations.</value>
        public List<ShaderStreamOutputDeclarationEntry> ShaderStreamOutputDeclarations { get; set; }

        /// <summary>
        /// Gets or sets the stream output strides.
        /// </summary>
        /// <value>The stream output strides.</value>
        public int[] StreamOutputStrides { get; set; }

        /// <summary>
        /// Gets or sets the stream output rasterized stream.
        /// </summary>
        /// <value>The stream output rasterized stream.</value>
        public int StreamOutputRasterizedStream { get; set; }
    }
}