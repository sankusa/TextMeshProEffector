using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TextMeshProEffector {
    public class EffectReorderableList<T> where T : TMPE_EffectBase {
        private readonly ReorderableList _reorderableList;

        public EffectReorderableList(SerializedProperty effectsProp, List<T> effects, string label) {
            _reorderableList = new ReorderableList(effectsProp.serializedObject, effectsProp);
            _reorderableList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, label);
            };
            _reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => {
                SerializedProperty effectProp = effectsProp.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, effectProp);
            };
            _reorderableList.elementHeightCallback = index => {
                return EditorGUI.GetPropertyHeight(effectsProp.GetArrayElementAtIndex(index), true);
            };
            _reorderableList.onAddDropdownCallback = (buttonRect, list) => {
                GenericMenu menu = new GenericMenu();
                foreach(Type type in TypeCache.GetTypesDerivedFrom<T>().Where(x => x.IsAbstract == false)) {
                    menu.AddItem(new GUIContent(type.Name), false, () => {
                        Undo.RecordObject(effectsProp.serializedObject.targetObject, "Add Effect");
                        effects.Add((T)Activator.CreateInstance(type));
                    });
                }
                menu.ShowAsContext();
            };
        }

        public void DoLayoutList() => _reorderableList.DoLayoutList();
    }
}