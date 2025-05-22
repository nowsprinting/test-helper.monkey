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
    public class ButtonMatcherTest
    {
        [Test]
        public void ToString_WithoutArguments_ReturnsOnlyType()
        {
            var sut = new ButtonMatcher();
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.UI.Button"));
        }

        [Test]
        public void ToString_WithName_ReturnsWithName()
        {
            var sut = new ButtonMatcher(name: "button");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.UI.Button, name=button"));
        }

        [Test]
        public void ToString_WithPath_ReturnsWithPath()
        {
            var sut = new ButtonMatcher(path: "/Path/To/Button");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.UI.Button, path=/Path/To/Button"));
        }

        [Test]
        public void ToString_WithText_ReturnsWithText()
        {
            var sut = new ButtonMatcher(text: "Click Me");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.UI.Button, text=Click Me"));
        }

        [Test]
        public void ToString_WithTexture_ReturnsWithTexture()
        {
            var sut = new ButtonMatcher(texture: "button_texture");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.UI.Button, texture=button_texture"));
        }

        [Test]
        public void IsMatch_NotMatchComponentType_ReturnsFalse()
        {
            var sut = new ButtonMatcher();              // UnityEngine.UI.Button
            var actual = sut.IsMatch(new GameObject()); // No Button component
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_NotMatchName_ReturnsFalse()
        {
            var sut = new ButtonMatcher(name: "button");
            var actual = sut.IsMatch(CreateButton(name: "not_button"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_NotMatchPath_ReturnsFalse()
        {
            var sut = new ButtonMatcher(path: "/Path/To/Button");
            var actual = sut.IsMatch(CreateButton(path: "/Path/To/Not/Button"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_NotMatchText_ReturnsFalse()
        {
            var sut = new ButtonMatcher(text: "Click Me");
            var actual = sut.IsMatch(CreateButton(text: "Not Click Me"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_NotMatchTmpText_ReturnsFalse()
        {
            var sut = new ButtonMatcher(text: "Click Me");
            var actual = sut.IsMatch(CreateButton(tmpText: "Not Click Me"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_NotMatchTexture_ReturnsFalse()
        {
            var sut = new ButtonMatcher(texture: "not_builtin_sprite");
            var actual = sut.IsMatch(CreateButton(texture: "unity_builtin_extra"));
            Assert.That(actual, Is.False);
        }

        [Test]
        public void IsMatch_MatchAllProperties_ReturnsTrue()
        {
            var sut = new ButtonMatcher(
                componentType: typeof(Button),
                name: "Button",
                path: "/Path/To/Button",
                text: "Click Me",
                texture: "test_sprite");
            var actual = sut.IsMatch(CreateButton(
                componentType: typeof(Button),
                name: "Button",
                path: "/Path/To/Button",
                text: "Click Me",
                texture: "test_sprite"));
            Assert.That(actual, Is.True);
        }

        [Test]
        public void IsMatch_MatchAllPropertiesWithTmpText_ReturnsTrue()
        {
            var sut = new ButtonMatcher(
                componentType: typeof(Button),
                name: "Button",
                path: "/Path/To/Button",
                text: "Click Me",
                texture: "test_sprite");
            var actual = sut.IsMatch(CreateButton(
                componentType: typeof(Button),
                name: "Button",
                path: "/Path/To/Button",
                tmpText: "Click Me",
                texture: "test_sprite"));
            Assert.That(actual, Is.True);
        }

        private static GameObject CreateButton(Type componentType = null, string name = null, string path = null,
            string text = null, string tmpText = null, string texture = null)
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

            if (texture != null)
            {
                var imageComponent = gameObject.AddComponent<Image>();
                imageComponent.sprite = CreateSprite(texture);
            }

            return gameObject;
        }

        private static Sprite CreateSprite(string name, int edgeSize = 1)
        {
            var sprite = Sprite.Create(new Texture2D(edgeSize, edgeSize),
                new Rect(Vector2.zero, new Vector2(edgeSize, edgeSize)), Vector2.zero);
            sprite.name = name;
            return sprite;
        }
    }
}
