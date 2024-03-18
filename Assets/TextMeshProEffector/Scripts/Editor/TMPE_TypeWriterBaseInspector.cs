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
        private static GUIStyle _groupBoxSkin;
        private static GUIStyle GroupBoxSkin => _groupBoxSkin ??= new GUIStyle("GroupBox") {margin = new RectOffset()};

        private ReorderableList _typingEffectList;
        private ReorderableList _typingEventEffectList;

        private Editor _effectInspector;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_TypeWriterBase typeWriter = target as TMPE_TypeWriterBase;

            // TypingEffects
            SerializedProperty typingEffectsProp = serializedObject.FindProperty("_typingEffects");
            if(_typingEffectList == null) {
                _typingEffectList = new ReorderableList(serializedObject, typingEffectsProp);
                _typingEffectList.drawHeaderCallback = rect => {
                    EditorGUI.LabelField(rect, "Typing Effects");
                };
                _typingEffectList.drawElementCallback = (rect, index, isActive, isFocused) => {
                    SerializedProperty effectProp = typingEffectsProp.GetArrayElementAtIndex(index);
                    TMPE_EffectBase effect = effectProp.objectReferenceValue as TMPE_EffectBase;
                    Rect fieldRect = new Rect(rect) {width = effect == null ? rect.width : rect.width - 60, height = EditorGUIUtility.singleLineHeight};
                    EditorGUI.PropertyField(fieldRect, effectProp, new GUIContent(effect == null ? "Null" : effect.GetCaption()));

                    if(effect != null) {
                        if(GUI.Button(new Rect(rect) {xMin = rect.xMax - 58, height = EditorGUIUtility.singleLineHeight}, "Open")) {
                            _effectInspector = CreateEditor(effect);
                        }
                    }
                };
                _typingEffectList.elementHeightCallback = index => {
                    return EditorGUI.GetPropertyHeight(typingEffectsProp.GetArrayElementAtIndex(index), true);
                };
            }

            // TypingEventEffects
            SerializedProperty typingEventEffectsProp = serializedObject.FindProperty("_typingEventEffects");
            if(_typingEventEffectList == null) {
                _typingEventEffectList = new ReorderableList(serializedObject, typingEventEffectsProp);
                _typingEventEffectList.drawHeaderCallback = rect => {
                    EditorGUI.LabelField(rect, "Typing Event Effects");
                };
                _typingEventEffectList.drawElementCallback = (rect, index, isActive, isFocused) => {
                    SerializedProperty effectProp = typingEventEffectsProp.GetArrayElementAtIndex(index);
                    TMPE_EffectBase effect = effectProp.objectReferenceValue as TMPE_EffectBase;
                    Rect fieldRect = new Rect(rect) {width = effect == null ? rect.width : rect.width - 60, height = EditorGUIUtility.singleLineHeight};
                    EditorGUI.PropertyField(fieldRect, effectProp, new GUIContent(effect == null ? "Null" : effect.GetCaption()));

                    if(effect != null) {
                        if(GUI.Button(new Rect(rect) {xMin = rect.xMax - 58, height = EditorGUIUtility.singleLineHeight}, "Open")) {
                            _effectInspector = CreateEditor(effect);
                        }
                    }
                };
                _typingEventEffectList.elementHeightCallback = index => {
                    return EditorGUI.GetPropertyHeight(typingEventEffectsProp.GetArrayElementAtIndex(index), true);
                };
            }

            if(_effectInspector == null) {
                SerializedProperty iterator = serializedObject.GetIterator();
                iterator.NextVisible(true);
                while(iterator.NextVisible(false)) {
                    if(iterator.name == "_typingEffects" || iterator.name == "_typingEventEffects") continue;
                    EditorGUILayout.PropertyField(iterator, true);
                }

                _typingEffectList.DoLayoutList();
                _typingEventEffectList.DoLayoutList();
            }
            else {
                if(GUILayout.Button("Close")) {
                    _effectInspector = null;
                }
                else {
                    using var _ = new BackgroundColorScope(new Color(0.92f, 0.9f, 1f));
                    using (new EditorGUILayout.VerticalScope(GroupBoxSkin, GUILayout.ExpandWidth(true))) {
                        TMPE_EffectBase effect = _effectInspector.target as TMPE_EffectBase;
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(effect, typeof(TMPE_EffectBase), false);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.LabelField(effect.GetCaption());
                        
                        using (new EditorGUILayout.VerticalScope(GroupBoxSkin, GUILayout.ExpandWidth(true))) {
                            _effectInspector.OnInspectorGUI();
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}