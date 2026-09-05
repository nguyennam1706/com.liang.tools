using LiangTools.Editor.TimeControl;
using NUnit.Framework;
using UnityEngine;

namespace LiangTools.Editor.Tests
{
    public class TimeScaleSnapTests
    {
        private TimeScaleSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<TimeScaleSettings>();
            _settings.minimum = 0f;
            _settings.maximum = 2f;
            _settings.step = 0.5f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        [TestCase(0f, 0f)]
        [TestCase(0.2f, 0f)]
        [TestCase(0.3f, 0.5f)]
        [TestCase(0.7f, 0.5f)]
        [TestCase(0.8f, 1f)]
        [TestCase(1.24f, 1f)]
        [TestCase(1.26f, 1.5f)]
        [TestCase(2f, 2f)]
        public void Snap_LandsOnStepMultiples(float input, float expected)
        {
            Assert.AreEqual(expected, _settings.Snap(input), 0.0001f);
        }

        [TestCase(-5f, 0f)]
        [TestCase(99f, 2f)]
        public void Snap_ClampsToRange(float input, float expected)
        {
            Assert.AreEqual(expected, _settings.Snap(input), 0.0001f);
        }

        [Test]
        public void Snap_WithZeroStep_IsContinuous()
        {
            _settings.step = 0f;
            Assert.AreEqual(0.73f, _settings.Snap(0.73f), 0.0001f);
        }

        [Test]
        public void Snap_RespectsNonZeroMinimum()
        {
            _settings.minimum = 0.25f;
            _settings.maximum = 2.25f;
            Assert.AreEqual(0.75f, _settings.Snap(0.8f), 0.0001f);
        }
    }
}
