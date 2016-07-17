using System;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    /// <summary>
    /// Base class to build light shader groups with <see cref="LightGroupRendererDynamic"/>.
    /// </summary>
    public abstract class LightShaderGroupDynamic : LightShaderGroup
    {
        protected GraphicsProfile GraphicsProfile;

        /// <summary>
        /// List of all available lights.
        /// </summary>
        protected FastListStruct<LightDynamicEntry> Lights = new FastListStruct<LightDynamicEntry>(8);

        protected LightRange[] LightRanges;

        /// <summary>
        /// List of lights selected for this rendering.
        /// </summary>
        protected FastListStruct<LightDynamicEntry> CurrentLights = new FastListStruct<LightDynamicEntry>(8);

        public ILightShadowMapShaderGroupData ShadowGroup { get; }

        public int LightCurrentCount { get; private set; }

        public int LightLastCount { get; private set; }

        protected LightShaderGroupDynamic(RenderContext renderContext, ILightShadowMapShaderGroupData shadowGroup)
        {
            GraphicsProfile = renderContext.GraphicsDevice.Features.RequestedProfile;
            ShadowGroup = shadowGroup;
        }

        /// <inheritdoc/>
        public override void Reset()
        {
            base.Reset();
            Lights.Clear();
            LightLastCount = LightCurrentCount;
            LightCurrentCount = 0;
        }

        public virtual void SetViews(FastList<RenderView> views)
        {
            Array.Resize(ref LightRanges, views.Count);

            // Reset ranges
            for (var i = 0; i < views.Count; ++i)
                LightRanges[i] = new LightRange(0, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewIndex"></param>
        /// <param name="renderView"></param>
        /// <param name="lightCount"></param>
        /// <returns>The number of lights accepted in <see cref="CurrentLights"/>.</returns>
        public virtual int AddView(int viewIndex, RenderView renderView, int lightCount)
        {
            LightRanges[viewIndex] = new LightRange(Lights.Count, Lights.Count + lightCount);
            LightCurrentCount = Math.Max(LightCurrentCount, ComputeLightCount(lightCount));

            return Math.Min(LightCurrentCount, lightCount);
        }

        /// <summary>
        /// Compute the number of light supported by this shader. Usually a different number of light will trigger a permutation and layout update.
        /// </summary>
        /// <param name="lightCount"></param>
        /// <returns></returns>
        protected virtual int ComputeLightCount(int lightCount)
        {
            // Shadows: return exact number
            // TODO: Only for PerView; PerDraw could be little bit more loose to avoid extra permutations
            if (ShadowGroup != null)
            {
                return lightCount;
            }

            // Use next power of two
            lightCount = MathUtil.NextPowerOfTwo(lightCount);

            // Make sure it is at least 8 to avoid unecessary permutations
            lightCount = Math.Max(lightCount, GraphicsProfile >= GraphicsProfile.Level_10_0 ? 8 : 0);

            return lightCount;
        }

        /// <summary>
        /// Try to add light to this group (returns false if not possible).
        /// </summary>
        /// <param name="light"></param>
        /// <param name="shadowMapTexture"></param>
        /// <returns></returns>
        public bool AddLight(LightComponent light, LightShadowMapTexture shadowMapTexture)
        {
            Lights.Add(new LightDynamicEntry(light, shadowMapTexture));
            return true;
        }

        /// <inheritdoc/>
        public override void UpdateLayout(string compositionName)
        {
            base.UpdateLayout(compositionName);
            ShadowGroup?.UpdateLayout(compositionName);

            if (LightLastCount != LightCurrentCount)
            {
                ShadowGroup?.UpdateLightCount(LightLastCount, LightCurrentCount);
                UpdateLightCount();
            }
        }

        protected virtual void UpdateLightCount()
        {

        }

        /// <inheritdoc/>
        public override void ApplyViewParameters(RenderDrawContext context, int viewIndex, ParameterCollection parameters)
        {
            base.ApplyViewParameters(context, viewIndex, parameters);
            ShadowGroup?.ApplyViewParameters(context, parameters, CurrentLights);
        }

        /// <inheritdoc/>
        public override void ApplyDrawParameters(RenderDrawContext context, int viewIndex, ParameterCollection parameters, ref BoundingBoxExt boundingBox)
        {
            base.ApplyDrawParameters(context, viewIndex, parameters, ref boundingBox);
            ShadowGroup?.ApplyDrawParameters(context, parameters, CurrentLights, ref boundingBox);
        }

        public struct LightRange
        {
            public readonly int Start;
            public readonly int End;

            public LightRange(int start, int end)
            {
                Start = start;
                End = end;
            }

            public override string ToString()
            {
                return $"LightRange {Start}..{End}";
            }
        }
    }
}