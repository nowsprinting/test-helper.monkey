// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators;
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
        /// <param name="eventData">Specify if avoid GC memory allocation</param>
        /// <param name="results">Specify if avoid GC memory allocation</param>
        /// <returns>true: this object can control by user</returns>
        public bool IsReallyInteractiveFromUser(PointerEventData eventData = null, List<RaycastResult> results = null)
        {
            if (!IsInteractable())
            {
                return false;
            }

            eventData = eventData ?? new PointerEventData(EventSystem.current);
            eventData.position = gameObject.GetScreenPoint();

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

        /// <summary>
        /// Check inner component can receive click event
        /// </summary>
        /// <returns>true: Can click</returns>
        public bool CanClick() => ClickOperator.CanClick(component);

        /// <summary>
        /// Click inner component
        /// </summary>
        public void Click() => ClickOperator.Click(component);

        /// <summary>
        /// Check inner component can receive tap (click) event
        /// </summary>
        /// <returns>true: Can tap</returns>
        public bool CanTap() => ClickOperator.CanClick(component);

        /// <summary>
        /// Tap (click) inner component
        /// </summary>
        public void Tap() => ClickOperator.Click(component);

        /// <summary>
        /// Check inner component can receive touch-and-hold event
        /// </summary>
        /// <returns>true: Can touch-and-hold</returns>
        public bool CanTouchAndHold() => TouchAndHoldOperator.CanTouchAndHold(component);

        /// <summary>
        /// Touch-and-hold inner component
        /// </summary>
        /// <param name="delayMillis">Delay time between down to up</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        public async UniTask TouchAndHold(int delayMillis = 1000, CancellationToken cancellationToken = default)
            => await TouchAndHoldOperator.TouchAndHold(component, delayMillis, cancellationToken);

        // TODO: drag, swipe, flick, etc...
    }
}
