// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.TestDoubles
{
    /// <summary>
    /// Write LogError when OnPointerUp is called.
    /// </summary>
    [AddComponentMenu("")] // Hide from "Add Component" picker
    public class StubLogErrorWhenOnPointerUp : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"{this.name}.{nameof(OnPointerDown)}");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.LogError($"{this.name}.{nameof(OnPointerUp)}");
        }
    }
}
