// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Operators;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Extensions
{
    /// <summary>
    /// A utility to select Camera
    /// </summary>
    public static class GameObjectExtensions
    {
        private static Camera s_cachedMainCamera;
        private static int s_cachedFrame;

        /// <summary>
        /// Returns the first enabled Camera component that is tagged "MainCamera"
        /// </summary>
        /// <returns>The first enabled Camera component that is tagged "MainCamera"</returns>
        private static Camera GetMainCamera()
        {
            if (Time.frameCount == s_cachedFrame)
            {
                return s_cachedMainCamera;
            }

            s_cachedFrame = Time.frameCount;
            return s_cachedMainCamera = Camera.main;
        }

        /// <summary>
        /// Returns an associated camera with <paramref name="gameObject"/>.
        /// Or return <c cref="Camera.main">Camera.main</c> if there are no camera associated with.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>
        /// Camera associated with <paramref name="gameObject"/>, or return <c cref="Camera.main">Camera.main</c> if there are no camera associated with
        /// </returns>
        public static Camera GetAssociatedCamera(this GameObject gameObject)
        {
            var canvas = gameObject.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return GetMainCamera();
            }

            if (!canvas.isRootCanvas)
            {
                canvas = canvas.rootCanvas;
            }

            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        /// <summary>
        /// Hit test using raycaster
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="isReachable">The function returns the <c>GameObject</c> is reachable from user or not.
        /// Default is <c>DefaultReachableStrategy.IsReachable</c>.</param>
        /// <param name="pointerEventData">Specify if avoid GC memory allocation</param>
        /// <param name="raycastResults">Specify if avoid GC memory allocation</param>
        /// <returns>true: this object can control by user</returns>
        [Obsolete("Use DefaultReachableStrategy instead.")]
        public static bool IsReachable(this GameObject gameObject,
            Func<GameObject, PointerEventData, List<RaycastResult>, ILogger, bool> isReachable = null,
            PointerEventData pointerEventData = null,
            List<RaycastResult> raycastResults = null)
        {
            Assert.IsNotNull(EventSystem.current);

            isReachable = isReachable ?? DefaultReachableStrategy.IsReachable;
            pointerEventData = pointerEventData ?? new PointerEventData(EventSystem.current);
            raycastResults = raycastResults ?? new List<RaycastResult>();

            raycastResults.Clear();
            return isReachable.Invoke(gameObject, pointerEventData, raycastResults, null);
        }

        /// <summary>
        /// Make sure the <c>GameObject</c> is reachable from user.
        /// Hit test using raycaster
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="screenPointStrategy">Function returns the screen position where raycast for the specified GameObject</param>
        /// <param name="eventData">Specify if avoid GC memory allocation</param>
        /// <param name="results">Specify if avoid GC memory allocation</param>
        /// <returns>True if this GameObject is reachable from user</returns>
        [Obsolete("Use DefaultReachableStrategy instead.")]
        public static bool IsReachable(this GameObject gameObject,
            Func<GameObject, Vector2> screenPointStrategy = null,
            PointerEventData eventData = null,
            List<RaycastResult> results = null)
        {
            Assert.IsNotNull(EventSystem.current);

            screenPointStrategy = screenPointStrategy ?? DefaultScreenPointStrategy.GetScreenPoint;

            eventData = eventData ?? new PointerEventData(EventSystem.current);
            eventData.position = screenPointStrategy(gameObject);

            results = results ?? new List<RaycastResult>();
            results.Clear();

            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0 && IsSameOrChildObject(gameObject, results[0].gameObject.transform);
        }

        private static bool IsSameOrChildObject(GameObject target, Transform hitObjectTransform)
        {
            while (hitObjectTransform != null)
            {
                if (hitObjectTransform == target.transform)
                {
                    return true;
                }

                hitObjectTransform = hitObjectTransform.transform.parent;
            }

            return false;
        }

        /// <summary>
        /// Return interactable components.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="isComponentInteractable"></param>
        /// <returns></returns>
        public static IEnumerable<Component> GetInteractableComponents(this GameObject gameObject,
            Func<Component, bool> isComponentInteractable = null)
        {
            isComponentInteractable = isComponentInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;

            return gameObject.GetComponents<Component>().Where(x => isComponentInteractable.Invoke(x));
        }

        /// <summary>
        /// Make sure the <c>GameObject</c> is interactable.
        ///
        /// If any of the following is true:
        /// <list type="number">
        ///   <item> Attached <c>Selectable</c> component and <c>interactable</c> property is true</item>
        ///   <item>Attached <c>EventTrigger</c> component</item>
        ///   <item>Attached component Implements <c>IEventSystemHandler</c> interface</item>
        /// </list>
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>True if this GameObject is interactable</returns>
        [Obsolete("Use DefaultGameObjectInteractableStrategy instead.")]
        public static bool IsInteractable(this GameObject gameObject)
        {
            return gameObject.GetComponents<MonoBehaviour>().Any(x => x.IsInteractable());
        }

        /// <summary>
        /// Try to get a component exclude disabled component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="component">A component of the matching type, if found.</param>
        /// <typeparam name="T">The type of Component to search for</typeparam>
        /// <returns>True if found and active and enabled component</returns>
        public static bool TryGetEnabledComponent<T>(this GameObject gameObject, out T component)
        {
            component = gameObject.GetComponent<T>();
            return component != null && (!(component is Behaviour) || (component as Behaviour).isActiveAndEnabled);
        }

        /// <summary>
        /// Returns the operators available to this component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        /// <returns>Available operators</returns>
        public static IEnumerable<IOperator> SelectOperators(this GameObject gameObject,
            IEnumerable<IOperator> operators)
        {
            return operators.Where(iOperator => iOperator.CanOperate(gameObject));
        }

        /// <summary>
        /// Returns the operators that specify types and are available to this component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        /// <returns>Available operators</returns>
        public static IEnumerable<T> SelectOperators<T>(this GameObject gameObject, IEnumerable<IOperator> operators)
            where T : IOperator
        {
            return operators.OfType<T>().Where(iOperator => iOperator.CanOperate(gameObject));
        }
    }
}
