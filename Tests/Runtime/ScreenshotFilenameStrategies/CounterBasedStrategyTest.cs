// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TestHelper.Monkey.ScreenshotFilenameStrategies
{
    [TestFixture]
    public class CounterBasedStrategyTest
    {
        [Test]
        public void FileNamePrefixSpecified()
        {
            var strategy = new CounterBasedStrategy("prefix");

            var actual = Enumerable.Repeat(0, 5).Select(_ => strategy.GetFileName()).ToList();
            var expected = new List<string>
            {
                "prefix_0001.png",
                "prefix_0002.png",
                "prefix_0003.png",
                "prefix_0004.png",
                "prefix_0005.png"
            };
            Assert.That(actual, Is.EquivalentTo(expected));
        }


        [Test]
        public void DefaultFileNamePrefix()
        {
            var strategy = new CounterBasedStrategy(null);

            var actual = Enumerable.Repeat(0, 5).Select(_ => strategy.GetFileName()).ToList();
            var expected = new List<string>
            {
                $"{nameof(DefaultFileNamePrefix)}_0001.png",
                $"{nameof(DefaultFileNamePrefix)}_0002.png",
                $"{nameof(DefaultFileNamePrefix)}_0003.png",
                $"{nameof(DefaultFileNamePrefix)}_0004.png",
                $"{nameof(DefaultFileNamePrefix)}_0005.png"
            };
            Assert.That(actual, Is.EquivalentTo(expected));
        }
    }
}
