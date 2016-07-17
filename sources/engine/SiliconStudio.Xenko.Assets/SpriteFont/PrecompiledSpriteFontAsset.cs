﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Graphics.Font;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    [DataContract("PregeneratedSpriteFont")]
    [AssetDescription(FileExtension)]
    [AssetCompiler(typeof(PrecompiledSpriteFontAssetCompiler))]
    [AssetFormatVersion(XenkoConfig.PackageName, "1.7.0-alpha01")]
    [AssetUpgrader(XenkoConfig.PackageName, "0.0.0", "1.5.0-alpha09", typeof(PremultiplyUpgrader))]
    [AssetUpgrader(XenkoConfig.PackageName, "1.5.0-alpha09", "1.7.0-alpha01", typeof(SourceMembersUpgrader))]
    [Display(105, "Sprite Font (Precompiled)")]
    [CategoryOrder(10, "Font Data")]
    [CategoryOrder(20, "Font Properties")]
    [CategoryOrder(30, "Font Characters")]
    public class PrecompiledSpriteFontAsset : Asset
    {
        /// <summary>
        /// The default file extension used by the <see cref="PrecompiledSpriteFontAsset"/>.
        /// </summary>
        public const string FileExtension = ".xkpcfnt";

        [Display(Browsable = false)]
        public string FontName; // Note: this field is used only for thumbnail.

        [DefaultValue(FontStyle.Regular)]
        [Display(Browsable = false)]
        public FontStyle Style; // Note: this field is used only for thumbnail.

        [DefaultValue(true)]
        [Display(Browsable = false)]
        public bool IsPremultiplied = true; // Note: this field is used only for thumbnail / preview.

        /// <summary>
        /// The size in points (pt).
        /// </summary>
        [DefaultValue(16)]
        [Display(Browsable = false)]
        public float Size = 16; // a random non-null value for font created by users

        /// <summary>
        /// The reference to the original source asset.
        /// </summary>
        /// <userdoc>The sprite font asset that has been used to generate this precompiled font.</userdoc>
        [DataMember(0)]
        public AssetReference<SpriteFontAsset> OriginalFont;

        /// <summary>
        /// The file containing the font data.
        /// </summary>
        /// <userdoc>The image file containing the extracted font data.</userdoc>
        [Display("Font data image", "Font Data")]
        [DataMember(10)]
        [SourceFileMember(false)]
        public UFile FontDataFile;

        /// <summary>
        /// Indicate if the font data in stored in sRGB mode.
        /// </summary>
        /// <userdoc>If checked the font data contained in the source image is considered as sRGB.</userdoc>
        [Display("sRGB", "Font Data")]
        [DataMember(20)]
        [DefaultValue(true)]
        public bool IsSrgb = true;

        /// <summary>
        /// The default character of the font.
        /// </summary>
        /// <userdoc>The fallback character when trying to draw a character not existing in the font.</userdoc>
        [Display("Fallback Character", "Font Properties")]
        [DefaultValue(' ')]
        [DataMember(30)]
        public char DefaultCharacter = ' ';

        /// <summary>
        /// The base offset of the font.
        /// </summary>
        /// <userdoc>The position of the base line of the font with respect to the glyphs top pixel (in pixels).</userdoc>
        [Display("Base Offset", "Font Properties")]
        [DataMember(40)]
        [DefaultValue(0)]
        public float BaseOffset;

        /// <summary>
        /// The space between two lines
        /// </summary>
        /// <userdoc>The space between two lines in pixels.</userdoc>
        [Display("Line Spacing", "Font Properties")]
        [DataMember(50)]
        [DefaultValue(10)]
        public float DefaultLineSpacing = 10;

        /// <summary>
        /// The extra horizontal spacing of the font.
        /// </summary>
        /// <userdoc>The extra horizontal spacing between characters.</userdoc>
        [Display("Extra Spacing", "Font Properties")]
        [DefaultValue(0)]
        [DataMember(60)]
        public float ExtraSpacing;

        /// <summary>
        /// The extra vertical spacing between two lines.
        /// </summary>
        [Display("Extra Line Spacing", "Font Properties")]
        [DefaultValue(0)]
        [DataMember(70)]
        public float ExtraLineSpacing;

        /// <summary>
        /// The font glyph information
        /// </summary>
        /// <userdoc>The glyph information specifying the position of the characters in the data image.</userdoc>
        [Display("Glyphs", "Font Characters")]
        [DataMember(80)]
        [NotNullItems]
        public List<Glyph> Glyphs = new List<Glyph>();

        /// <summary>
        /// The kerning information.
        /// </summary>
        /// <userdoc>The kerning information</userdoc>
        [Display("Kernings", "Font Characters")]
        [DataMember(90)]
        public List<Kerning> Kernings = new List<Kerning>();

        class PremultiplyUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.IsNotPremultiplied != null)
                {
                    asset.IsPremultiplied = !(bool)asset.IsNotPremultiplied;
                    asset.IsNotPremultiplied = DynamicYamlEmpty.Default;
                }
            }
        }

        class SourceMembersUpgrader : AssetUpgraderBase
        {
            protected override void UpgradeAsset(AssetMigrationContext context, PackageVersion currentVersion, PackageVersion targetVersion, dynamic asset, PackageLoadingAssetFile assetFile, OverrideUpgraderHint overrideHint)
            {
                if (asset.Source != null)
                {
                    asset.OriginalFont = asset.Source;
                    asset.Source = DynamicYamlEmpty.Default;
                }
            }
        }
    }
}
