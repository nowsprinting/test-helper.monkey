// Copyright (c) 2023-2024 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using TestHelper.Monkey.DefaultStrategies;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Editor.HitTestTool
{
    /// <summary>
    /// Detect interactable or clickable object in the <c>GameView</c>.
    /// Usage: Open this window and click point on the <c>GameView</c> to detect the hit object.
    /// </summary>
    /// <remarks>
    /// Requires <c>EventSystem</c> in the Scene.
    /// And requires <c>Physics2DRaycaster</c> or <c>PhysicsRaycaster</c> in the main camera if detect target is 2D or 3D object.
    /// </remarks>
    public class HitTestWindow : EditorWindow
    {
        /// <summary>
        /// Select the criteria for determining a hit to GameObject.
        /// </summary>
        public HitTestCriteria criteria = HitTestCriteria.Interactable;

        /// <summary>
        /// Automatically select the hit object in the Hierarchy window when a hit is detected.
        /// </summary>
        public bool selectInHierarchy;

        /// <summary>
        /// Output verbose log to console.
        /// </summary>
        public bool verbose;

        private string _hitGameObjectName;
        private string _hitGameObjectPath;
        private Vector2 _hitScreenPosition;
        private float _hitDistance;
        private Vector3 _hitWorldPosition;

        private const float SpacerPixels = 10f;

        private readonly Func<Component, bool> _isInteractable = DefaultComponentInteractableStrategy.IsInteractable;
        private readonly IClickOperator _clickOperator = new UGUIClickOperator();
        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

        [MenuItem("Window/Test Helper/Hit Test Tool")]
        private static void Init()
        {
            var window = GetWindow<HitTestWindow>("Hit Test Tool");
            window.Show();
        }

        private void OnGUI()
        {
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Click the GameView to detect the hit object.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode and click the GameView to detect the hit object.",
                    MessageType.Warning);
            }

            if (!ExistsEventSystem())
            {
                EditorGUILayout.HelpBox("Required EventSystem component in the Scene.",
                    MessageType.Error);
            }

            var physicsRaycasters = FindPhysicsRaycasters();
            if (physicsRaycasters.All(x => x.GetType() != typeof(Physics2DRaycaster)))
            {
                EditorGUILayout.HelpBox(
                    "Required Physics2DRaycaster component with main Camera if detecting a 2D object.",
                    MessageType.Warning);
            }

            if (physicsRaycasters.All(x => x.GetType() != typeof(PhysicsRaycaster)))
            {
                EditorGUILayout.HelpBox(
                    "Required PhysicsRaycaster component with main Camera if detecting a 3D object.",
                    MessageType.Warning);
            }

            GUILayout.Space(SpacerPixels);

            criteria = (HitTestCriteria)EditorGUILayout.EnumPopup(
                new GUIContent("Hit Test Criteria", "Select the criteria for determining a hit to GameObject."),
                criteria);

            selectInHierarchy = EditorGUILayout.Toggle(
                new GUIContent("Select In Hierarchy",
                    "Automatically select the hit object in the Hierarchy window when a hit is detected."),
                selectInHierarchy);

            verbose = EditorGUILayout.Toggle(
                new GUIContent("Verbose", "Output verbose log to console."),
                verbose);

            GUILayout.Space(SpacerPixels);
            EditorGUILayout.LabelField("Hit GameObject and Position (readonly)", EditorStyles.boldLabel);
            EditorGUILayout.TextField("GameObject Name", _hitGameObjectName);
            EditorGUILayout.TextField("GameObject Path", _hitGameObjectPath); // TODO: Click to copy to clipboard using `EditorGUIUtility.systemCopyBuffer`
            EditorGUILayout.TextField("World Position", ToStringOrEmpty(_hitWorldPosition));
            EditorGUILayout.TextField("Screen Position", ToStringOrEmpty(_hitScreenPosition));
            EditorGUILayout.TextField("Distance", ToStringOrEmpty(_hitDistance));
        }

        private static bool ExistsEventSystem()
        {
#if UNITY_2022_3_OR_NEWER
            return FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Any();
            // Note: Supported in Unity 2020.3.4, 2021.3.18, 2022.2.5 or later.
#else
            return GameObject.FindObjectsOfType<EventSystem>().Any();
#endif
        }

        private static PhysicsRaycaster[] FindPhysicsRaycasters()
        {
#if UNITY_2022_3_OR_NEWER
            return FindObjectsByType<PhysicsRaycaster>(FindObjectsSortMode.None);
            // Note: Supported in Unity 2020.3.4, 2021.3.18, 2022.2.5 or later.
#else
            return GameObject.FindObjectsOfType<PhysicsRaycaster>();
#endif
        }

        private static string ToStringOrEmpty<T>(T value)
        {
            if (value.Equals(default(T)))
            {
                return string.Empty;
            }

            return value.ToString();
        }

        private void Update()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            var eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            _raycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, _raycastResults);

            if (_raycastResults.Count == 0)
            {
                LoggingIfVerbose($"No hit GameObject on {eventData.position}");
                ClearHitInformation();
                Repaint();
                return;
            }

            var hitResult = _raycastResults.FirstOrDefault(x => IsHit(x.gameObject));
            if (hitResult.gameObject != null)
            {
                _hitGameObjectName = hitResult.gameObject.name;
                _hitGameObjectPath = hitResult.gameObject.transform.GetPath();
                _hitWorldPosition = hitResult.worldPosition;
                _hitScreenPosition = hitResult.screenPosition;
                _hitDistance = hitResult.distance;
                if (selectInHierarchy)
                {
                    // TODO: Automatically select the hit object in the Hierarchy window.
                }
            }
            else
            {
                LoggingIfVerbose($"No GameObject meet the hit criteria on {eventData.position}");
                ClearHitInformation();
            }

            Repaint();
        }

        private bool IsHit(GameObject gameObject)
        {
            if (criteria == HitTestCriteria.All)
            {
                LoggingIfVerbose($"Hit: {gameObject.name}");
                return true;
            }

            foreach (var component in gameObject.GetComponents<Component>())
            {
                if (criteria == HitTestCriteria.Interactable && _isInteractable.Invoke(component))
                {
                    LoggingIfVerbose($"Hit interactable: {gameObject.name}<{component}>");
                    return true;
                }

                if (criteria == HitTestCriteria.Clickable && _clickOperator.CanOperate(component))
                {
                    LoggingIfVerbose($"Hit clickable: {gameObject.name}<{component}>");
                    return true;
                }
            }

            LoggingIfVerbose($"Not hit criteria: {gameObject.name}");
            return false;
        }

        private void ClearHitInformation()
        {
            _hitGameObjectName = default;
            _hitGameObjectPath = default;
            _hitWorldPosition = default;
            _hitScreenPosition = default;
            _hitDistance = default;
        }

        private void LoggingIfVerbose(string message)
        {
            if (verbose)
            {
                Debug.Log(message);
            }
        }
    }
}
