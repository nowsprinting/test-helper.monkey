// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// Pointer down event handler
    /// </summary>
    [AddComponentMenu("/")] // Hide from "Add Component" picker
    public class StubDestroyingItselfWhenPointerDown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            var nameToKeepAfterGetDestroy = this.name;
            Debug.Log($"{nameToKeepAfterGetDestroy}.{nameof(OnPointerDown)}");
            Destroy(this.gameObject);
            Debug.Log($"{nameToKeepAfterGetDestroy}.{nameof(Destroy)}");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log($"{this.name}.{nameof(OnPointerUp)}");
        }
    }
}
