﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Core.Tests
{
    [TestFixture]
    public class TestProfiler
    {
        public static readonly ProfilingKey TestGroup = new ProfilingKey("TestProfiler");
        public static readonly ProfilingKey TestKey = new ProfilingKey(TestGroup, "Test", ProfilingKeyFlags.Log);
        public static readonly ProfilingKey Test2Key = new ProfilingKey(TestGroup, "Test2", ProfilingKeyFlags.Log);

        [Test]
        public void TestSimpleNotEnabled()
        {
            Profiler.Reset();
            using (var watcher = ExpectLog(new List<MatchMessageDelegate>()))
            {
                using (var profile = Profiler.Begin(TestKey))
                {
                    Utilities.Sleep(100);
                }
            }
        }

        [Test]
        public void TestSimpleEnabled()
        {
            Profiler.Reset();
            const int timeToWait = 200;
            using (var watcher = ExpectLog(new List<MatchMessageDelegate>()
                {
                    MessageStartWith("[Profiler] #0: Begin: TestProfiler.Test"),
                    MessageStartWith("[Profiler] #0: End: TestProfiler.Test", timeToWait),
                }))
            {

                Profiler.Enable(TestKey);
                using (var profile = Profiler.Begin(TestKey))
                {
                    Utilities.Sleep(timeToWait);
                }
            }
        }

        [Test]
        public void TestSimpleNested()
        {
            Profiler.Reset();
            const int timeToWait = 200;
            using (var watcher = ExpectLog(new List<MatchMessageDelegate>()
                {
                    MessageStartWith("[Profiler] #0: Begin: TestProfiler.Test"),
                    MessageStartWith("[Profiler] #1: Begin: TestProfiler.Test2"),
                    MessageStartWith("[Profiler] #1: End: TestProfiler.Test2", timeToWait),
                    MessageStartWith("[Profiler] #0: End: TestProfiler.Test", timeToWait),
                }))
            {

                Profiler.EnableAll();
                using (var profile = Profiler.Begin(TestKey))
                {
                    using (var profile2 = Profiler.Begin(Test2Key))
                    {
                        Utilities.Sleep(timeToWait);
                    }
                }
            }
        }

        [Test]
        public void TestWithMarkers()
        {
            Profiler.Reset();
            const int timeToWait = 100;

            using (var watcher = ExpectLog(new List<MatchMessageDelegate>()
                {
                    MessageStartWith("[Profiler] #0: Begin: TestProfiler.Test"),
                    MessageStartWith("[Profiler] #0: Mark: TestProfiler.Test", timeToWait),
                    MessageStartWith("[Profiler] #0: Mark: TestProfiler.Test", timeToWait * 2),
                    MessageStartWith("[Profiler] #0: End: TestProfiler.Test", timeToWait * 2),
                }))
            {

                Profiler.EnableAll();
                using (var profile = Profiler.Begin(TestKey))
                {
                    Utilities.Sleep(timeToWait);
                    profile.Mark();

                    Utilities.Sleep(timeToWait);
                    profile.Mark();
                }
            }
        }


        [Test]
        public void TestWitAttributes()
        {
            Profiler.Reset();
            const int timeToWait = 100;

            using (var watcher = ExpectLog(new List<MatchMessageDelegate>()
                {
                    MessageStartWith("[Profiler] #0: Begin: TestProfiler.Test"),
                    MessageStartWith("[Profiler] #0: Mark: TestProfiler.Test", message => message.Contains("MyAttribute")),
                    MessageStartWith("[Profiler] #0: End: TestProfiler.Test", timeToWait),
                }))
            {

                Profiler.EnableAll();
                using (var profile = Profiler.Begin(TestKey))
                {
                    profile.SetAttribute("MyAttribute", 5);
                    Utilities.Sleep(timeToWait);
                    profile.Mark();
                }
            }
        }


        private static Regex matchElapsed = new Regex(@"Elapsed = ([\d\.]+)");

        // Maximum time difference accepted between elapsed time
        private const double ElapsedTimeDeltaMax = 100;

        public static MatchMessageDelegate MessageStartWith(string text, Func<string, bool> matchFunction = null)
        {
            return (string message, out string expectingMessage, bool getOnlyExpectingMessage) =>
            {
                expectingMessage = text;

                if (!getOnlyExpectingMessage && matchFunction != null)
                {
                    matchFunction(message);
                }

                return message.StartsWith(text);
            };
        }

        public static MatchMessageDelegate MessageStartWith(string text, double expectedElapsed)
        {
            return MessageStartWith(text, message =>
                {
                    var match = matchElapsed.Match(message);
                    if (match.Success)
                    {
                        var elapsedStr = match.Groups[1].Value;
                        double elapsed;
                        Assert.That(double.TryParse(elapsedStr, out elapsed), "Expecting parsable double for elapsed [{0}]", elapsedStr);
                        Assert.That(Math.Abs(elapsed - expectedElapsed) < ElapsedTimeDeltaMax, "Elapsed time [{0}] doesn't match expected value [{1}]", elapsed, expectedElapsed);
                    }
                    return true;
                });
        }

        public delegate bool MatchMessageDelegate(string message, out string expectingMessage, bool getOnlyExpectingMessage);

        private class ProfilerWatcher : IDisposable
        {
            public int CurrentMessage;

            public readonly List<MatchMessageDelegate> ExpectedMessages;

            public Action<ILogMessage> LogAction;

            public ProfilerWatcher(List<MatchMessageDelegate> expectedMessages)
            {
                ExpectedMessages = expectedMessages;
            }

            public void Dispose()
            {
                GlobalLogger.GlobalMessageLogged -= LogAction;
                var missingMessage = new StringBuilder();
                for (int i = CurrentMessage; i < ExpectedMessages.Count; i++)
                {
                    string expectedMessage;
                    ExpectedMessages[i](string.Empty, out expectedMessage, true);
                    missingMessage.Append(expectedMessage);
                    if ((CurrentMessage + 1) < ExpectedMessages.Count)
                    {
                        missingMessage.AppendLine();
                    }
                }

                Assert.That(CurrentMessage, Is.EqualTo(ExpectedMessages.Count), "Invalid number of profiler events received [{0}] Expecting [{1}]. Missing messages: [{2}]", CurrentMessage, ExpectedMessages.Count, missingMessage);
            }
        }

        private ProfilerWatcher ExpectLog(List<MatchMessageDelegate> expectedMessages)
        {
            var watcher = new ProfilerWatcher(expectedMessages);
            watcher.LogAction = message =>
            {
                var messageToString = message.ToString();
                Console.Out.WriteLine(message.ToString());
                Console.Out.Flush();

                Assert.That(watcher.CurrentMessage < expectedMessages.Count, "Unexpected message received: [{0}]", messageToString);
                string expectedMessage;
                var result = expectedMessages[watcher.CurrentMessage](messageToString, out expectedMessage, false);
                Assert.That(result, "Expecting message [{0}]", expectedMessage);
                watcher.CurrentMessage++;
            };
            GlobalLogger.GlobalMessageLogged += watcher.LogAction;
            return watcher;
        }
    }
}