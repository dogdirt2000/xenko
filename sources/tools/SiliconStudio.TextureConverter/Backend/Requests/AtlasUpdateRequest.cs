﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request to update a texture from an atlas
    /// </summary>
    class AtlasUpdateRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.AtlasUpdate; } }

        /// <summary>
        /// The name of the texture to replace in the atlas.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// The new texture.
        /// </summary>
        public TexImage Texture { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="AtlasUpdateRequest"/> class.
        /// </summary>
        /// <param name="texture">The new texture.</param>
        /// <param name="name">The name of the texture to replace.</param>
        public AtlasUpdateRequest(TexImage texture, string name)
        {
            Texture = texture;
            Name = name;
        }
    }
}
