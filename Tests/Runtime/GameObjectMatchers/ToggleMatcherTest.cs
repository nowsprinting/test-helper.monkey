// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.GameObjectMatchers
{
    [TestFixture]
    public class ToggleMatcherTest
    {
        [Test]
        public void ToString_WithoutArguments_ReturnsOnlyType()
        {
            var sut = new ToggleMatcher();
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.UI.Toggle"));
        }

        [Test]
        public void ToString_WithName_ReturnsWithName()
        {
            var sut = new ToggleMatcher(name: "toggle");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.UI.Toggle, name=toggle"));
        }

        [Test]
        public void ToString_WithPath_ReturnsWithPath()
        {
            var sut = new ToggleMatcher(path: "/Path/To/Toggle");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.UI.Toggle, path=/Path/To/Toggle"));
        }

        [Test]
        public void ToString_WithText_ReturnsWithText()
        {
            var sut = new ToggleMatcher(text: "Click Me");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.UI.Toggle, text=Click Me"));
        }

        [Test]
        public void IsMatch_NotMatchComponentType_ReturnsFalse()
        {
            var sut = new ToggleMatcher();              // UnityEngine.UI.Toggle
            var actual = sut.IsMatch(new GameObject()); // No Toggle component
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_NotMatchName_ReturnsFalse()
        {
            var sut = new ToggleMatcher(name: "toggle");
            var actual = sut.IsMatch(CreateToggle(name: "not_toggle"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_NotMatchPath_ReturnsFalse()
        {
            var sut = new ToggleMatcher(path: "/Path/To/Toggle");
            var actual = sut.IsMatch(CreateToggle(path: "/Path/To/Not/Toggle"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_NotMatchText_ReturnsFalse()
        {
            var sut = new ToggleMatcher(text: "Click Me");
            var actual = sut.IsMatch(CreateToggle(text: "Not Click Me"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_NotMatchTmpText_ReturnsFalse()
        {
            var sut = new ToggleMatcher(text: "Click Me");
            var actual = sut.IsMatch(CreateToggle(tmpText: "Not Click Me"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_MatchAllProperties_ReturnsTrue()
        {
            var sut = new ToggleMatcher(
                componentType: typeof(Toggle),
                name: "Toggle",
                path: "/Path/To/Toggle",
                text: "Click Me");
            var actual = sut.IsMatch(CreateToggle(
                componentType: typeof(Toggle),
                name: "Toggle",
                path: "/Path/To/Toggle",
                text: "Click Me"));
            Assert.That(actual, Is.True);
        }

        [Test]
        public void IsMatch_MatchAllPropertiesWithTmpText_ReturnsTrue()
        {
            var sut = new ToggleMatcher(
                componentType: typeof(Toggle),
                name: "Toggle",
                path: "/Path/To/Toggle",
                text: "Click Me");
            var actual = sut.IsMatch(CreateToggle(
                componentType: typeof(Toggle),
                name: "Toggle",
                path: "/Path/To/Toggle",
                tmpText: "Click Me"));
            Assert.That(actual, Is.True);
        }

        private static GameObject CreateToggle(Type componentType = null, string name = null, string path = null,
            string text = null, string tmpText = null)
        {
            var gameObject = new GameObject();

            if (path != null)
            {
                using var enumerator = path.Split("/").Reverse().GetEnumerator();
                enumerator.MoveNext();
                gameObject.name = enumerator.Current!;
                var lastGameObject = gameObject;
                while (enumerator.MoveNext())
                {
                    var node = enumerator.Current!;
                    if (string.IsNullOrEmpty(node))
                    {
                        continue;
                    }

                    var parent = new GameObject(node);
                    lastGameObject.transform.SetParent(parent.transform);
                    lastGameObject = parent;
                }
            }

            if (name != null)
            {
                gameObject.name = name; // Note: Allow it to be overwritten
            }

            if (componentType != null)
            {
                gameObject.AddComponent(componentType);
            }

            if (text != null)
            {
                var textComponent = new GameObject().AddComponent<Text>();
                textComponent.transform.SetParent(gameObject.transform);
                textComponent.text = text;
            }

            if (tmpText != null)
            {
                var textComponent = new GameObject().AddComponent<TextMeshProUGUI>();
                textComponent.transform.SetParent(gameObject.transform);
                textComponent.SetText(tmpText);
            }

            return gameObject;
        }
    }
}
