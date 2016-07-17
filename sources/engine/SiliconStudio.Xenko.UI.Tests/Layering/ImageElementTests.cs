﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI.Controls;

namespace SiliconStudio.Xenko.UI.Tests.Layering
{
    /// <summary>
    /// A class that contains test functions for layering of the <see cref="ImageElement"/> class.
    /// </summary>
    [TestFixture]
    [System.ComponentModel.Description("Tests for ImageElement layering")]
    public class ImageElementTests : ImageElement
    {
        /// <summary>
        /// Test the invalidations generated object property changes.
        /// </summary>
        [Test]
        public void TestBasicInvalidations()
        {
            var source = new Sprite();

            // ReSharper disable ImplicitlyCapturedClosure

            // - test the properties that are supposed to invalidate the object measurement
            UIElementLayeringTests.TestMeasureInvalidation(this, () => StretchType = StretchType.None);
            UIElementLayeringTests.TestMeasureInvalidation(this, () => StretchDirection = StretchDirection.DownOnly);
            UIElementLayeringTests.TestMeasureInvalidation(this, () => Source = (SpriteFromTexture)source);
            UIElementLayeringTests.TestMeasureInvalidation(this, () => source.Region = new Rectangle(1, 2, 3, 4));
            UIElementLayeringTests.TestMeasureInvalidation(this, () => source.Orientation = ImageOrientation.Rotated90);
            UIElementLayeringTests.TestMeasureInvalidation(this, () => source.Borders = Vector4.One);

            // - test the properties that are not supposed to invalidate the object layout state
            UIElementLayeringTests.TestNoInvalidation(this, () => source.Region = new Rectangle(8, 9, 3, 4)); // if the size of the region does not change we avoid re-measuring
            UIElementLayeringTests.TestNoInvalidation(this, () => source.Orientation = ImageOrientation.Rotated90); // no changes
            UIElementLayeringTests.TestNoInvalidation(this, () => source.Borders = Vector4.One); // no changes

            // ReSharper restore ImplicitlyCapturedClosure
        }

        /// <summary>
        /// Test the <see cref="UIElement.MeasureOverride"/> function
        /// </summary>
        [Test]
        public void TestMeasureOverride()
        {
            var rand = new Random();
            var imageSize = new Vector3(100, 50, 0);
            var sprite = new Sprite { Region = new Rectangle(0, 0, (int)imageSize.X, (int)imageSize.Y), Borders = new Vector4(1, 2, 3, 4) };
            var image = new ImageElement { Source = (SpriteFromTexture)sprite };

            // Fixed sized
            image.StretchType = StretchType.None;
            image.Measure(rand.NextVector3());
            Assert.AreEqual(imageSize, image.DesiredSizeWithMargins);

            // Uniform sized
            image.StretchType = StretchType.Uniform;
            image.Measure(new Vector3(50));
            Assert.AreEqual(new Vector3(50, 25, 0), image.DesiredSizeWithMargins);

            // Uniform to fill sized
            image.StretchType = StretchType.UniformToFill;
            image.Measure(new Vector3(50));
            Assert.AreEqual(new Vector3(100, 50, 0), image.DesiredSizeWithMargins);

            // Fill on stretch
            image.StretchType = StretchType.FillOnStretch;
            image.Measure(new Vector3(50));
            Assert.AreEqual(new Vector3(50, 25, 0), image.DesiredSizeWithMargins);

            // Fill
            image.StretchType = StretchType.Fill;
            image.Measure(new Vector3(50));
            Assert.AreEqual(new Vector3(50, 50, 0), image.DesiredSizeWithMargins);

            // Test minimal size due to borders
            image.StretchType = StretchType.Fill;
            image.Measure(new Vector3());
            Assert.AreEqual(new Vector3(4, 6, 0), image.DesiredSizeWithMargins);

            // Test with infinite value
            for (var type = 0; type < 5; ++type)
                TestMeasureOverrideInfiniteValues((StretchType)type);

            // Test stretch directions
            image.StretchType = StretchType.Fill;
            image.StretchDirection = StretchDirection.DownOnly;
            image.Measure(new Vector3(200, 300, 220));
            Assert.AreEqual(new Vector3(100, 50, 0), image.DesiredSizeWithMargins);
            image.Measure(new Vector3(20, 15, 30));
            Assert.AreEqual(new Vector3(20, 15, 0), image.DesiredSizeWithMargins);
            image.StretchDirection = StretchDirection.UpOnly;
            image.Measure(new Vector3(200, 300, 220));
            Assert.AreEqual(new Vector3(200, 300, 0), image.DesiredSizeWithMargins);
            image.Measure(new Vector3(20, 30, 22));
            Assert.AreEqual(new Vector3(100, 50, 0), image.DesiredSizeWithMargins);
        }

