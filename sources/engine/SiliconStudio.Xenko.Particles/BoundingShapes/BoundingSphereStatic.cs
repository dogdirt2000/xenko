﻿// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.DebugDraw;

namespace SiliconStudio.Xenko.Particles.BoundingShapes
{
    [DataContract("BoundingSpheretatic")]
    [Display("Uniform AABB")]
    public class BoundingSphereStatic : BoundingShape
    {
        /// <summary>
        /// Fixed radius of the <see cref="BoundingSphereStatic"/>
        /// </summary>
        /// <userdoc>
        /// Fixed radius of the bounding sphere. Gets calculated as a AABB, which is a cube with corners (-R, -R, -R) - (+R, +R, +R)
        /// </userdoc>
        [DataMember(20)]
        [Display("Distance")]
        public float Radius { get; set; } = 1f;

        [DataMemberIgnore]
        private BoundingBox cachedBox;
        
        public override BoundingBox GetAABB(Vector3 translation, Quaternion rotation, float scale)
        {
            if (Dirty)
            {
                var r = Radius*scale;

                cachedBox = new BoundingBox(new Vector3(-r, -r, -r) + translation, new Vector3(r, r, r) + translation);
            }

            return cachedBox;
        }

        public override bool TryGetDebugDrawShape(out DebugDrawShape debugDrawShape, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
        {
            if (!DebugDraw)
                return base.TryGetDebugDrawShape(out debugDrawShape, out translation, out rotation, out scale);

            debugDrawShape = DebugDrawShape.Cube;
            scale = new Vector3(Radius, Radius, Radius);
            translation = new Vector3(0, 0, 0);
            rotation = Quaternion.Identity;
            return true;
        }

    }
}
