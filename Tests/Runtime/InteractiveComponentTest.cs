// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TestHelper.Monkey
{
    [TestFixture]
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    public class InteractiveComponentTest
    {
        /// <summary>
        /// InteractiveComponent test cases using 3D objects
        /// </summary>
        [TestFixture]
        public class ThreeD
        {
            [SetUp]
            public async Task SetUp()
            {
                await EditorSceneManager.LoadSceneAsyncInPlayMode(
                    "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/MonkeyThreeD.unity",
                    new LoadSceneParameters(LoadSceneMode.Single));
            }

            [Test]
            public void Fields()
            {
                var gameObject = GameObject.Find("UsingEventTrigger");
                var component = gameObject.GetComponent<EventTrigger>();
                var sut = new InteractiveComponent(component);

                Assert.That(sut.component, Is.EqualTo(component));
                Assert.That(sut.gameObject, Is.EqualTo(gameObject));
                Assert.That(sut.transform, Is.EqualTo(gameObject.transform));
            }

            [TestCase("UsingEventHandler")] // Implements IPointerClickHandler
            [TestCase("UsingEventTrigger")] // Attached EventTrigger
            [TestCase("ChildInTheSight")] // Parent object is out of sight, but this object is in the sight
            public void IsReallyInteractiveFromUser_reachableObjects_returnTrue(string targetName)
            {
                var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReallyInteractiveFromUser(), Is.True);
            }

            [TestCase("BeyondTheWall")] // Beyond the another object
            [TestCase("OutOfSight")] // Out of sight
            public void IsReallyInteractiveFromUser_unreachableObjects_returnFalse(string targetName)
            {
                var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReallyInteractiveFromUser(), Is.False);
            }
        }

        /// <summary>
        /// InteractiveComponent test cases using UI elements
        /// </summary>
        [TestFixture]
        public class UI
        {
            [SetUp]
            public async Task SetUp()
            {
                await EditorSceneManager.LoadSceneAsyncInPlayMode(
                    "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/MonkeyUiWorldSpace.unity",
                    new LoadSceneParameters(LoadSceneMode.Single));
                await UniTask.NextFrame(); // Wait 1 frame because warmup for GraphicRaycaster
            }

            [TestCase("Button")] // Attached Button
            [TestCase("ChildInTheSight")] // Parent object is out of sight, but this object is in the sight
            [TestCase("ButtonOnInnerCanvas")] // On the inner Canvas
            [Category("IgnoreCI")] // GraphicRaycaster not work on batchmode
            public void IsReallyInteractiveFromUser_reachableObjects_returnTrue(string targetName)
            {
                var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReallyInteractiveFromUser(), Is.True);
            }

            [TestCase("BeyondTheWall")] // Beyond the another object
            [TestCase("OutOfSight")] // Out of sight
            [TestCase("NotInteractable")] // Interactable=false
            [TestCase("BeyondThe2D")] // Beyond the 2D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
            [TestCase("BeyondThe3D")] // Beyond the 3D object (GraphicRaycaster.blockingObjects is BlockingObjects.All)
            public void IsReallyInteractiveFromUser_unreachableObjects_returnFalse(string targetName)
            {
                var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.IsReallyInteractiveFromUser(), Is.False);
            }

            [TestCase("Button", "ReceiveOnClick")]
            public void Tap(string targetName, string expectedMessage)
            {
                var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                    .First(x => x.gameObject.name == targetName);

                Assert.That(target.CanTap(), Is.True);
                target.Tap();

                LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage}");
            }
        }
    }
}
