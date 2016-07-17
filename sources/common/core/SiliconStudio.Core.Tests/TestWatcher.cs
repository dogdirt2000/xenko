﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Core.Tests
{
    [TestFixture, Ignore("Need check")]
    public class TestWatcher
    {
        [Test]
        public void TestDirectory()
        {
            var tempDirectory = new DirectoryInfo("Temp." + typeof(TestWatcher).Assembly.GetName().Name);

            RemoveDirectory(tempDirectory);
            if (!tempDirectory.Exists)
            {
                tempDirectory.Create();
            }

            var pa0 = GetDirectoryPath(tempDirectory, @"a0");
            var pb0 = GetDirectoryPath(tempDirectory, @"a0\b0");
            var p1 = CreateDirectoryPath(tempDirectory, @"a0\b0\c0");
            var p2 = CreateDirectoryPath(tempDirectory, @"a0\b0\c1");
            var p3 = CreateDirectoryPath(tempDirectory, @"a0\b0\c2");

            var watcher = new DirectoryWatcher();
            watcher.Track(p1);
            var list = watcher.GetTrackedDirectories();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(p1, list[0]);

            watcher.Track(p2);
            list = watcher.GetTrackedDirectories();
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(p1, list[0]);
            Assert.AreEqual(p2, list[1]);

            // Adding p3 should set the track on the parent directory
            watcher.Track(p3);
            list = watcher.GetTrackedDirectories();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(pb0, list[0]);

            // Tracking again a child should not add a new track as the parent is already tracking
            watcher.Track(p1);
            list = watcher.GetTrackedDirectories();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(pb0, list[0]);

            watcher.Track(pb0);
            list = watcher.GetTrackedDirectories();
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(pb0, list[0]);

            var events = new List<FileEvent>();
            EventHandler<FileEvent> fileEventHandler = (sender, args) => events.Add(args);

            watcher.Modified += fileEventHandler;
            var p4 = CreateDirectoryPath(tempDirectory, @"a0\b0\c3");
            Thread.Sleep(20);
            watcher.Modified -= fileEventHandler;

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(p4, events[0].FullPath.ToLower());

            events.Clear();
            watcher.Modified += fileEventHandler;
            RemoveDirectory(new DirectoryInfo(pb0));
            Thread.Sleep(400);
            watcher.Modified -= fileEventHandler;

            Assert.IsTrue(events.All(args => args.ChangeType == FileEventChangeType.Deleted)); // c0, c1, c2, c3 removed

            //// We should not track any directory
            //list = watcher.GetTrackedDirectories();
            //Assert.AreEqual(0, list.Count);

            RemoveDirectory(tempDirectory);
        }

        private string GetDirectoryPath(DirectoryInfo root, string subPath)
        {
            var tempDirectory = Path.Combine(root.FullName, subPath);
            return tempDirectory.ToLower();
        }

        private string CreateDirectoryPath(DirectoryInfo root, string subPath)
        {
            var tempDirectory = GetDirectoryPath(root, subPath);
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }
            return tempDirectory;
        }

        private void RemoveDirectory(DirectoryInfo path)
        {
            if (Directory.Exists(path.FullName))
            {
                TemporaryDirectory.DeleteDirectory(path.FullName);
            }
            if (Directory.Exists(path.FullName))
            {
                Trace.WriteLine(string.Format("Unable to remove directory {0}", path.FullName));
            }
        }
    }
}
#endif