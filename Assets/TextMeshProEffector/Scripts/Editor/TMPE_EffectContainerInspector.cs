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
        private static GUIStyle _groupBoxSkin;
        private static GUIStyle GroupBoxSkin => _groupBoxSkin ??= new GUIStyle("GroupBox") {margin = new RectOffset()};

        private ReorderableList _basicEffectList;
        private Editor _effectInspector;

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
                    SerializedProperty effectProp = basicEffectsProp.GetArrayElementAtIndex(index);
                    TMPE_EffectBase effect = effectProp.objectReferenceValue as TMPE_EffectBase;
                    Rect fieldRect = new Rect(rect) {width = effect == null ? rect.width : rect.width - 60, height = EditorGUIUtility.singleLineHeight};
                    EditorGUI.PropertyField(fieldRect, effectProp, new GUIContent(effect == null ? "Null" : effect.GetCaption()));

                    if(effect != null) {
                        if(GUI.Button(new Rect(rect) {xMin = rect.xMax - 58, height = EditorGUIUtility.singleLineHeight}, "Open")) {
                            _effectInspector = CreateEditor(effect);
                        }
                    }
                };
                _basicEffectList.elementHeightCallback = index => {
                    return EditorGUI.GetPropertyHeight(basicEffectsProp.GetArrayElementAtIndex(index), true);
                };
            }

            if(_effectInspector == null) {
                _basicEffectList.DoLayoutList();
            }
            else {
                if(GUILayout.Button("Close")) {
                    _effectInspector = null;
                }
                else {
                    using var _ = new BackgroundColorScope(new Color(0.92f, 0.9f, 1f));
                    using (new EditorGUILayout.VerticalScope(GroupBoxSkin, GUILayout.ExpandWidth(true))) {
                        // using(new EditorGUILayout.HorizontalScope()) {
                        //     // EditorGUILayout.LabelField(EditorGUIUtility.IconContent("d_ScriptableObject Icon"), GUILayout.Width(20));
                        //     // EditorGUILayout.LabelField($"{_effectInspector.target.name}({_effectInspector.target.GetType().Name})");

                        // }
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