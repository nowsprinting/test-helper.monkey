// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEditor;
using UnityEngine;

namespace TestHelper.Monkey.Editor.Hints
{
    /// <summary>
    /// Custom editor for <c cref="InteractiveComponentHint" />
    /// </summary>
    [CustomEditor(typeof(InteractiveComponentHint))]
    public class InteractiveComponentHintEditor : UnityEditor.Editor
    {
        private static readonly string s_activePointColor = L10n.Tr("Active Point");

        private static readonly string s_activePointTooltip =
            L10n.Tr("Color for interactive components that user can operate");

        private SerializedProperty _activePointColorProp;
        private GUIContent _activePointColorContent;

        private static readonly string s_inactiveColor = L10n.Tr("Inactive Point");

        private static readonly string s_inactiveTooltip =
            L10n.Tr("Color for interactive components that user cannot operate");

        private SerializedProperty _inactivePointColorProp;
        private GUIContent _inactivePointColorContent;

        private static readonly string s_originalPointColor = L10n.Tr("Original Point");

        private static readonly string s_originalPointTooltip = L10n.Tr(
            "Color for the default operation point for interactive components that is used for the origin point of position annotations"
        );

        private SerializedProperty _originalPointColorProp;
        private GUIContent _originalPointColorContent;

        private static readonly string s_refreshFrameCount = L10n.Tr("Frames/Refresh");
        private static readonly string s_refreshFrameCountTooltip = L10n.Tr("Number of frames per refresh hints");
        private SerializedProperty _refreshFrameCountProp;
        private GUIContent _refreshFrameCountContent;


        private void OnEnable()
        {
            _activePointColorProp = serializedObject.FindProperty(nameof(InteractiveComponentHint.activePointColor));
            _activePointColorContent = new GUIContent(s_activePointColor, s_activePointTooltip);
            _inactivePointColorProp =
                serializedObject.FindProperty(nameof(InteractiveComponentHint.inactivePointColor));
            _inactivePointColorContent = new GUIContent(s_inactiveColor, s_inactiveTooltip);
            _originalPointColorProp = serializedObject.FindProperty(nameof(InteractiveComponentHint.originalPointColor));
            _originalPointColorContent = new GUIContent(s_originalPointColor, s_originalPointTooltip);
            _refreshFrameCountProp = serializedObject.FindProperty(nameof(InteractiveComponentHint.framesPerRefresh));
            _refreshFrameCountContent = new GUIContent(s_refreshFrameCount, s_refreshFrameCountTooltip);
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_activePointColorProp, _activePointColorContent);
            EditorGUILayout.PropertyField(_inactivePointColorProp, _inactivePointColorContent);
            EditorGUILayout.PropertyField(_originalPointColorProp, _originalPointColorContent);
            EditorGUILayout.PropertyField(_refreshFrameCountProp, _refreshFrameCountContent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
