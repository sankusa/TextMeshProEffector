using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TextMeshProEffector {
    public class EffectContainerReorderableList<T> where T : ScriptableObject {
        private readonly ReorderableList _reorderableList;

        public EffectContainerReorderableList(SerializedProperty effectContainersProp, string label) {
            _reorderableList = new ReorderableList(effectContainersProp.serializedObject, effectContainersProp);
            _reorderableList.drawHeaderCallback = (rect) => {
                EditorGUI.LabelField(rect, label);
            };
            _reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => {
                SerializedProperty property =  effectContainersProp.GetArrayElementAtIndex(index);

                Rect line1Rect = new Rect(rect) {y = rect.y + 2, height = EditorGUIUtility.singleLineHeight};

                float insepectorButtonWidth = 70;
                
                if(property.objectReferenceValue == null) {
                    EditorGUI.PropertyField(line1Rect, property, GUIContent.none);
                }
                else {
                    Rect propertyFieldRect = new Rect(line1Rect) {width = rect.width - insepectorButtonWidth - EditorGUIUtility.standardVerticalSpacing};
                    EditorGUI.PropertyField(propertyFieldRect, property, GUIContent.none);

                    Rect inspectorButtonRect = new Rect(line1Rect) {xMin = rect.xMax - insepectorButtonWidth};
                    if(GUI.Button(inspectorButtonRect, "Inspector")) {
                        InspectorUtil.OpenAnotherInspector(property.objectReferenceValue);
                    }
                }
            };
        }

        public void DoLayoutList() => _reorderableList.DoLayoutList();
    }
}