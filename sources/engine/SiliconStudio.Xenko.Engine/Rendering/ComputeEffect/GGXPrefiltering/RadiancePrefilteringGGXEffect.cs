﻿// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Xenko Shader Mixin Code Generator.
// To generate it yourself, please install SiliconStudio.Xenko.VisualStudio.Package .vsix
// and re-save the associated .xkfx.
// </auto-generated>

using System;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Xenko.Graphics.Buffer;

namespace SiliconStudio.Xenko.Rendering.Images
{
    [DataContract]public partial class RadiancePrefilteringGGXParams : ShaderMixinParameters
    {
        public static readonly ParameterKey<int> NbOfSamplings = ParameterKeys.New<int>();
    };
    internal static partial class ShaderMixins
    {
        internal partial class RadiancePrefilteringGGXEffect  : IShaderMixinBuilder
        {
            public void Generate(ShaderMixinSource mixin, ShaderMixinContext context)
            {
                context.Mixin(mixin, "RadiancePrefilteringGGXShader", context.GetParam(RadiancePrefilteringGGXParams.NbOfSamplings));
            }

            [ModuleInitializer]
            internal static void __Initialize__()

            {
                ShaderMixinManager.Register("RadiancePrefilteringGGXEffect", new RadiancePrefilteringGGXEffect());
            }
        }
    }
}