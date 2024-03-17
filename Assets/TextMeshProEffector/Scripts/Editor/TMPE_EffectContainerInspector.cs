using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_EffectContainer))]
    public class TMPE_EffectContainerInspector : Editor {
        private ReorderableList _basicEffectList;
        // private ReorderableList _typingEffectList;
        // private ReorderableList _typingEventEffectList;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_EffectContainer effectContainer = target as TMPE_EffectContainer;

            // BasicEffects
            SerializedProperty basicEffectsProp = serializedObject.FindProperty("_basicEffects");
            if(_basicEffectList == null) {
                _basicEffectList = new ReorderableList(serializedObject, basicEffectsProp);
                _basicEffectList.drawHeaderCallback = rect => {
                    EditorGUI.LabelField(rect, "Basic Effects");
                };
                _basicEffectList.drawElementCallback = (rect, index, isActive, isFocused) => {
                    EditorGUI.PropertyField(rect, basicEffectsProp.GetArrayElementAtIndex(index), true);
                };
                _basicEffectList.elementHeightCallback = index => {
                    return EditorGUI.GetPropertyHeight(basicEffectsProp.GetArrayElementAtIndex(index), true);
                };
                _basicEffectList.onAddDropdownCallback = (rect, list) => {
                    GenericMenu addMenu = new GenericMenu();
                    foreach(Type effectType in TypeCache.GetTypesDerivedFrom<TMPE_BasicEffect>().Where(x => x.IsAbstract == false)) {
                        addMenu.AddItem(new GUIContent(effectType.Name), false, () => {
                            Undo.RecordObject(effectContainer, "Add Effect");
                            TMPE_BasicEffect effect = Activator.CreateInstance(effectType) as TMPE_BasicEffect;
                            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(effect), effect);
                            effectContainer.BasicEffects.Add(effect);
                        });
                    }
                    addMenu.ShowAsContext();
                };
            }
            _basicEffectList.DoLayoutList();

            // // TypingEffects
            // SerializedProperty typingEffectsProp = serializedObject.FindProperty("_typingEffects");
            // if(_typingEffectList == null) {
            //     _typingEffectList = new ReorderableList(serializedObject, typingEffectsProp);
            //     _typingEffectList.drawHeaderCallback = rect => {
            //         EditorGUI.LabelField(rect, "Typing Effects");
            //     };
            //     _typingEffectList.drawElementCallback = (rect, index, isActive, isFocused) => {
            //         EditorGUI.PropertyField(rect, typingEffectsProp.GetArrayElementAtIndex(index), true);
            //     };
            //     _typingEffectList.elementHeightCallback = index => {
            //         return EditorGUI.GetPropertyHeight(typingEffectsProp.GetArrayElementAtIndex(index), true);
            //     };
            //     _typingEffectList.onAddDropdownCallback = (rect, list) => {
            //         GenericMenu addMenu = new GenericMenu();
            //         foreach(Type effectType in TypeCache.GetTypesDerivedFrom<TMPE_TypingEffect>().Where(x => x.IsAbstract == false)) {
            //             addMenu.AddItem(new GUIContent(effectType.Name), false, () => {
            //                 Undo.RecordObject(effectContainer, "Add Effect");
            //                 TMPE_TypingEffect effect = Activator.CreateInstance(effectType) as TMPE_TypingEffect;
            //                 JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(effect), effect);
            //                 effectContainer.TypingEffects.Add(effect);
            //             });
            //         }
            //         addMenu.ShowAsContext();
            //     };
            // }
            // _typingEffectList.DoLayoutList();

            // // TypingEventEffects
            // SerializedProperty typingEventEffectsProp = serializedObject.FindProperty("_typingEventEffects");
            // if(_typingEventEffectList == null) {
            //     _typingEventEffectList = new ReorderableList(serializedObject, typingEventEffectsProp);
            //     _typingEventEffectList.drawHeaderCallback = rect => {
            //         EditorGUI.LabelField(rect, "Typing Event Effects");
            //     };
            //     _typingEventEffectList.drawElementCallback = (rect, index, isActive, isFocused) => {
            //         EditorGUI.PropertyField(rect, typingEventEffectsProp.GetArrayElementAtIndex(index), true);
            //     };
            //     _typingEventEffectList.elementHeightCallback = index => {
            //         return EditorGUI.GetPropertyHeight(typingEventEffectsProp.GetArrayElementAtIndex(index), true);
            //     };
            //     _typingEventEffectList.onAddDropdownCallback = (rect, list) => {
            //         GenericMenu addMenu = new GenericMenu();
            //         foreach(Type effectType in TypeCache.GetTypesDerivedFrom<TMPE_TypingEventEffect>().Where(x => x.IsAbstract == false)) {
            //             addMenu.AddItem(new GUIContent(effectType.Name), false, () => {
            //                 Undo.RecordObject(effectContainer, "Add Effect");
            //                 TMPE_TypingEventEffect effect = Activator.CreateInstance(effectType) as TMPE_TypingEventEffect;
            //                 JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(effect), effect);
            //                 effectContainer.TypingEventEffects.Add(effect);
            //             });
            //         }
            //         addMenu.ShowAsContext();
            //     };
            // }
            // _typingEventEffectList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}