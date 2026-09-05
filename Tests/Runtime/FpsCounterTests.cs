using LiangTools.Debugging;
using NUnit.Framework;

namespace LiangTools.Tests
{
    public class FpsCounterTests
    {
        [Test]
        public void Sample_ComputesCurrentFromDeltaTime()
        {
            var counter = new FpsCounter();
            counter.Sample(0.02f);

            Assert.AreEqual(50f, counter.Current, 0.001f);
            Assert.AreEqual(20f, counter.FrameTimeMs, 0.001f);
        }

        [Test]
        public void Sample_IgnoresNonPositiveDeltaTime()
        {
            var counter = new FpsCounter();
            counter.Sample(0f);
            counter.Sample(-1f);

            Assert.AreEqual(0f, counter.Current);
            Assert.AreEqual(0f, counter.Average);
        }

        [Test]
        public void Average_UsesOnlyTheSamplesSeen()
        {
            var counter = new FpsCounter(10);
            counter.Sample(0.02f);
            counter.Sample(0.01f);

            Assert.AreEqual(75f, counter.Average, 0.001f);
        }

        [Test]
        public void Average_DropsSamplesOutsideTheWindow()
        {
            var counter = new FpsCounter(2);
            counter.Sample(0.1f);
            counter.Sample(0.02f);
            counter.Sample(0.02f);

            Assert.AreEqual(50f, counter.Average, 0.001f);
        }

        [Test]
        public void MinAndMax_TrackExtremes()
        {
            var counter = new FpsCounter(4);
            counter.Sample(0.1f);
            counter.Sample(0.01f);
            counter.Sample(0.05f);

            Assert.AreEqual(10f, counter.Minimum, 0.001f);
            Assert.AreEqual(100f, counter.Maximum, 0.001f);
        }

        [Test]
        public void Reset_ClearsStatistics()
        {
            var counter = new FpsCounter(4);
            counter.Sample(0.02f);
            counter.Reset();

            Assert.AreEqual(0f, counter.Current);
            Assert.AreEqual(0f, counter.Average);
            Assert.AreEqual(0f, counter.Maximum);
        }
    }
}
