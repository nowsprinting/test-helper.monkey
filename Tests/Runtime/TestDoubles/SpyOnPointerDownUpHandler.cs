// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// Pointer down/up event handler
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    internal class SpyOnPointerDownUpHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private void Log([CallerMemberName] string member = "")
        {
            Debug.Log($"{this.name}.{member}");
        }

        public void OnPointerDown(PointerEventData eventData) => Log();
        public void OnPointerUp(PointerEventData eventData) => Log();
    }
}
