// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// Pointer enter/exit event handler spy
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    internal class SpyOnPointerEnterExitHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool WasPointerEntered { get; private set; }
        public bool WasPointerExited { get; private set; }

        private void Log([CallerMemberName] string member = "")
        {
            Debug.Log($"{this.name}.{member}");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            WasPointerEntered = true;
            Log();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            WasPointerExited = true;
            Log();
        }
    }
}
