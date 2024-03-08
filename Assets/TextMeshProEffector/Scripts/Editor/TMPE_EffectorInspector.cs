using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_Effector))]
    public class TMPE_EffectorInspector : Editor {
        private Editor effectContainerEditor;
        private static GUIStyle _groupBoxSkin;
        private static GUIStyle GroupBoxSkin => _groupBoxSkin ??= new GUIStyle("GroupBox");

        void OnEnable() {
            TMPE_Effector.IsEditing = true;
        }
        void OnDisable() {
            TMPE_Effector.IsEditing = false;
        }
        public override void OnInspectorGUI() {
            serializedObject.Update();

            SerializedProperty effectContainerProp = serializedObject.FindProperty("_effectContainer");

            TMPE_Effector.ForceUpdateInEditing = EditorGUILayout.Toggle("Play In Edit Mode", TMPE_Effector.ForceUpdateInEditing);
            
            EditorGUILayout.LabelField("TypeWriter");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_intervalPerChar"));
            EditorGUI.indentLevel--;

            using var _ = new EditorGUILayout.VerticalScope(GroupBoxSkin);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(effectContainerProp);
            if(EditorGUI.EndChangeCheck()) {
                if(effectContainerProp.objectReferenceValue == null) {
                    effectContainerEditor = null;
                }
            }

            if(effectContainerEditor == null && effectContainerProp.objectReferenceValue != null) {
                effectContainerEditor = Editor.CreateEditor(effectContainerProp.objectReferenceValue);
            }

            if(effectContainerEditor != null) {
                if(effectContainerProp.isExpanded) {
                    if(GUILayout.Button("Hide Inspector")) {
                        effectContainerProp.isExpanded = false;
                    }
                }
                else {
                    if(GUILayout.Button("Open Inspector")) {
                        effectContainerProp.isExpanded = true;
                    }
                }

                if(effectContainerProp.isExpanded) {
                    using var __ = new EditorGUILayout.VerticalScope(GroupBoxSkin);
                    // EditorGUILayout.LabelField("Effect Container");
                    effectContainerEditor.OnInspectorGUI();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}