// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Text;
using TestHelper.Monkey.Extensions;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_TMP
using TMPro;
#endif

namespace TestHelper.Monkey.GameObjectMatchers
{
    /// <summary>
    /// <see cref="GameObject"/> matcher that matchers by <c>Button</c> component properties.
    /// </summary>
    /// <remarks>
    /// You can use custom <c>Button</c> components as long as they have the same structure as <see cref="Button"/>.
    /// Pass the type of your custom <c>Button</c> component to the constructor argument <c>componentType</c>.
    /// </remarks>
    public class ButtonMatcher : IGameObjectMatcher
    {
        private readonly Type _componentType;
        private readonly string _name;
        private readonly string _path;
        private readonly string _text;
        private readonly string _texture;

        /// <summary>
        /// Constructor with properties of <c>Button</c>.
        /// </summary>
        /// <param name="componentType"><c>Button</c> component type. If omitted, <see cref="Button"/> is used.</param>
        /// <param name="name"><see cref="GameObject"/> name</param>
        /// <param name="path"><see cref="GameObject"/> hierarchy path separated by `/`. Can specify glob pattern</param>
        /// <param name="text">text under the <c>Button</c></param>
        /// <param name="texture">texture name under the <c>Button</c></param>
        /// <seealso href="https://en.wikipedia.org/wiki/Glob_(programming)"/>
        public ButtonMatcher(Type componentType = null, string name = null, string path = null, string text = null,
            string texture = null)
        {
            _componentType = componentType ?? typeof(Button);
            _name = name;
            _path = path;
            _text = text;
            _texture = texture;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder($"type={_componentType}");
            if (_name != null)
            {
                builder.Append($", name={_name}");
            }

            if (_path != null)
            {
                builder.Append($", path={_path}");
            }

            if (_text != null)
            {
                builder.Append($", text={_text}");
            }

            if (_texture != null)
            {
                builder.Append($", texture={_texture}");
            }

            return builder.ToString();
        }

        /// <inheritdoc/>
        public bool IsMatch(GameObject gameObject)
        {
            if (!gameObject.TryGetComponent(_componentType, out _))
            {
                return false;
            }

            if (_name != null && gameObject.name != _name)
            {
                return false;
            }

            if (_path != null && !gameObject.transform.MatchPath(_path))
            {
                return false;
            }

            if (_text != null && !IsMatchText(gameObject, _text))
            {
                return false;
            }

            if (_texture != null && !IsMatchTexture(gameObject, _texture))
            {
                return false;
            }

            return true;
        }

        private static bool IsMatchText(GameObject gameObject, string text)
        {
            var textComponent = gameObject.transform.GetComponentInChildren<Text>();
            if (textComponent)
            {
                return textComponent.text == text;
            }

#if ENABLE_TMP
            var tmpTextComponent = gameObject.transform.GetComponentInChildren<TMP_Text>();
            if (tmpTextComponent)
            {
                return tmpTextComponent.text == text;
            }
#endif
            return false;
        }

        private static bool IsMatchTexture(GameObject gameObject, string texture)
        {
            var imageComponent = gameObject.GetComponent<Image>();
            if (imageComponent)
            {
                return imageComponent.sprite.name == texture;
            }

            return false;
        }
    }
}
