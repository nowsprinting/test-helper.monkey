// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// OnClick event receiver for UnityEngine.UI.Button
    /// </summary>
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("")] // Hide from "Add Component" picker
    internal class SpyButtonClickReceiver : MonoBehaviour
    {
        private void Log([CallerMemberName] string member = null)
        {
            Debug.Log($"{this.name}.{member}");
        }

        private void ReceiveOnClick() => Log();

        private void Awake()
        {
            var button = this.GetComponent<Button>();
            button.onClick.AddListener(ReceiveOnClick);
        }
    }
}
