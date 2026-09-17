using LiangTools.Debugging;
using NUnit.Framework;

namespace LiangTools.Tests
{
    public class TapGestureTests
    {
        private static bool FeedAll(TapGesture gesture, params ScreenCorner[] taps)
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
        public void DefaultPattern_IsOneTopLeftTwoTopRightThreeTopLeft()
        {
            var gesture = new TapGesture();
            Assert.AreEqual(6, gesture.Length);
        }

        [Test]
        public void Feed_OpensOnTheFullSequence()
        {
            var gesture = new TapGesture();
            var opened = FeedAll(gesture,
                ScreenCorner.TopLeft,
                ScreenCorner.TopRight, ScreenCorner.TopRight,
                ScreenCorner.TopLeft, ScreenCorner.TopLeft, ScreenCorner.TopLeft);

            Assert.IsTrue(opened);
        }

        [Test]
        public void Feed_DoesNotOpenEarly()
        {
            var gesture = new TapGesture();
            var opened = FeedAll(gesture,
                ScreenCorner.TopLeft,
                ScreenCorner.TopRight, ScreenCorner.TopRight,
                ScreenCorner.TopLeft, ScreenCorner.TopLeft);

            Assert.IsFalse(opened);
            Assert.AreEqual(5, gesture.Progress);
        }

        [Test]
        public void Feed_ResetsOnAWrongCorner()
        {
            var gesture = new TapGesture();
            FeedAll(gesture, ScreenCorner.TopLeft, ScreenCorner.TopRight);
            var opened = gesture.Feed(ScreenCorner.TopLeft, 1f);

            Assert.IsFalse(opened);
            Assert.AreEqual(1, gesture.Progress, "a wrong tap that matches the first step restarts the sequence");
        }

        [Test]
        public void Feed_WrongCornerThatCannotStartTheSequenceClearsProgress()
        {
            var gesture = new TapGesture();
            FeedAll(gesture, ScreenCorner.TopLeft, ScreenCorner.TopRight, ScreenCorner.TopRight);
            gesture.Feed(ScreenCorner.TopRight, 1f);

            Assert.AreEqual(0, gesture.Progress);
        }

        [Test]
        public void Feed_RestartsAfterTheTimeout()
        {
            var gesture = new TapGesture(timeoutSeconds: 2f);
            gesture.Feed(ScreenCorner.TopLeft, 0f);
            gesture.Feed(ScreenCorner.TopRight, 10f);

            Assert.AreEqual(0, gesture.Progress, "the right tap arrived too late, and cannot start the sequence");
        }

        [Test]
        public void Feed_CanOpenTwiceInARow()
        {
            var gesture = new TapGesture();
            var full = new[]
            {
                ScreenCorner.TopLeft,
                ScreenCorner.TopRight, ScreenCorner.TopRight,
                ScreenCorner.TopLeft, ScreenCorner.TopLeft, ScreenCorner.TopLeft
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
                new TapStep(ScreenCorner.TopRight, 2),
                new TapStep(ScreenCorner.TopLeft, 1)
            });

            Assert.AreEqual(3, gesture.Length);
            Assert.IsTrue(FeedAll(gesture, ScreenCorner.TopRight, ScreenCorner.TopRight, ScreenCorner.TopLeft));
        }
    }
}
