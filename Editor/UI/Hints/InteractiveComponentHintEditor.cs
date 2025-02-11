// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Hints;
using UnityEditor;
using UnityEngine;

namespace TestHelper.Monkey.Editor.UI.Hints
{
    /// <summary>
    /// Custom editor for <c cref="InteractiveComponentHint">InteractiveComponentHint</c>.
    /// </summary>
    [CustomEditor(typeof(InteractiveComponentHint))]
    public class InteractiveComponentHintEditor : UnityEditor.Editor
    {
        private static readonly string s_reachablePointColor = L10n.Tr("Corrected Point (reachable)");

        private static readonly string s_reachablePointTooltip =
            L10n.Tr("Color for corrected points of interactive components that monkey operators can operate");

        private SerializedProperty _reachablePointColorProp;
        private GUIContent _reachablePointColorContent;

        private static readonly string s_unreachableColor = L10n.Tr("Corrected Point (unreachable)");

        private static readonly string s_unreachableTooltip =
            L10n.Tr("Color for corrected points of interactive components that monkey operators cannot operate");

        private SerializedProperty _unreachablePointColorProp;
        private GUIContent _unreachablePointColorContent;

        private static readonly string s_originalPointColor = L10n.Tr("Original Point");

        private static readonly string s_originalPointTooltip = L10n.Tr(
            "Color for default operation points for interactive components that is used for the origin point of position annotations"
        );

        private SerializedProperty _originalPointColorProp;
        private GUIContent _originalPointColorContent;

        private static readonly string s_refreshFrameCount = L10n.Tr("Frames/Refresh");
        private static readonly string s_refreshFrameCountTooltip = L10n.Tr("Number of frames per refresh hints");
        private SerializedProperty _refreshFrameCountProp;
        private GUIContent _refreshFrameCountContent;


        private void OnEnable()
        {
            _reachablePointColorProp = serializedObject.FindProperty(nameof(InteractiveComponentHint.reachableColor));
            _reachablePointColorContent = new GUIContent(s_reachablePointColor, s_reachablePointTooltip);
            _unreachablePointColorProp =
                serializedObject.FindProperty(nameof(InteractiveComponentHint.unreachableColor));
            _unreachablePointColorContent = new GUIContent(s_unreachableColor, s_unreachableTooltip);
            _originalPointColorProp = serializedObject.FindProperty(nameof(InteractiveComponentHint.originalPointColor));
            _originalPointColorContent = new GUIContent(s_originalPointColor, s_originalPointTooltip);
            _refreshFrameCountProp = serializedObject.FindProperty(nameof(InteractiveComponentHint.framesPerRefresh));
            _refreshFrameCountContent = new GUIContent(s_refreshFrameCount, s_refreshFrameCountTooltip);
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_reachablePointColorProp, _reachablePointColorContent);
            EditorGUILayout.PropertyField(_unreachablePointColorProp, _unreachablePointColorContent);
            EditorGUILayout.PropertyField(_originalPointColorProp, _originalPointColorContent);
            EditorGUILayout.PropertyField(_refreshFrameCountProp, _refreshFrameCountContent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
