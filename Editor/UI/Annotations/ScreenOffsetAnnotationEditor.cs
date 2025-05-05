// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Annotations;
using UnityEditor;
using UnityEngine;

namespace TestHelper.Monkey.Editor.UI.Annotations
{
    /// <summary>
    /// Editor GUI for ScreenOffsetAnnotation
    /// </summary>
    [CustomEditor(typeof(ScreenOffsetAnnotation))]
    public class ScreenOffsetAnnotationEditor : UnityEditor.Editor
    {
        private static readonly string s_offset = L10n.Tr("Offset");

        private static readonly string s_offsetTooltip =
            L10n.Tr("Offset from a screen point of the GameObject that the annotation attached to");

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(ScreenOffsetAnnotation.offset)),
                new GUIContent(s_offset, s_offsetTooltip)
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
