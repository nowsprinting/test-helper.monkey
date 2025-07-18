// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Annotations;
using UnityEditor;
using UnityEngine;

namespace TestHelper.Monkey.Editor.UI.Annotations
{
    /// <summary>
    /// Editor GUI for WorldPositionAnnotation
    /// </summary>
    [CustomEditor(typeof(WorldPositionAnnotation))]
    public class WorldPositionAnnotationEditor : UnityEditor.Editor
    {
        private static readonly string s_position = L10n.Tr("Position");

        private static readonly string s_positionTooltip =
            L10n.Tr("A world position that where monkey operators operate on");

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(WorldPositionAnnotation.position)),
                new GUIContent(s_position, s_positionTooltip)
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
