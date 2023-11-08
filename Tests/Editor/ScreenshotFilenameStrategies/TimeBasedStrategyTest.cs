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
    public class TimeBasedStrategyTest
    {
        [Test]
        public void CreateWithStubs()
        {
            var directory = Path.Combine("path", "to", "dir");
            var i = 0;
            Func<DateTime> now = () => new DateTime(1970, 1, 1, 0, 0, 0, i++);
            var getFilePath = TimeBasedStrategy.Create(
                directory,
                "prefix",
                now
            );

            var actual = Enumerable.Repeat(0, 5).Select(_ => getFilePath()).ToList();
            var expected = new List<ScreenshotOptions.FilePath>
            {
                new ScreenshotOptions.FilePath(directory, "prefix_1970-01-01T00_00_00.0000000.png"),
                new ScreenshotOptions.FilePath(directory, "prefix_1970-01-01T00_00_00.0010000.png"),
                new ScreenshotOptions.FilePath(directory, "prefix_1970-01-01T00_00_00.0020000.png"),
                new ScreenshotOptions.FilePath(directory, "prefix_1970-01-01T00_00_00.0030000.png"),
                new ScreenshotOptions.FilePath(directory, "prefix_1970-01-01T00_00_00.0040000.png")
            };
            Assert.That(
                actual,
                Is.EquivalentTo(expected),
                string.Join("\n", actual.Select(path => Path.Combine(path.Directory, path.Filename)))
            );
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
            Assert.That(
                actual.Filename.StartsWith(nameof(CreateRealWorld_FilenamePrefix_Default)),
                Is.True,
                actual.Filename
            );
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
