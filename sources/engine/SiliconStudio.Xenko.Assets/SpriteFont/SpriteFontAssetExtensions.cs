﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.IO;
using SiliconStudio.Assets;
using SiliconStudio.Core.IO;
using SiliconStudio.Xenko.Assets.SpriteFont.Compiler;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Graphics.Font;
using Glyph = SiliconStudio.Xenko.Graphics.Font.Glyph;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    public static class SpriteFontAssetExtensions
    {
        private static readonly FontDataFactory FontDataFactory = new FontDataFactory();

        /// <summary>
        /// Generate a precompiled sprite font from the current sprite font asset.
        /// </summary>
        /// <param name="asset">The sprite font asset</param>
        /// <param name="sourceAsset">The source sprite font asset item</param>
        /// <param name="texturePath">The path of the source texture</param>
        /// <param name="srgb">Indicate if the generated texture should be srgb</param>
        /// <returns>The precompiled sprite font asset</returns>
        public static PrecompiledSpriteFontAsset GeneratePrecompiledSpriteFont(this SpriteFontAsset asset, AssetItem sourceAsset, string texturePath, bool srgb)
        {
            var staticFont = (OfflineRasterizedSpriteFont)OfflineRasterizedFontCompiler.Compile(FontDataFactory, asset, srgb);

            var referenceToSourceFont = new AssetReference<SpriteFontAsset>(sourceAsset.Id, sourceAsset.Location);
            var glyphs = new List<Glyph>(staticFont.CharacterToGlyph.Values);
            var textures = staticFont.Textures;
            
            var imageType = ImageFileType.Png;
            var textureFileName = new UFile(texturePath).GetFullPathWithoutExtension() + imageType.ToFileExtension();

            if (textures != null && textures.Count > 0)
            {
                // save the texture   TODO support for multi-texture
                using (var stream = File.OpenWrite(textureFileName))
                    staticFont.Textures[0].GetSerializationData().Save(stream, imageType);
            }

            var precompiledAsset = new PrecompiledSpriteFontAsset
            {
                Glyphs = glyphs,
                Size = asset.FontType.Size,
                Style = asset.FontSource.Style,
                OriginalFont = referenceToSourceFont,
                FontDataFile = textureFileName,
                BaseOffset = staticFont.BaseOffsetY,
                DefaultLineSpacing = staticFont.DefaultLineSpacing,
                ExtraSpacing = staticFont.ExtraSpacing,
                ExtraLineSpacing = staticFont.ExtraLineSpacing,
                DefaultCharacter = asset.DefaultCharacter,
                FontName = asset.FontSource.GetFontName(),
                IsPremultiplied = asset.FontType.IsPremultiplied,
                IsSrgb = srgb
            };

            return precompiledAsset;
        }


        /// <summary>
        /// Generate a precompiled sprite font from the current sprite font asset.
        /// </summary>
        /// <param name="asset">The sprite font asset</param>
        /// <param name="sourceAsset">The source sprite font asset item</param>
        /// <param name="texturePath">The path of the source texture</param>
        /// <param name="srgb">Indicate if the generated texture should be srgb</param>
        /// <returns>The precompiled sprite font asset</returns>
        public static PrecompiledSpriteFontAsset GeneratePrecompiledSDFSpriteFont(this SpriteFontAsset asset, AssetItem sourceAsset, string texturePath)
        {
            // TODO create PrecompiledSDFSpriteFontAsset
            var scalableFont = (SignedDistanceFieldSpriteFont)SignedDistanceFieldFontCompiler.Compile(FontDataFactory, asset);

            var referenceToSourceFont = new AssetReference<SpriteFontAsset>(sourceAsset.Id, sourceAsset.Location);
            var glyphs = new List<Glyph>(scalableFont.CharacterToGlyph.Values);
            var textures = scalableFont.Textures;

            var imageType = ImageFileType.Png;
            var textureFileName = new UFile(texturePath).GetFullPathWithoutExtension() + imageType.ToFileExtension();

            if (textures != null && textures.Count > 0)
            {
                // save the texture   TODO support for multi-texture
                using (var stream = File.OpenWrite(textureFileName))
                    scalableFont.Textures[0].GetSerializationData().Save(stream, imageType);
            }

            var precompiledAsset = new PrecompiledSpriteFontAsset
            {
                Glyphs = glyphs,
                Size = asset.FontType.Size,
                Style = asset.FontSource.Style,
                OriginalFont = referenceToSourceFont,
                FontDataFile = textureFileName,
                BaseOffset = scalableFont.BaseOffsetY,
                DefaultLineSpacing = scalableFont.DefaultLineSpacing,
                ExtraSpacing = scalableFont.ExtraSpacing,
                ExtraLineSpacing = scalableFont.ExtraLineSpacing,
                DefaultCharacter = asset.DefaultCharacter,
                FontName = asset.FontSource.GetFontName(),
                IsPremultiplied = asset.FontType.IsPremultiplied,
                IsSrgb = false,
            };

            return precompiledAsset;
        }
    }
}
