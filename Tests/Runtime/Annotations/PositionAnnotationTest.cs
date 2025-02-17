// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Monkey.DefaultStrategies;
using UnityEngine;

namespace TestHelper.Monkey.Annotations
{
    [TestFixture]
    public class PositionAnnotationTest
    {
        [TestFixture]
        public class NotOnScaledCanvas
        {
            private const string TestScene = "../../Scenes/PositionAnnotations.unity";

            private static readonly IEnumerable<string> s_annotations = new[]
            {
                "ScreenOffsetAnnotation", "ScreenPositionAnnotation", "WorldOffsetAnnotation",
                "WorldPositionAnnotation",
            };

            [TestCaseSource(nameof(s_annotations))]
            [LoadScene(TestScene)]
            public void AttachAnnotation_CorrectedToBeReachable(string name)
            {
                var target = GameObject.Find(name);

                // Without no position annotations, IsReachable() is always false because
                // gameObject.transform.position is not in the mesh. So IsReachable() is true means
                // the position annotation work well
                Assert.That(DefaultReachableStrategy.IsReachable(target), Is.True);
            }

            [TestCaseSource(nameof(s_annotations))]
            [LoadScene(TestScene)]
            public void DisabledAnnotation_NotReachable(string name)
            {
                var target = GameObject.Find(name);
                var annotation = target.GetComponents<MonoBehaviour>().First(x => x.GetType().Name.Equals(name));
                annotation.enabled = false;

                Assert.That(DefaultReachableStrategy.IsReachable(target), Is.False);
            }
        }

        [TestFixture]
        public class OnScaledCanvas
        {
            private const string TestScene = "../../Scenes/ScaledCanvas.unity";

            private static readonly IEnumerable<string> s_annotations = new[]
            {
                "ScreenOffsetAnnotation", // The offset is within the GameObject's rect on XGA
                "ScreenPositionAnnotation", // The position is within the GameObject's rect on XGA
            };

            [TestCaseSource(nameof(s_annotations))]
            [LoadScene(TestScene)]
            [CanBeNull]
            public async Task AttachAnnotation_OnScaledCanvas_CorrectedToBeReachable(string name)
            {
                await UniTask.NextFrame();
                var target = GameObject.Find(name);

                Assert.That(DefaultReachableStrategy.IsReachable(target, verboseLogger: Debug.unityLogger), Is.True);
                // The offset/position is within the GameObject's rect on XGA display.
                // If the annotation's properties don't consider CanvasScaler, the specified position will be outside the GameObject's rect on VGA display.
                // Then IsReachable will return false.
            }
        }
    }
}
