// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Operators
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [TestFixture]
    public class LongTapOperatorTest
    {
        [SetUp]
        public async Task SetUp()
        {
#if UNITY_EDITOR
            await UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Operators.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
#endif
        }

        [TestCase("UsingOnPointerDownUpHandler", "OnPointerDown", "OnPointerUp")]
        [TestCase("UsingPointerDownUpEventTrigger", "ReceivePointerDown", "ReceivePointerUp")]
        public async Task LongTap(string targetName, string expectedMessage1, string expectedMessage2)
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanLongTap(), Is.True);
            await target.LongTap();
            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage1}");
            LogAssert.Expect(LogType.Log, $"{targetName}.{expectedMessage2}");
        }

        [TestCase("UsingOnPointerClickHandler")]
        [TestCase("UsingPointerClickEventTrigger")]
        public void CanNotLongTap(string targetName)
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == targetName);

            Assert.That(target.CanLongTap(), Is.False);
        }
    }
}
