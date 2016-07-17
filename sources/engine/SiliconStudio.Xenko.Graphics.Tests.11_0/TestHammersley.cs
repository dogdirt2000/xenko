﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.ComputeEffect;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    /// <summary>
    /// Test class for Hammersley sampling shader
    /// </summary>
    public class TestHammersley : GraphicTestGameBase
    {
        private Texture output;

        private const int OutputSize = 512;

        private int samplesCount = 1024;
        private ComputeEffectShader renderHammersley;

        public TestHammersley()
        {
            GraphicsDeviceManager.PreferredBackBufferWidth = OutputSize;
            GraphicsDeviceManager.PreferredBackBufferHeight = OutputSize;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var context = RenderContext.GetShared(Services);
            renderHammersley = new ComputeEffectShader(context) { ShaderSourceName = "HammersleyTest" };

            output = Texture.New2D(GraphicsDevice, OutputSize, OutputSize, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.UnorderedAccess | TextureFlags.RenderTarget).DisposeBy(this);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.IsKeyPressed(Keys.NumPad1))
                samplesCount = Math.Max(1, samplesCount / 2);

            if (Input.IsKeyPressed(Keys.NumPad3))
                samplesCount = Math.Min(1024, samplesCount * 2);
        }

        protected override void Draw(GameTime gameTime)
        {
            var renderDrawContext = new RenderDrawContext(Services, RenderContext.GetShared(Services), GraphicsContext);

            GraphicsContext.CommandList.Clear(output, Color4.White);
            renderHammersley.ThreadGroupCounts = new Int3(samplesCount, 1, 1);
            renderHammersley.ThreadNumbers = new Int3(1);
            renderHammersley.Parameters.Set(HammersleyTestKeys.OutputTexture, output);
            renderHammersley.Parameters.Set(HammersleyTestKeys.SamplesCount, samplesCount);
            renderHammersley.Draw(renderDrawContext);

            GraphicsContext.DrawTexture(output);

            base.Draw(gameTime);
        }

        public static void Main()
        {
            using (var game = new TestHammersley())
                game.Run();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunImageLoad()
        {
            IgnoreGraphicPlatform(GraphicsPlatform.OpenGLES);

            RunGameTest(new TestHammersley());
        }
    }
}