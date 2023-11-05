// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Monkey.Annotations;
using UnityEditor;
using UnityEngine;

namespace TestHelper.Monkey.Editor.UI.Annotations
{
    [CustomEditor(typeof(InputFieldAnnotation))]
    public class InputFieldAnnotationEditor : UnityEditor.Editor
    {
        private static readonly string s_charactersKind = L10n.Tr("Character Kind");

        private static readonly string s_charactersKindTooltip =
            L10n.Tr("Character kind of random input text by Monkey");

        private static readonly string s_minimumLength = L10n.Tr("Minimum Length");

        private static readonly string s_minimumLengthTooltip =
            L10n.Tr("Minimum length of random input text by Monkey");

        private static readonly string s_maximumLength = L10n.Tr("Maximum Length");

        private static readonly string s_maximumLengthTooltip =
            L10n.Tr("Maximum length of random input text by Monkey");

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(
                    nameof(InputFieldAnnotation.charactersKind)),
                new GUIContent(s_charactersKind, s_charactersKindTooltip)
            );

            EditorGUILayout.PropertyField(serializedObject.FindProperty(
                    nameof(InputFieldAnnotation.minimumLength)),
                new GUIContent(s_minimumLength, s_minimumLengthTooltip)
            );
            EditorGUILayout.PropertyField(serializedObject.FindProperty(
                    nameof(InputFieldAnnotation.maximumLength)),
                new GUIContent(s_maximumLength, s_maximumLengthTooltip)
            );
            // Note: MinMaxSlider can only handle floats.

            serializedObject.ApplyModifiedProperties();
        }
    }
}
