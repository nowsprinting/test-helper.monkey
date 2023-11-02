// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Annotations;
using TestHelper.Monkey.Operators;
using TestHelper.Monkey.ScreenPointStrategies;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.Monkey
{
    /// <summary>
    /// Wrapped component that provide interaction for user.
    /// </summary>
    public class InteractiveComponent : MonoBehaviour
    {
        private InteractiveState _state = InteractiveState.None;
        private string _operationLabel = null;

        /// <summary>
        /// Inner component (EventTrigger or implements IEventSystemHandler)
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [Obsolete]
        public MonoBehaviour component => this;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="component"></param>
        [Obsolete]
        public InteractiveComponent(MonoBehaviour component)
        {
        }

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

        public void UpdateSituation(bool operationTarget = false, string operationLabel = null)
        {
            if (operationTarget)
            {
                _state = InteractiveState.OperationTarget;
                _operationLabel = operationLabel;
                return;
            }

            if (gameObject.GetComponent<IgnoreAnnotation>() != null)
            {
                _state = InteractiveState.Ignore;
                return;
            }

            Func<GameObject, Vector2> screenPointFunction = DefaultScreenPointStrategy.GetScreenPoint;
            // TODO: Consider offset annotations

            _state = IsReallyInteractiveFromUser(screenPointFunction)
                ? InteractiveState.Reachable
                : InteractiveState.Unreachable;
        }

        private void OnDrawGizmos()
        {
            switch (_state)
            {
                case InteractiveState.Ignore:
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(transform.position, 0.2f); // TODO: mizaru
                    break;
                case InteractiveState.Unreachable:
                    Gizmos.color = new Color(0xef, 0x81, 0x0f);
                    Gizmos.DrawSphere(transform.position, 0.2f); // TODO: mizaru
                    break;
                case InteractiveState.Reachable:
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(transform.position, 0.2f); // TODO: saru
                    break;
                case InteractiveState.OperationTarget:
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(transform.position, 0.2f); // TODO: saru
                    // TODO: with draw _operationLabel
                    break;
            }
        }

        /// <summary>
        /// Check inner component can receive click event
        /// </summary>
        /// <returns>true: Can click</returns>
        public bool CanClick() => ClickOperator.CanClick(component);

        /// <summary>
        /// Click inner component
        /// </summary>
        /// <param name="screenPointStrategy">Function returns the screen position where monkey operators operate on for the specified gameObject</param>
        public void Click(Func<GameObject, Vector2> screenPointStrategy) =>
            ClickOperator.Click(component, screenPointStrategy);

        /// <summary>
        /// Check inner component can receive tap (click) event
        /// </summary>
        /// <returns>true: Can tap</returns>
        public bool CanTap() => ClickOperator.CanClick(component);

        /// <summary>
        /// Tap (click) inner component
        /// </summary>
        /// <param name="screenPointStrategy">Function returns the screen position where monkey operators operate on for the specified gameObject</param>
        public void Tap(Func<GameObject, Vector2> screenPointStrategy) =>
            ClickOperator.Click(component, screenPointStrategy);

        /// <summary>
        /// Check inner component can receive touch-and-hold event
        /// </summary>
        /// <returns>true: Can touch-and-hold</returns>
        public bool CanTouchAndHold() => TouchAndHoldOperator.CanTouchAndHold(component);

        /// <summary>
        /// Touch-and-hold inner component
        /// </summary>
        /// <param name="screenPointStrategy">Function returns the screen position where monkey operators operate on for the specified gameObject</param>
        /// <param name="delayMillis">Delay time between down to up</param>
        /// <param name="cancellationToken">Task cancellation token</param>
        public async UniTask TouchAndHold(Func<GameObject, Vector2> screenPointStrategy, int delayMillis = 1000,
            CancellationToken cancellationToken = default)
            => await TouchAndHoldOperator.TouchAndHold(component, screenPointStrategy, delayMillis, cancellationToken);

        // TODO: drag, swipe, flick, etc...
    }
}
