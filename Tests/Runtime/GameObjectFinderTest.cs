// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey
{
    [TestFixture]
    public class GameObjectFinderTest
    {
        private readonly GameObjectFinder _finder = new GameObjectFinder() { SecondsToWait = 0.1d };
        private const string Name = "Target";

        [Test]
        [CreateScene]
        public async Task FindByNameAsync_InactiveObject_TimeoutException()
        {
            var gameObject = new GameObject(Name);
            gameObject.SetActive(false);

            try
            {
                await _finder.FindByNameAsync(Name);
                Assert.Fail("Expected TimeoutException but was not thrown");
            }
            catch (TimeoutException)
            {
                // Success
            }
        }

        [Test]
        [CreateScene]
        public async Task FindByNameAsync_ActiveObject_GotObject()
        {
            var gameObject = new GameObject(Name);

            var actual = await _finder.FindByNameAsync(Name);
            Assert.That(actual, Is.EqualTo(gameObject));
        }

        [TestFixture]
        public class CreateReachableGameObjectFinder
        {
            private readonly GameObjectFinder _finder = GameObjectFinder.CreateReachableGameObjectFinder(0.1d);

            [Test]
            [CreateScene]
            public async Task FindByNameAsync_NotSelectableObject_TimeoutException()
            {
                var gameObject = new GameObject(Name);

                try
                {
                    await _finder.FindByNameAsync(Name);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException)
                {
                    // Success
                }
            }

            [Test]
            [CreateScene]
            public async Task FindByNameAsync_NotInteractiveObject_TimeoutException()
            {
                var gameObject = new GameObject(Name);
                var button = gameObject.AddComponent<Button>();
                button.interactable = false;

                try
                {
                    await _finder.FindByNameAsync(Name);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException)
                {
                    // Success
                }
            }

            [Test]
            [CreateScene(camera: true)]
            public async Task FindByNameAsync_RaycastNotReachObject_TimeoutException()
            {
                var gameObject = new GameObject(Name);
                var button = gameObject.AddComponent<Button>();

                // TODO: なにか手前に置く

                try
                {
                    await _finder.FindByNameAsync(Name);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException)
                {
                    // Success
                }
            }

            [Test]
            [CreateScene(camera: true)]
            public async Task FindByNameAsync_RaycastReachObject_GotObject()
            {
                var gameObject = new GameObject(Name);
                gameObject.AddComponent<Button>();

                var actual = await _finder.FindByNameAsync(Name);
                Assert.That(actual, Is.EqualTo(gameObject));
            }
        }

        [TestFixture]
        public class CreateInteractiveGameObjectFinder
        {
            private readonly GameObjectFinder _finder = GameObjectFinder.CreateInteractiveGameObjectFinder(0.1d);

            [Test]
            [CreateScene]
            public async Task FindByNameAsync_NotSelectableObject_TimeoutException()
            {
                var gameObject = new GameObject(Name);

                try
                {
                    await _finder.FindByNameAsync(Name);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException)
                {
                    // Success
                }
            }

            [Test]
            [CreateScene]
            public async Task FindByNameAsync_NotInteractiveObject_TimeoutException()
            {
                var gameObject = new GameObject(Name);
                var button = gameObject.AddComponent<Button>();
                button.interactable = false;

                try
                {
                    await _finder.FindByNameAsync(Name);
                    Assert.Fail("Expected TimeoutException but was not thrown");
                }
                catch (TimeoutException)
                {
                    // Success
                }
            }

            [Test]
            [CreateScene(camera: true)]
            public async Task FindByNameAsync_RaycastNotReachObject_GotObject()
            {
                var gameObject = new GameObject(Name);
                gameObject.AddComponent<Button>();

                // TODO: なにか手前に置く

                var actual = await _finder.FindByNameAsync(Name);
                Assert.That(actual, Is.EqualTo(gameObject));
            }
        }
    }
}
