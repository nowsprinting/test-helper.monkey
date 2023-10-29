// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using System.Linq;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.Monkey.Annotations
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [TestFixture]
    public class PositionAnnotationTest
    {
        private const string TestScene = "Packages/com.nowsprinting.test-helper.monkey/Tests/Scenes/Annotations.unity";

        private static object[][] s_TestCases =>
            new[]
            {
                new object[] { "WorldOffsetAnnotation" },
                new object[] { "ScreenOffsetAnnotation" },
                new object[] { "WorldPositionAnnotation" },
                new object[] { "ScreenPositionAnnotation" }
            };

        [TestCaseSource(nameof(s_TestCases))]
        [LoadScene(TestScene)]
        [GameViewResolution(640, 480, "VGA")]
        public IEnumerator IsReallyInteractive(string name)
        {
            yield return null;

            var target = InteractiveComponentCollector.FindInteractiveComponents(false)
                .First(x => x.gameObject.name == name);

            // Without no position annotations, IsReallyInteractiveFromUser() is always false because
            // gameObject.transform.position is not in the mesh. So IsReallyInteractiveFromUser() is true means
            // the position annotation work well
            Assert.That(target.IsReallyInteractiveFromUser(), Is.True);
        }
    }
}
