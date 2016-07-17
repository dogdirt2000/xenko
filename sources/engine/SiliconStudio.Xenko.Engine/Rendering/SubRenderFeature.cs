using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders.Compiler;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A render feature used inside another one (i.e. <see cref="MeshRenderFeature.RenderFeatures"/>.
    /// </summary>
    public abstract class SubRenderFeature : RenderFeature
    {
        /// <summary>
        /// Gets root render feature
        /// </summary>
        protected RootRenderFeature RootRenderFeature;

        /// <summary>
        /// Attach this <see cref="SubRenderFeature"/> to a <see cref="RootRenderFeature"/>.
        /// </summary>
        /// <param name="rootRenderFeature"></param>
        internal void AttachRootRenderFeature(RootRenderFeature rootRenderFeature)
        {
            RootRenderFeature = rootRenderFeature;
            RenderSystem = rootRenderFeature.RenderSystem;
        }

        /// <summary>
        /// Do any changes required to the pipeline state.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="renderNodeReference"></param>
        /// <param name="renderNode"></param>
        /// <param name="renderObject"></param>
        /// <param name="pipelineState"></param>
        public virtual void ProcessPipelineState(RenderContext context, RenderNodeReference renderNodeReference, ref RenderNode renderNode, RenderObject renderObject, PipelineStateDescription pipelineState)
        {
        }
    }
}