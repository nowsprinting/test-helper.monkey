// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;

namespace TestHelper.Monkey.DefaultStrategies
{
    [TestFixture]
    public class DefaultComponentInteractableStrategyTest
    {
        private const string TestScene = "../../Scenes/PhysicsRaycasterSandbox.unity";

        [Test]
        public void IsInteractable_NotInteractableComponent_ReturnsFalse()
        {
            var component = new GameObject().AddComponent<MeshCollider>();
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(component), Is.False);
        }

        [Test]
        public void IsInteractable_InteractableComponent_ReturnsTrue()
        {
            var component = new GameObject().AddComponent<SpyOnPointerClickHandler>();
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(component), Is.True);
        }

        [Test]
        public void IsInteractable_DisabledInteractableComponent_ReturnsFalse()
        {
            var component = new GameObject().AddComponent<SpyOnPointerClickHandler>();
            component.enabled = false;

            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(component), Is.False);
        }
    }
}
