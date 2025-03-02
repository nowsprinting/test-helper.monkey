// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;

namespace TestHelper.Monkey.Operators.Utils
{
    [TestFixture]
    public class OperationLoggerTest
    {
        private SpyLogger _spyLogger;

        private OperationLogger CreateOperationLogger(ScreenshotOptions screenshotOptions = null)
        {
            var gameObject = new GameObject("Target");
            gameObject.AddComponent<FakeComponent>();
            var @operator = new UGUIClickOperator();
            _spyLogger = new SpyLogger();
            return new OperationLogger(gameObject, @operator, _spyLogger, screenshotOptions);
        }

        [Test]
        public async Task Log_WithoutScreenshotOptions_OutputLog()
        {
            var sut = CreateOperationLogger();

            await sut.Log();

            Assume.That(_spyLogger.Messages, Has.Count.EqualTo(1));
            Assert.That(_spyLogger.Messages[0], Does.Match(@"UGUIClickOperator operates to Target\(-?\d+\)"));
        }

        [Test]
        public async Task Log_WithScreenshotOptions_TakeScreenshotAndOutputLog()
        {
            var screenshotOptions = new ScreenshotOptions()
            {
                Directory = Application.temporaryCachePath,
                FilenameStrategy = new StubScreenshotFilenameStrategy(
                    $"{TestContext.CurrentContext.Test.Name}.png"),
            };
            var sut = CreateOperationLogger(screenshotOptions);
            sut.Comments.Add("foo");
            sut.Properties.Add("bar", "baz");

            await sut.Log();

            Assume.That(_spyLogger.Messages, Has.Count.EqualTo(1));
            Assert.That(_spyLogger.Messages[0], Is.EqualTo(
                $"UGUIClickOperator operates to Target(foo), bar=baz, screenshot={screenshotOptions.FilenameStrategy.GetFilename()}"));

            var path = Path.Combine(screenshotOptions.Directory, screenshotOptions.FilenameStrategy.GetFilename());
            Assert.That(path, Does.Exist.IgnoreDirectories);
        }

        [Test]
        public void BuildMessage_WithoutCommentsAndProperties_AttachedInstanceID()
        {
            var sut = CreateOperationLogger();
            var actual = sut.BuildMessage();
            Assert.That(actual, Does.Match(@"UGUIClickOperator operates to Target\(-?\d+\)"));
        }

        [Test]
        public void BuildMessage_WithComment()
        {
            var sut = CreateOperationLogger();
            sut.Comments.Add("foo");

            var actual = sut.BuildMessage();

            Assert.That(actual, Is.EqualTo("UGUIClickOperator operates to Target(foo)"));
        }

        [Test]
        public void BuildMessage_WithMultipleComments()
        {
            var sut = CreateOperationLogger();
            sut.Comments.Add("foo");
            sut.Comments.Add("bar");

            var actual = sut.BuildMessage();

            Assert.That(actual, Is.EqualTo("UGUIClickOperator operates to Target(foo, bar)"));
        }

        [Test]
        public void BuildMessage_WithProperty()
        {
            var sut = CreateOperationLogger();
            sut.Properties.Add("bar", "baz");

            var actual = sut.BuildMessage();

            Assert.That(actual, Does.Match(@"UGUIClickOperator operates to Target\(-?\d+\), bar=baz"));
        }

        [Test]
        public void BuildMessage_WithMultipleProperties()
        {
            var sut = CreateOperationLogger();
            sut.Properties.Add("screen-pos", new Vector2(1.0f, 2.0f));
            sut.Properties.Add("world-pos", new Vector3(-3.0f, 4.0f, 5.6789f));

            var actual = sut.BuildMessage();

            Assert.That(actual, Does.Match(
                @"UGUIClickOperator operates to Target\(-?\d+\), screen-pos=\(1,2\), world-pos=\(-3.00,4.00,5.68\)"));
            // Note: screen-pos is formatted as an integer because the screen position
        }
    }
}
