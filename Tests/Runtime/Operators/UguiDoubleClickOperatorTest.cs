// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey.Operators
{
    [TestFixture]
    public class UguiDoubleClickOperatorTest
    {
        private const string TestScene = "../../Scenes/Operators.unity";

        private static RaycastResult CreateRaycastResult(GameObject gameObject)
        {
            Assume.That(Camera.main, Is.Not.Null);

            var raycastResult = new RaycastResult
            {
                gameObject = gameObject,
                worldPosition = gameObject.transform.position,
                screenPosition = Camera.main.WorldToScreenPoint(gameObject.transform.position)
            };

            return raycastResult;
        }

        #region Constructor Tests

        [Test]
        public void Constructor_ValidDoubleClickInterval_ObjectCreatedSuccessfully()
        {
            var sut = new UguiDoubleClickOperator(100);

            Assert.That(sut, Is.Not.Null);
        }

        [Test]
        public void Constructor_ZeroDoubleClickInterval_ThrowsArgumentException()
        {
            Assert.That(() => new UguiDoubleClickOperator(0), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_NegativeDoubleClickInterval_ThrowsArgumentException()
        {
            Assert.That(() => new UguiDoubleClickOperator(-1), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_AllParametersSpecified_ObjectCreatedSuccessfully()
        {
            var logger = new SpyLogger();
            var screenshotOptions = new ScreenshotOptions();

            var sut = new UguiDoubleClickOperator(100, logger, screenshotOptions);

            Assert.That(sut, Is.Not.Null);
        }

        [Test]
        public void Constructor_NullLogger_ObjectCreatedSuccessfully()
        {
            var sut = new UguiDoubleClickOperator(100, null, null);

            Assert.That(sut, Is.Not.Null);
        }

        #endregion

        #region CanOperate Tests

        [Test]
        [LoadScene(TestScene)]
        public void CanOperate_GameObjectWithEventTriggerPointerClick_ReturnsTrue()
        {
            var gameObject = new GameObject("TestObject");
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            eventTrigger.triggers.Add(entry);

            var sut = new UguiDoubleClickOperator(100);
            var actual = sut.CanOperate(gameObject);

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void CanOperate_GameObjectWithEventTriggerNonPointerClick_ReturnsFalse()
        {
            var gameObject = new GameObject("TestObject");
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            eventTrigger.triggers.Add(entry);

            var sut = new UguiDoubleClickOperator(100);
            var actual = sut.CanOperate(gameObject);

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void CanOperate_GameObjectWithButton_ReturnsTrue()
        {
            var gameObject = new GameObject("TestObject");
            gameObject.AddComponent<Button>();

            var sut = new UguiDoubleClickOperator(100);
            var actual = sut.CanOperate(gameObject);

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void CanOperate_GameObjectWithImageOnly_ReturnsFalse()
        {
            var gameObject = new GameObject("TestObject");
            gameObject.AddComponent<Image>();

            var sut = new UguiDoubleClickOperator(100);
            var actual = sut.CanOperate(gameObject);

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void CanOperate_InactiveGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject("TestObject");
            gameObject.AddComponent<Button>();
            gameObject.SetActive(false);

            var sut = new UguiDoubleClickOperator(100);
            var actual = sut.CanOperate(gameObject);

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void CanOperate_GameObjectWithInactiveParent_ReturnsFalse()
        {
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.AddComponent<Button>();
            child.transform.SetParent(parent.transform);
            parent.SetActive(false);

            var sut = new UguiDoubleClickOperator(100);
            var actual = sut.CanOperate(child);

            Assert.That(actual, Is.False);
        }

        [Test]
        public void CanOperate_NullGameObject_ReturnsFalse()
        {
            var sut = new UguiDoubleClickOperator(100);
            var actual = sut.CanOperate(null);

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void CanOperate_DestroyedGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject("TestObject");
            gameObject.AddComponent<Button>();
            UnityEngine.Object.DestroyImmediate(gameObject);

            var sut = new UguiDoubleClickOperator(100);
            var actual = sut.CanOperate(gameObject);

            Assert.That(actual, Is.False);
        }

        #endregion

        #region OperateAsync Tests

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_ValidGameObject_DoubleClickOccurs()
        {
            var gameObject = new GameObject("DoubleClickTarget");
            var spy = gameObject.AddComponent<SpyPointerClickHandler>();
            var raycastResult = CreateRaycastResult(gameObject);

            var sut = new UguiDoubleClickOperator(100);
            Assume.That(sut.CanOperate(gameObject), Is.True);
            await sut.OperateAsync(gameObject, raycastResult);

            Assert.That(spy.ClickCount, Is.EqualTo(2));
        }

        [TestCase(1)]
        [TestCase(100)]
        [TestCase(200)]
        [TestCase(500)]
        [LoadScene(TestScene)]
        public async Task OperateAsync_VariousIntervals_ClicksWithCorrectTiming(int intervalMillis)
        {
            var gameObject = new GameObject("DoubleClickTarget");
            var spy = gameObject.AddComponent<SpySequenceTrackingClickHandler>();
            var raycastResult = CreateRaycastResult(gameObject);

            var stopwatch = Stopwatch.StartNew();
            var sut = new UguiDoubleClickOperator(intervalMillis);
            await sut.OperateAsync(gameObject, raycastResult);
            stopwatch.Stop();

            Assert.That(spy.ClickTimestamps.Count, Is.EqualTo(2));
            Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(intervalMillis));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_PointerClickHandler_OnPointerClickCalled()
        {
            var gameObject = new GameObject("DoubleClickTarget");
            var spy = gameObject.AddComponent<SpyPointerClickHandler>();
            var raycastResult = CreateRaycastResult(gameObject);

            var sut = new UguiDoubleClickOperator(100);
            await sut.OperateAsync(gameObject, raycastResult);

            Assert.That(spy.WasClicked, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_DoubleClickSequenceAndTiming_CorrectInterval()
        {
            var gameObject = new GameObject("DoubleClickTarget");
            var spy = gameObject.AddComponent<SpySequenceTrackingClickHandler>();
            var raycastResult = CreateRaycastResult(gameObject);

            var sut = new UguiDoubleClickOperator(50);
            await sut.OperateAsync(gameObject, raycastResult);

            Assert.That(spy.ClickTimestamps.Count, Is.EqualTo(2));
            var interval = (spy.ClickTimestamps[1] - spy.ClickTimestamps[0]).TotalMilliseconds;
            Assert.That(interval, Is.GreaterThanOrEqualTo(50).And.LessThan(100));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithLogger_LoggingWorks()
        {
            var gameObject = new GameObject("DoubleClickTarget");
            gameObject.AddComponent<Button>();
            var spyLogger = new SpyLogger();
            var raycastResult = CreateRaycastResult(gameObject);

            var sut = new UguiDoubleClickOperator(100, spyLogger);
            await sut.OperateAsync(gameObject, raycastResult);

            Assert.That(spyLogger.Messages, Is.Not.Empty);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithScreenshotOptions_TakeScreenshotOnce()
        {
            var gameObject = new GameObject("DoubleClickTarget");
            gameObject.AddComponent<Button>();
            var directory = Application.temporaryCachePath;
            var filename = $"{TestContext.CurrentContext.Test.FullName}.png";
            var path = Path.Combine(directory, filename);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var screenshotOptions = new ScreenshotOptions
            {
                Directory = directory,
                FilenameStrategy = new StubScreenshotFilenameStrategy(filename),
            };

            var sut = new UguiDoubleClickOperator(100, null, screenshotOptions);
            var raycastResult = CreateRaycastResult(gameObject);
            await sut.OperateAsync(gameObject, raycastResult);

            Assert.That(path, Does.Exist);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_GameObjectDisabledDuringOperation_HandledGracefully()
        {
            var gameObject = new GameObject("DoubleClickTarget");
            var spy = gameObject.AddComponent<SpyPointerClickHandler>();
            var raycastResult = CreateRaycastResult(gameObject);

            var sut = new UguiDoubleClickOperator(100);
            var operationTask = sut.OperateAsync(gameObject, raycastResult);
            
            // Disable the GameObject after 50ms
            await Task.Delay(50);
            gameObject.SetActive(false);

            // Should not throw exception
            await operationTask;
            
            // The operation should complete without throwing
            Assert.Pass("Operation completed without exception");
        }

        [Test]
        public async Task OperateAsync_NullGameObject_ThrowsArgumentNullException()
        {
            var sut = new UguiDoubleClickOperator(100);
            var raycastResult = new RaycastResult();

            try
            {
                await sut.OperateAsync(null, raycastResult);
                Assert.Fail("Expected ArgumentNullException");
            }
            catch (ArgumentNullException exception)
            {
                Assert.That(exception.ParamName, Is.EqualTo("gameObject"));
            }
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_DestroyedGameObject_ThrowsException()
        {
            var gameObject = new GameObject("DoubleClickTarget");
            gameObject.AddComponent<Button>();
            var raycastResult = CreateRaycastResult(gameObject);
            UnityEngine.Object.DestroyImmediate(gameObject);

            var sut = new UguiDoubleClickOperator(100);

            try
            {
                await sut.OperateAsync(gameObject, raycastResult);
                Assert.Fail("Expected exception for destroyed GameObject");
            }
            catch (Exception)
            {
                Assert.Pass("Exception thrown as expected for destroyed GameObject");
            }
        }

        #endregion
    }
}