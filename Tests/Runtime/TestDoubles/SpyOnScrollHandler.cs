// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// Scroll event handler spy
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    internal class SpyOnScrollHandler : MonoBehaviour, IScrollHandler
    {
        public bool WasScrolled { get; private set; }
        public Vector2 LastScrollDelta { get; private set; }

        private void Log([CallerMemberName] string member = "")
        {
            Debug.Log($"{this.name}.{member}");
        }

        public void OnScroll(PointerEventData eventData)
        {
            WasScrolled = true;
            LastScrollDelta = eventData.scrollDelta;
            Log();
        }
    }
}
