// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.TestDoubles
{
    [AddComponentMenu("")] // Hide from "Add Component" picker
    [RequireComponent(typeof(InputField))]
    public class SpyInputFieldValidator : MonoBehaviour
    {
        private InputField _inputField;
        private Regex _pattern;

        public void SetValidPattern(string pattern)
        {
            _pattern = new Regex(pattern);
        }

        private void Start()
        {
            _inputField = GetComponent<InputField>();
        }

        private void Update()
        {
            if (_inputField.text == string.Empty)
            {
                return;
            }

            if (!_pattern.IsMatch(_inputField.text))
            {
                Debug.LogError($"{gameObject.name}: {_inputField.text} is not match pattern.");
            }
        }
    }
}
