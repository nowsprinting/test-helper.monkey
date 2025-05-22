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
    /// <see cref="GameObject"/> matcher that matchers by <c>Toggle</c> component properties.
    /// </summary>
    /// <remarks>
    /// You can use custom <c>Toggle</c> components as long as they have the same structure as <see cref="Toggle"/>.
    /// Pass the type of your custom <c>Toggle</c> component to the constructor argument <c>componentType</c>.
    /// </remarks>
    public class ToggleMatcher : IGameObjectMatcher
    {
        private readonly Type _componentType;
        private readonly string _name;
        private readonly string _path;
        private readonly string _text;

        /// <summary>
        /// Constructor with properties of <c>Toggle</c>.
        /// </summary>
        /// <param name="componentType"><c>Toggle</c> component type. If omitted, <see cref="Toggle"/> is used.</param>
        /// <param name="name"><see cref="GameObject"/> name</param>
        /// <param name="path"><see cref="GameObject"/> hierarchy path separated by `/`. Can specify glob pattern</param>
        /// <param name="text">text under the <c>Toggle</c></param>
        /// <seealso href="https://en.wikipedia.org/wiki/Glob_(programming)"/>
        public ToggleMatcher(Type componentType = null, string name = null, string path = null, string text = null)
        {
            _componentType = componentType ?? typeof(Toggle);
            _name = name;
            _path = path;
            _text = text;
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
    }
}
