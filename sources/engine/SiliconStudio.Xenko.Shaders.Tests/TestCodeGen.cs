﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Text;

using NUnit.Framework;

using SiliconStudio.Xenko.Shaders.Parser.Mixins;

namespace SiliconStudio.Xenko.Shaders.Tests
{
    /// <summary>
    /// Code used to regenerate all cs files from xksl/xkfx in the project
    /// </summary>
    [TestFixture]
    public class TestCodeGen
    {
        //[Test]
        public void Test()
        {
            var filePath = @"D:\Code\Xenko\sources\engine\SiliconStudio.Xenko.Shaders.Tests\GameAssets\Mixins\A.xksl";
            var source = File.ReadAllText(filePath);
            var content = ShaderMixinCodeGen.GenerateCsharp(source, filePath.Replace("C:", "D:"));
        }

        //[Test] // Decomment this line to regenerate all files (sources and samples)
        public void RebuildAllXkfxXksl()
        {
            RegenerateDirectory(Path.Combine(Environment.CurrentDirectory, @"..\..\sources"));
            RegenerateDirectory(Path.Combine(Environment.CurrentDirectory, @"..\..\samples"));
        }

        private static void RegenerateDirectory(string directory)
        {
            //foreach (var xksl in Directory.EnumerateFiles(directory, "*.xksl", SearchOption.AllDirectories))
            //{
            //    RebuildFile(xksl);
            //}
            foreach (var xkfx in Directory.EnumerateFiles(directory, "*.xkfx", SearchOption.AllDirectories))
            {
                RebuildFile(xkfx);
            }
        }

        private static void RebuildFile(string filePath)
        {
            try
            {
                var source = File.ReadAllText(filePath);
                var content = ShaderMixinCodeGen.GenerateCsharp(source, filePath);

                // Sometimes, we have a collision with the .cs file, so generated filename might be postfixed with 1
                var destPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "1.cs");
                if (!File.Exists(destPath))
                    destPath = Path.ChangeExtension(filePath, ".cs");
                if (!File.Exists(destPath))
                {
                    Console.WriteLine("Target file {0} doesn't exist", destPath);
                    return;
                }
                File.WriteAllText(destPath, content, Encoding.UTF8);
                Console.WriteLine("File generated {0}", filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error {0}: {1}", filePath, ex);
            }
        }
    }
}