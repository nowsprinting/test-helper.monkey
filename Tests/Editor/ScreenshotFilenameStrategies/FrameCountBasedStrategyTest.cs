// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace TestHelper.Monkey.ScreenshotFilenameStrategies
{
    [TestFixture]
    public class FrameCountBasedStrategyTest
    {
        [Test]
        public void CreateWithStubs()
        {
            var directory = Path.Combine("path", "to", "dir");
            var frameCount = 0;
            Func<int> getFrameCount = () => frameCount++;
            var getFilePath = FrameCountBasedStrategy.Create(
                directory,
                "prefix",
                getFrameCount
            );

            var actual = Enumerable.Repeat(0, 5).Select(_ => getFilePath()).ToList();
            var expected = new List<ScreenshotOptions.FilePath>
            {
                new ScreenshotOptions.FilePath(directory, "prefix_0000000000.png"),
                new ScreenshotOptions.FilePath(directory, "prefix_0000000001.png"),
                new ScreenshotOptions.FilePath(directory, "prefix_0000000002.png"),
                new ScreenshotOptions.FilePath(directory, "prefix_0000000003.png"),
                new ScreenshotOptions.FilePath(directory, "prefix_0000000004.png")
            };
            Assert.That(actual, Is.EquivalentTo(expected));
        }


        [Test]
        public void CreateRealWorld_Directory_Default()
        {
            var actual = FrameCountBasedStrategy.Create()();
            Assert.That(actual.Directory, Is.EqualTo(ScreenshotFilenameStrategy.GetDefaultDirectory()));
        }


        [Test]
        public void CreateRealWorld_Directory_Specified()
        {
            var directory = Path.Combine("path", "to", "dir");
            var actual = FrameCountBasedStrategy.Create(directory: directory)();
            Assert.That(actual.Directory, Is.EqualTo(directory));
        }


        [Test]
        public void CreateRealWorld_FilenamePrefix_Default()
        {
            var actual = FrameCountBasedStrategy.Create()();
            Assert.That(actual.Filename.StartsWith(nameof(CreateRealWorld_FilenamePrefix_Default)), Is.True, actual.Filename);
            Assert.That(actual.Filename.EndsWith(".png"), Is.True, actual.Filename);
        }


        [Test]
        public void CreateRealWorld_FilenamePrefix_Specified()
        {
            var prefix = Path.Combine("prefix");
            var actual = FrameCountBasedStrategy.Create(filenamePrefix: prefix)();
            Assert.That(actual.Filename.StartsWith(prefix), Is.True);
            Assert.That(actual.Filename.EndsWith(".png"), Is.True);
        }
    }
}
