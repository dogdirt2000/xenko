using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core.MicroThreading;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Graphics.Regression;
// ReSharper disable AccessToDisposedClosure
// ReSharper disable FunctionNeverReturns

namespace SiliconStudio.Xenko.Engine.Tests
{
    internal class EventSystemTest
    {
        private readonly Scheduler scheduler = new Scheduler();

        public bool IsRunning { get; private set; } = true;

        public void AddTask(Func<Task> task, int priority = 0)
        {
            var microThread = scheduler.Create();
            microThread.Priority = priority;
            microThread.Start(task);
        }

        public void Run(int frames = int.MaxValue, int msWait = 25)
        {
            while (IsRunning && frames > 0)
            {
                scheduler.Run();
                Thread.Sleep(msWait);
            }
        }

        public void Exit()
        {
            IsRunning = false;
        }

        public ChannelMicroThreadAwaiter<int> NextFrame()
        {
            return scheduler.NextFrame();
        }
    }

    [TestFixture]
    public class EventSystemTests
    {
        /// <summary>
        /// Make sure that events are able to be consumed immediately
        /// </summary>
        [Test]
        public void SameFrameReceive()
        {
            var key = new EventKey();
            var recv = new EventReceiver(key);

            key.Broadcast();
            Assert.True(recv.TryReceive());

            Assert.False(recv.TryReceive());
        }

        /// <summary>
        /// Make sure that we can receive events immediately even when using await/async
        /// </summary>
        [Test]
        public void SameFrameReceiveAsync()
        {
            var test = new EventSystemTest();

            var frameCounter = 0;

            test.AddTask(async () =>
            {
                while (test.IsRunning)
                {
                    frameCounter++;
                    await test.NextFrame();
                }
            }, 100);

            test.AddTask(async () =>
            {
                var key = new EventKey();
                var recv = new EventReceiver(key);

                key.Broadcast();

                var currentFrame = frameCounter;

                await recv.ReceiveAsync();

                Assert.AreEqual(currentFrame, frameCounter);

                test.Exit();
            });

            test.Run();
        }

        /// <summary>
        /// Make sure that newly created receivers do not receive previously broadcasted events (before creation)
        /// </summary>
        [Test]
        public void DelayedReceiverCreation()
        {
            var game = new EventSystemTest();

            var frameCount = 0;

            game.AddTask(async () =>
            {
                var evt = new EventKey();
                EventReceiver rcv = null;
                while (frameCount < 25)
                {
                    if (frameCount == 5)
                    {
                        evt.Broadcast();
                    }
                    if (frameCount == 20)
                    {
                        rcv = new EventReceiver(evt);
                        Assert.False(rcv.TryReceive());
                        evt.Broadcast();
                    }
                    if (frameCount == 22)
                    {
                        Assert.NotNull(rcv);
                        Assert.True(rcv.TryReceive());

                        game.Exit();
                    }
                    await game.NextFrame();
                    frameCount++;
                }
            });

            game.Run();
        }

        /// <summary>
        /// Test that even if broadcast happens in another thread we receive events in the game schedluer thread
        /// </summary>
        [Test]
        public void DifferentThreadBroadcast()
        {
            var game = new EventSystemTest();

            var counter = 0;

            var broadcaster = new EventKey();

            var readyCount = 0;

            game.AddTask(async () =>
            {
                var recv = new EventReceiver(broadcaster, EventReceiverOptions.Buffered);

                Interlocked.Increment(ref readyCount);

                for (;;)
                {
                    await recv.ReceiveAsync();
                    Interlocked.Increment(ref counter);
                }
            });

            game.AddTask(async () =>
            {
                var recv = new EventReceiver(broadcaster, EventReceiverOptions.Buffered);

                Interlocked.Increment(ref readyCount);

                for (;;)
                {
                    await recv.ReceiveAsync();
                    Interlocked.Increment(ref counter);
                }
            });

            game.AddTask(async () =>
            {
                var recv = new EventReceiver(broadcaster, EventReceiverOptions.Buffered);

                Interlocked.Increment(ref readyCount);

                for (;;)
                {
                    await recv.ReceiveAsync();
                    Interlocked.Increment(ref counter);
                }
            });

            var t1W = new AutoResetEvent(false);
            var t2W = new AutoResetEvent(false);

            var waitHandles = new WaitHandle[]
            {
                t1W,
                t2W
            };

            Exception threadException = null;

            new Thread(() =>
            {
                try
                {
                    while (!game.IsRunning && readyCount < 3)
                    {
                        Thread.Sleep(200);
                    }

                    var frameCounter = 0;

                    while (true)
                    {
                        Thread.Sleep(50);
                        frameCounter++;
                        broadcaster.Broadcast();

                        if (frameCounter < 200) continue;
                        t1W.Set();
                        return;
                    }
                }
                catch (Exception e)
                {
                    threadException = e;
                }
            }).Start();

            new Thread(() =>
            {
                try
                {
                    while (!game.IsRunning && readyCount < 3)
                    {
                        Thread.Sleep(200);
                    }

                    var frameCounter = 0;

                    while (true)
                    {
                        Thread.Sleep(50);
                        frameCounter++;
                        broadcaster.Broadcast();

                        if (frameCounter < 200) continue;
                        t2W.Set();
                        return;
                    }
                }
                catch (Exception e)
                {
                    threadException = e;
                }
            }).Start();

            new Thread(() =>
            {
                try
                {
                    //wait until both threads have broadcasted 200 times each
                    WaitHandle.WaitAll(waitHandles);

                    Thread.Sleep(2000);

                    game.Exit();
                }
                catch (Exception e)
                {
                    threadException = e;
                }
            }).Start();

            game.Run();

            Assert.IsNull(threadException);

            Assert.AreEqual(1200, counter);
        }

