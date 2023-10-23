using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Pointer down event handler
    /// </summary>
    [AddComponentMenu("")] // Hide from "Add Component" picker
    public class StubDestroyingItselfWhenPointerDown : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            var nameToKeepAfterGetDestroy = this.name;
            Debug.Log($"{nameToKeepAfterGetDestroy}.{nameof(OnPointerDown)}");
            DestroyImmediate(this.gameObject);
            Debug.Log($"{nameToKeepAfterGetDestroy}.{nameof(DestroyImmediate)}");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log($"{this.name}.{nameof(OnPointerUp)}");
        }
    }
}
