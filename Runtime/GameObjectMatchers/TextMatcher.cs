// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.UI;
#if ENABLE_TMP
using TMPro;
#endif

namespace TestHelper.Monkey.GameObjectMatchers
{
    /// <summary>
    /// GameObject matcher that matchers by text under the Button and Toggle.
    /// </summary>
    public class TextMatcher : IGameObjectMatcher
    {
        // primary predicate
        private readonly string _text;

        // secondary predicates
        private readonly string _name;
        private readonly string _path;
        private readonly string _texture;

        /// <summary>
        /// Constructor with text.
        /// </summary>
        /// <param name="text">text under the Button and Toggle</param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="texture"></param>
        public TextMatcher(string text, string name = null, string path = null, string texture = null)
        {
            _text = text;
            _name = name;
            _path = path;
            _texture = texture;
        }

        /// <inheritdoc/>
        public override string ToString() => $"text={_text}, name={_name}, path={_path}, texture={_texture}";

        /// <inheritdoc/>
        public bool IsMatch(GameObject gameObject)
        {
            var textComponent = gameObject.transform.GetComponentInChildren<Text>();
            if (textComponent)
            {
                return textComponent.text == _text;
                // TODO: name, path, textureも（あれば）見る
            }

#if ENABLE_TMP
            var tmpTextComponent = gameObject.transform.GetComponentInChildren<TMP_Text>();
            if (tmpTextComponent)
            {
                return tmpTextComponent.text == _text;
                // TODO: name, path, textureも（あれば）見る
            }
#endif
            return false;
        }
    }
}
