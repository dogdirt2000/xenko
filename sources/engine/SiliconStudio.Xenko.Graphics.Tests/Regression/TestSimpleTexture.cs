﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;

namespace SiliconStudio.Xenko.Graphics.Tests.Regression
{
    [TestFixture]
    public class TestSimpleTexture : GameTestBase
    {
        /// <summary>
        /// The texture.
        /// </summary>
        private Texture texture;
        
        public TestSimpleTexture()
        {
            CurrentVersion = 1;
            GraphicsDeviceManager.PreferredBackBufferWidth = 256;
            GraphicsDeviceManager.PreferredBackBufferHeight = 256;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            Console.WriteLine(@"Begin load.");
            texture = Content.Load<Texture>("small_uv");
            Console.WriteLine(@"End load.");
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.Draw(DrawTexture).TakeScreenshot();
        }

        public void DrawTexture()
        {
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.BackBuffer, Color.Black);
            GraphicsContext.CommandList.Clear(GraphicsDevice.Presenter.DepthStencilBuffer, DepthStencilClearOptions.DepthBuffer);
            GraphicsContext.CommandList.SetRenderTargetAndViewport(GraphicsDevice.Presenter.DepthStencilBuffer, GraphicsDevice.Presenter.BackBuffer);
            GraphicsContext.DrawTexture(texture, GraphicsDevice.SamplerStates.PointClamp);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            DrawTexture();
        }

        /// <summary>
        /// Run the test
        /// </summary>
        [Test]
        public void RunTestSimpleTexture()
        {
            RunGameTest(new TestSimpleTexture());
        }
    }
}
