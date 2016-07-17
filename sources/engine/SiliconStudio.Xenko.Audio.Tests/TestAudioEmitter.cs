﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Audio.Tests
{
    /// <summary>
    /// Tests for <see cref="AudioEmitter"/>.
    /// </summary>
    [TestFixture]
    public class TestAudioEmitter
    {
        private readonly AudioEmitter defaultEmitter = new AudioEmitter();

        /// <summary>
        /// Test the behaviour of the Position function.
        /// </summary>
        [Test]
        public void TestPosition()
        {
            //////////////////////////////
            // 1. Check the default value
            Assert.AreEqual(Vector3.Zero, defaultEmitter.Position, "The AudioEmitter defaul location is not 0");

            ///////////////////////////////////////////
            // 2. Check for no crash and correct value
            Assert.DoesNotThrow(() => defaultEmitter.Position = Vector3.One, "AudioEmitter.Position.Set crashed.");
            Assert.AreEqual(Vector3.One, defaultEmitter.Position, "AudioEmitter.Position value is not what it is supposed to be.");
        }

        /// <summary>
        /// Test the behaviour of the Velocity function.
        /// </summary>
        [Test]
        public void TestVelocity()
        {
            //////////////////////////////
            // 1. Check the default value
            Assert.AreEqual(Vector3.Zero, defaultEmitter.Velocity, "The AudioEmitter defaul Velocity is not 0");

            ///////////////////////////////////////////
            // 2. Check for no crash and correct value
            Assert.DoesNotThrow(() => defaultEmitter.Velocity = Vector3.One, "AudioEmitter.Velocity.Set crashed.");
            Assert.AreEqual(Vector3.One, defaultEmitter.Velocity, "AudioEmitter.Velocity value is not what it is supposed to be.");
        }

//        /// <summary>
//        /// Test the behaviour of the DopplerScale function.
//        /// </summary>
//        [Test]
//        public void TestDopplerScale()
//        {
//            //////////////////////////////
//            // 1. Check the default value
//            Assert.AreEqual(1f, defaultEmitter.DopplerScale, "The AudioEmitter defaul DopplerScale is not 1");
//
//            ///////////////////////////////////////////
//            // 2. Check for no crash and correct value
//            Assert.DoesNotThrow(() => defaultEmitter.DopplerScale = 5f, "AudioEmitter.DopplerScale.Set crashed.");
//            Assert.AreEqual(5f, defaultEmitter.DopplerScale, "AudioEmitter.DopplerScale value is not what it is supposed to be.");
//
//            /////////////////////////////////////////////////////////////////////////////////////
//            // 3. Check that a negative value throws the 'ArgumentOutOfRangeException' exception
//            Assert.Throws<ArgumentOutOfRangeException>(() => defaultEmitter.DopplerScale = -1f, "AudioEmitter.DopplerScale did not throw 'ArgumentOutOfRangeException' when setting a negative value.");
//        }
//
//        /// <summary>
//        /// Test the behaviour of the DistanceScale function.
//        /// </summary>
//        [Test]
//        public void TestDistanceScale()
//        {
//            //////////////////////////////
//            // 1. Check the default value
//            Assert.AreEqual(1f, defaultEmitter.DistanceScale, "The AudioEmitter defaul DistanceScale is not 1");
//
//            ///////////////////////////////////////////
//            // 2. Check for no crash and correct value
//            Assert.DoesNotThrow(() => defaultEmitter.DistanceScale = 5f, "AudioEmitter.DistanceScale.Set crashed.");
//            Assert.AreEqual(5f, defaultEmitter.DistanceScale, "AudioEmitter.DistanceScale value is not what it is supposed to be.");
//
//            /////////////////////////////////////////////////////////////////////////////////////
//            // 3. Check that a negative value throws the 'ArgumentOutOfRangeException' exception
//            Assert.Throws<ArgumentOutOfRangeException>(() => defaultEmitter.DistanceScale = -1f, "AudioEmitter.DistanceScale did not throw 'ArgumentOutOfRangeException' when setting a negative value.");
//        }
    }
}
