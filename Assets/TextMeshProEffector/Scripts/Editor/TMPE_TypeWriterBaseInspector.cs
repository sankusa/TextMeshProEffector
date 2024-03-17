using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_TypeWriterBase), true)]
    public class TMPE_TypeWriterBaseInspector : Editor {
        private ReorderableList _typingEffectList;
        private ReorderableList _typingEventEffectList;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_TypeWriterBase typeWriter = target as TMPE_TypeWriterBase;

            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            while(iterator.NextVisible(false)) {
                if(iterator.name == "_typingEffects" || iterator.name == "_typingEventEffects") continue;
                EditorGUILayout.PropertyField(iterator, true);
            }

            // TypingEffects
            SerializedProperty typingEffectsProp = serializedObject.FindProperty("_typingEffects");
            if(_typingEffectList == null) {
                _typingEffectList = new ReorderableList(serializedObject, typingEffectsProp);
                _typingEffectList.drawHeaderCallback = rect => {
                    EditorGUI.LabelField(rect, "Typing Effects");
                };
                _typingEffectList.drawElementCallback = (rect, index, isActive, isFocused) => {
                    EditorGUI.PropertyField(rect, typingEffectsProp.GetArrayElementAtIndex(index), true);
                };
                _typingEffectList.elementHeightCallback = index => {
                    return EditorGUI.GetPropertyHeight(typingEffectsProp.GetArrayElementAtIndex(index), true);
                };
                _typingEffectList.onAddDropdownCallback = (rect, list) => {
                    GenericMenu addMenu = new GenericMenu();
                    foreach(Type effectType in TypeCache.GetTypesDerivedFrom<TMPE_TypingEffect>().Where(x => x.IsAbstract == false)) {
                        addMenu.AddItem(new GUIContent(effectType.Name), false, () => {
                            Undo.RecordObject(typeWriter, "Add Effect");
                            TMPE_TypingEffect effect = Activator.CreateInstance(effectType) as TMPE_TypingEffect;
                            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(effect), effect);
                            typeWriter.TypingEffects.Add(effect);
                        });
                    }
                    addMenu.ShowAsContext();
                };
            }
            _typingEffectList.DoLayoutList();

            // TypingEventEffects
            SerializedProperty typingEventEffectsProp = serializedObject.FindProperty("_typingEventEffects");
            if(_typingEventEffectList == null) {
                _typingEventEffectList = new ReorderableList(serializedObject, typingEventEffectsProp);
                _typingEventEffectList.drawHeaderCallback = rect => {
                    EditorGUI.LabelField(rect, "Typing Event Effects");
                };
                _typingEventEffectList.drawElementCallback = (rect, index, isActive, isFocused) => {
                    EditorGUI.PropertyField(rect, typingEventEffectsProp.GetArrayElementAtIndex(index), true);
                };
                _typingEventEffectList.elementHeightCallback = index => {
                    return EditorGUI.GetPropertyHeight(typingEventEffectsProp.GetArrayElementAtIndex(index), true);
                };
                _typingEventEffectList.onAddDropdownCallback = (rect, list) => {
                    GenericMenu addMenu = new GenericMenu();
                    foreach(Type effectType in TypeCache.GetTypesDerivedFrom<TMPE_TypingEventEffect>().Where(x => x.IsAbstract == false)) {
                        addMenu.AddItem(new GUIContent(effectType.Name), false, () => {
                            Undo.RecordObject(typeWriter, "Add Effect");
                            TMPE_TypingEventEffect effect = Activator.CreateInstance(effectType) as TMPE_TypingEventEffect;
                            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(effect), effect);
                            typeWriter.TypingEventEffects.Add(effect);
                        });
                    }
                    addMenu.ShowAsContext();
                };
            }
            _typingEventEffectList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}