        public void TestMeasureOverrideInfiniteValues(StretchType stretch)
        {
            var imageSize = new Vector3(100, 50, 0);
            var sprite = new Sprite { Region = new Rectangle(0, 0, (int)imageSize.X, (int)imageSize.Y), Borders = new Vector4(1, 2, 3, 4) };
            var image = new ImageElement { Source = (SpriteFromTexture)sprite, StretchType = stretch };
            
            image.Measure(new Vector3(float.PositiveInfinity));
            Assert.AreEqual(imageSize, image.DesiredSizeWithMargins);

            image.Measure(new Vector3(150, float.PositiveInfinity, 10));
            Assert.AreEqual(stretch == StretchType.None ? imageSize : new Vector3(150, 75, 0), image.DesiredSizeWithMargins);
        }

        /// <summary>
        /// Test the <see cref="UIElement.ArrangeOverride"/> function
        /// </summary>
        [Test]
        public void TestArrangeOverride()
        {
            var rand = new Random();
            var imageSize = new Vector3(100, 50, 0);
            var sprite = new Sprite { Region = new Rectangle(0, 0, (int)imageSize.X, (int)imageSize.Y), Borders = new Vector4(1, 2, 3, 4) };
            var image = new ImageElement { Source = (SpriteFromTexture)sprite };

            // Fixed sized
            image.StretchType = StretchType.None;
            image.Arrange(rand.NextVector3(), false);
            Assert.AreEqual(imageSize, image.RenderSize);

            // Uniform sized
            image.StretchType = StretchType.Uniform;
            image.Arrange(new Vector3(50), false);
            Assert.AreEqual(new Vector3(50, 25, 0), image.RenderSize);

            // Uniform to fill sized
            image.StretchType = StretchType.UniformToFill;
            image.Arrange(new Vector3(50), false);
            Assert.AreEqual(new Vector3(100, 50, 0), image.RenderSize);

            // Fill on stretch
            image.StretchType = StretchType.FillOnStretch;
            image.Arrange(new Vector3(50), false);
            Assert.AreEqual(new Vector3(50, 50, 0), image.RenderSize);

            // Fill
            image.StretchType = StretchType.Fill;
            image.Arrange(new Vector3(50), false);
            Assert.AreEqual(new Vector3(50, 50, 0), image.RenderSize);

            // Test there is no minimal size due to borders in arrange
            image.StretchType = StretchType.Fill;
            image.Arrange(new Vector3(), false);
            Assert.AreEqual(new Vector3(), image.RenderSize);

            // Test with infinite value
            for (var type = 0; type < 5; ++type)
                TestArrangeOverrideInfiniteValues((StretchType)type);

            // Test stretch directions
            image.StretchType = StretchType.Fill;
            image.StretchDirection = StretchDirection.DownOnly;
            image.Arrange(new Vector3(200, 300, 220), false);
            Assert.AreEqual(new Vector3(100, 50, 0), image.RenderSize);
            image.Arrange(new Vector3(20, 15, 30), false);
            Assert.AreEqual(new Vector3(20, 15, 0), image.RenderSize);
            image.StretchDirection = StretchDirection.UpOnly;
            image.Arrange(new Vector3(200, 300, 220), false);
            Assert.AreEqual(new Vector3(200, 300, 0), image.RenderSize);
            image.Arrange(new Vector3(20, 30, 22), false);
            Assert.AreEqual(new Vector3(100, 50, 0), image.RenderSize);
        }

        public void TestArrangeOverrideInfiniteValues(StretchType stretch)
        {
            var imageSize = new Vector3(100, 50, 0);
            var sprite = new Sprite { Region = new Rectangle(0, 0, (int)imageSize.X, (int)imageSize.Y), Borders = new Vector4(1, 2, 3, 4) };
            var image = new ImageElement { Source = (SpriteFromTexture)sprite, StretchType = stretch };

            image.Arrange(new Vector3(float.PositiveInfinity), false);
            Assert.AreEqual(imageSize, image.RenderSize);

            image.Arrange(new Vector3(150, float.PositiveInfinity, 10), false);
            Assert.AreEqual(stretch == StretchType.None ? imageSize : new Vector3(150, 75, 0), image.RenderSize);
        }
    }
}
