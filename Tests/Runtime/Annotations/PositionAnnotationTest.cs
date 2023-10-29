// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Annotations
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [TestFixture]
    public class PositionAnnotationTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Annotations.unity";
        
        [SetUp]
        public async Task SetUp()
        {
#if UNITY_EDITOR
            await UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Annotations.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
#endif
        }

        [TestCase("WorldOffsetAnnotation")]
        [TestCase("ScreenOffsetAnnotation")]
        [TestCase("WorldPositionAnnotation")]
        [TestCase("ScreenPositionAnnotation")]
        [LoadScene(TestScene)]
        public void IsReallyInteractive(string name)
        {
            var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == name);
            
            // Without no position annotations, IsReallyInteractiveFromUser() is always false because
            // gameObject.transform.position is not in the mesh. So IsReallyInteractiveFromUser() is true means
            // the position annotation work well
            Assert.That(target.IsReallyInteractiveFromUser(), Is.True);
        }
    }
}
