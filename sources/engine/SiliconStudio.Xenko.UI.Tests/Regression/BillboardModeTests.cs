﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    /// <summary>
    /// Test for UI on scene entities
    /// </summary>
    public class BillboardModeTests : UITestGameBase
    {
        public BillboardModeTests()
        {
            CurrentVersion = 4;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            var cube = new Entity { new ModelComponent { Model = Content.Load<Model>("cube Model") } };
            cube.Transform.Scale = new Vector3(10000);
            cube.Transform.Position = new Vector3(0, 0, 10);
            Scene.Entities.Add(cube);

            var imageElement = new ImageElement { Source = (SpriteFromTexture)new Sprite(Content.Load<Texture>("uv")) };
            var imageEntity = new Entity { new UIComponent { RootElement = imageElement, IsFullScreen = false, Resolution = new Vector3(150) } };
            imageEntity.Transform.Scale = new Vector3(150);
            imageEntity.Transform.Position = new Vector3(-500, 0, 0);
            Scene.Entities.Add(imageEntity);

            var imageEntity2 = new Entity { new UIComponent { RootElement = imageElement, IsFullScreen = false, Resolution = new Vector3(200) } };
            imageEntity2.Transform.Position = new Vector3(0, 250, 0);
            imageEntity2.Transform.Scale = new Vector3(200);
            Scene.Entities.Add(imageEntity2);

            var imageEntity3 = new Entity { new UIComponent { RootElement = imageElement, IsFullScreen = false, Resolution = new Vector3(250) } };
            imageEntity3.Transform.Position = new Vector3(0, 0, -500);
            imageEntity3.Transform.Scale = new Vector3(250);
            Scene.Entities.Add(imageEntity3);
            
            // setup the camera
            var camera = new TestCamera { Yaw = MathUtil.Pi/4, Pitch = MathUtil.Pi/4, Position = new Vector3(500, 500, 500), MoveSpeed = 100 };
            camera.SetTarget(cube, true);
            CameraComponent = camera.Camera;
            Script.Add(camera);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
        }

        [Test]
        public void RunBillboardModeTests()
        {
            RunGameTest(new BillboardModeTests());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new BillboardModeTests())
                game.Run();
        }
    }
}
