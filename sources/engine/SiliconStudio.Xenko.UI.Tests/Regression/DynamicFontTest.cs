﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// Class for dynamic sized text rendering tests.
    /// </summary>
    public class DynamicFontTest : UITestGameBase
    {
        private ContentDecorator decorator;
        private TextBlock textBlock;

        public DynamicFontTest()
        {
            CurrentVersion = 6; // Font type, names & sizes changed slightly
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            textBlock = new TextBlock
                {
                    Font = Content.Load<SpriteFont>("HanSans13"), 
                    Text = "Simple Text - 簡単な文章。", 
                    TextColor = Color.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    SynchronousCharacterGeneration = true
                };

            decorator = new ContentDecorator
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                BackgroundImage = (SpriteFromTexture)new Sprite(Content.Load<Texture>("DumbWhite")),
                Content = textBlock
            };

            UIComponent.RootElement = decorator;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            const float ChangeFactor = 1.1f;
            const float ChangeFactorInverse = 1 / ChangeFactor;

            // change the size of the virtual resolution
            if (Input.IsKeyReleased(Keys.NumPad0))
                UIComponent.Resolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width / 2f, GraphicsDevice.Presenter.BackBuffer.Height / 2f, 400);
            if (Input.IsKeyReleased(Keys.NumPad1))
                UIComponent.Resolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 400);
            if (Input.IsKeyReleased(Keys.NumPad2))
                UIComponent.Resolution = new Vector3(2 * GraphicsDevice.Presenter.BackBuffer.Width, 2 * GraphicsDevice.Presenter.BackBuffer.Height, 400);
            if (Input.IsKeyReleased(Keys.Right))
                UIComponent.Resolution = new Vector3((ChangeFactor * UIComponent.Resolution.X), UIComponent.Resolution.Y, UIComponent.Resolution.Z);
            if (Input.IsKeyReleased(Keys.Left))
                UIComponent.Resolution = new Vector3((ChangeFactorInverse * UIComponent.Resolution.X), UIComponent.Resolution.Y, UIComponent.Resolution.Z);
            if (Input.IsKeyReleased(Keys.Up))
                UIComponent.Resolution = new Vector3(UIComponent.Resolution.X, (ChangeFactor * UIComponent.Resolution.Y), UIComponent.Resolution.Z);
            if (Input.IsKeyReleased(Keys.Down))
                UIComponent.Resolution = new Vector3(UIComponent.Resolution.X, (ChangeFactorInverse * UIComponent.Resolution.Y), UIComponent.Resolution.Z);

            if (Input.IsKeyReleased(Keys.D1))
                decorator.LocalMatrix = Matrix.Scaling(1);
            if (Input.IsKeyReleased(Keys.D2))
                decorator.LocalMatrix = Matrix.Scaling(1.5f);
            if (Input.IsKeyReleased(Keys.D3))
                decorator.LocalMatrix = Matrix.Scaling(2);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.Draw(DrawTest0).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest1).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest2).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest3).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest4).TakeScreenshot();
            FrameGameSystem.Draw(DrawTest5).TakeScreenshot();
        }

        public void DrawTest0()
        {
            decorator.LocalMatrix = Matrix.Scaling(1);
            textBlock.TextSize = textBlock.Font.Size;
            UIComponent.Resolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 500);
        }

        public void DrawTest1()
        {
            decorator.LocalMatrix = Matrix.Scaling(1);
            textBlock.TextSize = 2*textBlock.Font.Size;
            UIComponent.Resolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 500);
        }

        public void DrawTest2()
        {
            decorator.LocalMatrix = Matrix.Scaling(1);
            textBlock.TextSize = textBlock.Font.Size;
            UIComponent.Resolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width / 2f, GraphicsDevice.Presenter.BackBuffer.Height / 2f, 500);
        }

        public void DrawTest3()
        {
            decorator.LocalMatrix = Matrix.Scaling(2);
            textBlock.TextSize = textBlock.Font.Size;
            UIComponent.Resolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height, 500);
        }

        public void DrawTest4()
        {
            decorator.LocalMatrix = Matrix.Scaling(1);
            textBlock.TextSize = textBlock.Font.Size;
            UIComponent.Resolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width / 2f, GraphicsDevice.Presenter.BackBuffer.Height, 500);
        }

        public void DrawTest5()
        {
            decorator.LocalMatrix = Matrix.Scaling(1);
            textBlock.TextSize = textBlock.Font.Size;
            UIComponent.Resolution = new Vector3(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height / 2f, 500);
        }

        [Test]
        public void RunDynamicFontTest()
        {
            RunGameTest(new DynamicFontTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new DynamicFontTest())
                game.Run();
        }
    }
}
