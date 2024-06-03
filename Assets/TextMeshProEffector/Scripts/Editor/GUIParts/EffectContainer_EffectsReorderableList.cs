using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TextMeshProEffector {
    public class EffectContainer_EffectsReorderableList {
        private readonly TMPE_EffectContainerBase _effectContainer;
        private readonly ReorderableList _reorderableList;
        private readonly Type _effectBaseType;
        private readonly IEnumerable<Type> _availableEffectTypes;

        public EffectContainer_EffectsReorderableList(SerializedObject serializedEffectContainer, TMPE_EffectContainerBase effectContainer, string label) {
            SerializedProperty effectsProp = serializedEffectContainer.FindProperty("_effects");

            _effectContainer = effectContainer;
            _effectBaseType = effectContainer.GetType().BaseType.GetGenericArguments()[0];
            _availableEffectTypes = TypeCache.GetTypesDerivedFrom(_effectBaseType).Where(x => x.IsAbstract == false);
            
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
                foreach(Type effectType in _availableEffectTypes) {
                    menu.AddItem(new GUIContent(effectType.Name), false, () => {
                        Undo.RecordObject(effectsProp.serializedObject.targetObject, "Add Effect");
                        _effectContainer.AddEffect(Activator.CreateInstance(effectType));
                    });
                }
                menu.ShowAsContext();
            };
        }

        public void DoLayoutList() => _reorderableList.DoLayoutList();
    }
}