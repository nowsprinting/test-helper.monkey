// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// Pointer click event handler
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    internal class SpyOnPointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        private void Log([CallerMemberName] string member = "")
        {
            Debug.Log($"{this.name}.{member}");
        }

        public void OnPointerClick(PointerEventData eventData) => Log();
    }
}
