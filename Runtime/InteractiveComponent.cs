// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Wrapped component that provide interaction for user.
    /// </summary>
    public class InteractiveComponent
    {
        /// <summary>
        /// Inner component (EventTrigger or implements IEventSystemHandler)
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public readonly MonoBehaviour component;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="component"></param>
        public InteractiveComponent(MonoBehaviour component)
        {
            this.component = component;
        }

        /// <summary>
        /// Transform via inner component
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public Transform transform => component.transform;

        /// <summary>
        /// GameObject via inner component
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public GameObject gameObject => component.gameObject;

        /// <summary>
        /// Hit test using raycaster
        /// </summary>
        /// <remarks>
        /// This method does not give correct results about UI elements when run on batchmode.
        /// Because GraphicRaycaster does not work in batchmode.
        /// </remarks>
        /// <param name="screenPointStrategy">Function returns the screen position where monkey operators operate on for the specified gameObject</param>
        /// <param name="eventData">Specify if avoid GC memory allocation</param>
        /// <param name="results">Specify if avoid GC memory allocation</param>
        /// <returns>true: this object can control by user</returns>
        public bool IsReallyInteractiveFromUser(Func<GameObject, Vector2> screenPointStrategy,
            PointerEventData eventData = null, List<RaycastResult> results = null)
        {
            if (!IsInteractable())
            {
                return false;
            }

            eventData = eventData ?? new PointerEventData(EventSystem.current);
            eventData.position = screenPointStrategy(gameObject);

            results = results ?? new List<RaycastResult>();
            results.Clear();

            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0 && IsSameOrChildObject(results[0].gameObject.transform);
        }

        private bool IsInteractable()
        {
            var selectable = this.component as Selectable;
            return selectable == null || selectable.interactable;
        }

        private bool IsSameOrChildObject(Transform hitObjectTransform)
        {
            while (hitObjectTransform != null)
            {
                if (hitObjectTransform == this.transform)
                {
                    return true;
                }

                hitObjectTransform = hitObjectTransform.transform.parent;
            }

            return false;
        }
    }
}
