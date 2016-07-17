﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    public abstract class AssetImporterBase : IAssetImporter
    {
        public abstract Guid Id { get; }

        public virtual string Name => GetType().Name;

        public abstract string Description { get; }

        public int Order { get; protected set; }

        public abstract string SupportedFileExtensions { get; }

        public virtual bool IsSupportingFile(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            var file = new UFile(filePath);
            if (file.GetFileExtension() == null) return false;

            return FileUtility.GetFileExtensionsAsSet(SupportedFileExtensions).Contains(file.GetFileExtension());
        }

        public abstract IEnumerable<Type> RootAssetTypes { get; }

        public virtual IEnumerable<Type> AdditionalAssetTypes { get { yield break; } }

        public AssetImporterParameters GetDefaultParameters(bool isForReImport)
        {
            return new AssetImporterParameters(RootAssetTypes.Concat(AdditionalAssetTypes));
        }

        public abstract IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters);
    }
}