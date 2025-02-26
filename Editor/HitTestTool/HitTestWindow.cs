// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        /// Hotkey for the hit test.
        /// </summary>
        public KeyCode hotkey = KeyCode.T;

        /// <summary>
        /// Raycast results.
        /// </summary>
        public string results;

        private const float SpacerPixels = 10f;

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

            hotkey = (KeyCode)EditorGUILayout.EnumPopup(
                new GUIContent("Hotkey", "Hotkey for the hit test."),
                hotkey);

            GUILayout.Space(SpacerPixels);
            EditorGUILayout.LabelField("Hit GameObject and Position (readonly)", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(results, GUILayout.Height(400));
        }

        private void Update()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            if (IsEnabledLegacyInputManager)
            {
                if (Input.GetKeyDown(hotkey))
                {
                    HitTest();
                    Repaint();
                }
            }
            else
            {
#if ENABLE_INPUT_SYSTEM
                // Project Settings > Player > Active Input Handling is "Input System (New)" or "Both".
                if (Keyboard.current[FromKeyCode(hotkey)].wasPressedThisFrame)
                {
                    HitTest();
                    Repaint();
                }
#endif
            }
        }

        private void HitTest()
        {
            var pointerEventData = new PointerEventData(EventSystem.current) { position = GetMousePosition() };
            var raycastResults = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            if (raycastResults.Count == 0)
            {
                results = $"No hit GameObject on {pointerEventData.position}";
                return;
            }

            var builder = new StringBuilder();
            foreach (var raycastResult in raycastResults)
            {
                builder.AppendLine(raycastResult.ToString());
                builder.AppendLine("---");
            }

            results = builder.ToString(0, builder.Length - 3);
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

        private static bool IsEnabledLegacyInputManager =>
            EventSystem.current.currentInputModule is StandaloneInputModule;

#if ENABLE_INPUT_SYSTEM
        // Project Settings > Player > Active Input Handling is "Input System (New)" or "Both".
        private static Key FromKeyCode(KeyCode key)
        {
            return (Key)Enum.Parse(typeof(Key), key.ToString());
        }
#endif

        private static Vector2 GetMousePosition()
        {
            if (IsEnabledLegacyInputManager)
            {
                return (Vector2)Input.mousePosition;
            }
            else
            {
#if ENABLE_INPUT_SYSTEM
                // Project Settings > Player > Active Input Handling is "Input System (New)" or "Both".
                return Mouse.current.position.value;
#endif
            }
        }
    }
}
