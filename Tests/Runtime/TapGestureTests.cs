using LiangTools.Debugging;
using NUnit.Framework;

namespace LiangTools.Tests
{
    public class TapGestureTests
    {
        private static bool FeedAll(TapGesture gesture, params ScreenHalf[] taps)
        {
            var opened = false;
            var time = 0f;
            foreach (var tap in taps)
            {
                time += 0.2f;
                opened = gesture.Feed(tap, time);
            }

            return opened;
        }

        [Test]
        public void DefaultPattern_IsOneLeftTwoRightThreeLeft()
        {
            var gesture = new TapGesture();
            Assert.AreEqual(6, gesture.Length);
        }

        [Test]
        public void Feed_OpensOnTheFullSequence()
        {
            var gesture = new TapGesture();
            var opened = FeedAll(gesture,
                ScreenHalf.Left,
                ScreenHalf.Right, ScreenHalf.Right,
                ScreenHalf.Left, ScreenHalf.Left, ScreenHalf.Left);

            Assert.IsTrue(opened);
        }

        [Test]
        public void Feed_DoesNotOpenEarly()
        {
            var gesture = new TapGesture();
            var opened = FeedAll(gesture,
                ScreenHalf.Left,
                ScreenHalf.Right, ScreenHalf.Right,
                ScreenHalf.Left, ScreenHalf.Left);

            Assert.IsFalse(opened);
            Assert.AreEqual(5, gesture.Progress);
        }

        [Test]
        public void Feed_ResetsOnAWrongHalf()
        {
            var gesture = new TapGesture();
            FeedAll(gesture, ScreenHalf.Left, ScreenHalf.Right);
            var opened = gesture.Feed(ScreenHalf.Left, 1f);

            Assert.IsFalse(opened);
            Assert.AreEqual(1, gesture.Progress, "a wrong tap that matches the first step restarts the sequence");
        }

        [Test]
        public void Feed_WrongHalfThatCannotStartTheSequenceClearsProgress()
        {
            var gesture = new TapGesture();
            FeedAll(gesture, ScreenHalf.Left, ScreenHalf.Right, ScreenHalf.Right);
            gesture.Feed(ScreenHalf.Right, 1f);

            Assert.AreEqual(0, gesture.Progress);
        }

        [Test]
        public void Feed_RestartsAfterTheTimeout()
        {
            var gesture = new TapGesture(timeoutSeconds: 2f);
            gesture.Feed(ScreenHalf.Left, 0f);
            gesture.Feed(ScreenHalf.Right, 10f);

            Assert.AreEqual(0, gesture.Progress, "the right tap arrived too late, and cannot start the sequence");
        }

        [Test]
        public void Feed_CanOpenTwiceInARow()
        {
            var gesture = new TapGesture();
            var full = new[]
            {
                ScreenHalf.Left,
                ScreenHalf.Right, ScreenHalf.Right,
                ScreenHalf.Left, ScreenHalf.Left, ScreenHalf.Left
            };

            Assert.IsTrue(FeedAll(gesture, full));
            Assert.AreEqual(0, gesture.Progress);
            Assert.IsTrue(FeedAll(gesture, full));
        }

        [Test]
        public void CustomPattern_IsHonoured()
        {
            var gesture = new TapGesture(new[]
            {
                new TapStep(ScreenHalf.Right, 2),
                new TapStep(ScreenHalf.Left, 1)
            });

            Assert.AreEqual(3, gesture.Length);
            Assert.IsTrue(FeedAll(gesture, ScreenHalf.Right, ScreenHalf.Right, ScreenHalf.Left));
        }
    }
}