        /// <summary>
        /// Test that even if broadcast happens in another thread we receive events in the game schedluer thread
        /// With a different syntax sugar
        /// </summary>
        [Test]
        public void DifferentSyntax()
        {
            var game = new EventSystemTest();

            var frameCounter = 0;

            var broadcaster = new EventKey();

            game.AddTask(async () =>
            {
                var tests = 5;
                var recv = new EventReceiver(broadcaster);

                var threadId = Thread.CurrentThread.ManagedThreadId;

                while (tests-- > 0)
                {
                    await recv;
                    Assert.AreEqual(threadId, Thread.CurrentThread.ManagedThreadId);
                }
            });

            game.AddTask(async () =>
            {
                var tests = 5;
                var recv = new EventReceiver(broadcaster);

                var threadId = Thread.CurrentThread.ManagedThreadId;

                while (tests-- > 0)
                {
                    await recv;
                    Assert.AreEqual(threadId, Thread.CurrentThread.ManagedThreadId);
                }
            });

            game.AddTask(async () =>
            {
                var tests = 5;
                var recv = new EventReceiver(broadcaster);

                var threadId = Thread.CurrentThread.ManagedThreadId;

                while (tests-- > 0)
                {
                    await recv;
                    Assert.AreEqual(threadId, Thread.CurrentThread.ManagedThreadId);
                }
            });

            Task.Run(async () =>
            {
                while (!game.IsRunning)
                {
                    await Task.Delay(100);
                }

                while (true)
                {
                    frameCounter++;
                    broadcaster.Broadcast();
                    if (frameCounter == 20)
                    {
                        game.Exit();
                    }
                    await Task.Delay(50);
                }
            });

            game.Run();
        }

        /// <summary>
        /// Test buffered events and receive many in one go
        /// </summary>
        [Test]
        public void ReceiveManyCheck()
        {
            var game = new EventSystemTest();

            var frameCount = 0;

            game.AddTask(async () =>
            {
                var evt = new EventKey();
                var rcv = new EventReceiver(evt, EventReceiverOptions.Buffered);
                while (frameCount < 25)
                {
                    evt.Broadcast();

                    if (frameCount == 20)
                    {
                        var manyEvents = rcv.TryReceiveAll();
                        Assert.AreEqual(manyEvents, 21);
                        game.Exit();
                    }
                    await game.NextFrame();
                    frameCount++;
                }
            });

            game.Run();
        }

        /// <summary>
        /// Test proper Task.WaitAny behavior..
        /// </summary>
        [Test]
        public void ReceiveFirstCheck()
        {
            var game = new EventSystemTest();

            var frameCount = 0;

            var evt1 = new EventKey();
            var evt2 = new EventKey();

            game.AddTask(async () =>
            {
                var rcv1 = new EventReceiver(evt1);
                var rcv2 = new EventReceiver(evt2);

                for (;;)
                {
                    var rcv = await EventReceiver.ReceiveOne(rcv1, rcv2);

                    if (rcv.Receiver == rcv1)
                    {
                        evt2.Broadcast(); //this is the point of this test.. see if t2 will get populated next loop
                        await game.NextFrame();
                    }
                    else if (rcv.Receiver == rcv2)
                    {
                        await game.NextFrame();
                        game.Exit();
                    }
                }
            });

            game.AddTask(async () =>
            {
                while (frameCount < 30 && game.IsRunning)
                {
                    frameCount++;

                    if (frameCount == 20)
                    {
                        evt1.Broadcast();
                    }

                    await game.NextFrame();
                }

                Assert.Fail("t2 should be completed");
            });
        }

        /// <summary>
        /// Test ClearEveryFrame option flag, which clears events at end of every game frame
        /// </summary>
        [Test]
        public void EveryFrameClear()
        {
            var game = new EventSystemTest();

            var frameCount = 0;

            var evt = new EventKey();

            game.AddTask(async () =>
            {
                while (frameCount < 25)
                {
                    evt.Broadcast();
                    evt.Broadcast();

                    await game.NextFrame();
                }
            }, 10);

            game.AddTask(async () =>
            {
                var rcv = new EventReceiver(evt, EventReceiverOptions.Buffered);
                while (frameCount < 25)
                {
                    if (frameCount == 20)
                    {
                        var manyEvents = rcv.TryReceiveAll();
                        Assert.AreEqual(2, manyEvents);
                        game.Exit();
                    }

                    rcv.Reset();

                    await game.NextFrame();

                    frameCount++;
                }
            }, -10);

            game.Run();
        }
    }
}
