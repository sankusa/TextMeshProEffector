using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Object = UnityEngine.Object;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_EffectorBase), true)]
    public class TMPE_EffectorBaseInspector : Editor {
        private Editor _typeWriterEditor;
        private List<Editor> _typeWriterEditors = new List<Editor>();
        // private Editor _effectContainerEditor;
        private EffectContainerReorderableList<TMPE_BasicEffectContainer> _effectContainerList;
        private static GUIStyle _groupBoxSkin;
        private static GUIStyle GroupBoxSkin => _groupBoxSkin ??= new GUIStyle("GroupBox") {margin = new RectOffset()};

        private ReorderableList _typeWriterList;
        private bool _typeWriterInspectorOpened;
        private int _inspectorShowingTypeWriterSettingIndex;

        void OnEnable() {
            TMPE_EffectorBase.IsEditing = true;
        }
        void OnDisable() {
            TMPE_EffectorBase.IsEditing = false;
        }
        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_EffectorBase effector = target as TMPE_EffectorBase;

            SerializedProperty useTypeWriterProp = serializedObject.FindProperty("_useTypeWriter");
            SerializedProperty defaultVisiblityProp = serializedObject.FindProperty("_defaultVisiblity");
            SerializedProperty effectContainersProp = serializedObject.FindProperty("_effectContainers");
            SerializedProperty typeWritersProp = serializedObject.FindProperty("_typeWriters");
            // SerializedProperty startTypingAutoProp = serializedObject.FindProperty("_startTypingAuto");

            // if(_typeWriterSettingList == null) {
            //     _typeWriterSettingList = new ReorderableList(serializedObject, typeWriterSettingsProp, false, true, true, true);
            //     _typeWriterSettingList.drawHeaderCallback = rect => {
            //         EditorGUI.LabelField(rect, "TypeWriterSettings", EditorStyles.boldLabel);
            //     };
            //     _typeWriterSettingList.drawElementCallback = (rect, index, isActive, isFocused) => {
            //         EditorGUI.PropertyField(rect, typeWriterSettingsProp.GetArrayElementAtIndex(index));
            //     };
            //     _typeWriterSettingList.elementHeightCallback = index => {
            //         return EditorGUI.GetPropertyHeight(typeWriterSettingsProp.GetArrayElementAtIndex(index));
            //     };
            //     _typeWriterSettingList.onAddCallback = list => {
            //         Undo.RecordObject(effector, "Add TypeWriterSetting");
            //         effector.TypeWriterSettings.Add(new TMPE_TypeWriterSetting());
            //     };
            // }
            
            using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
                TMPE_EffectorBase.ForceUpdateInEditing = EditorGUILayout.Toggle("Preview In Edit Mode (Shared)", TMPE_Effector.ForceUpdateInEditing);
            }

            if(effector is TMPE_LeanEffector) {
                SerializedProperty textProp = serializedObject.FindProperty("_text");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(textProp);
                if(EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(effector.TextComponent, "Text Changed");
                    effector.SetText(textProp.stringValue);
                }
            }

            EditorGUILayout.PropertyField(defaultVisiblityProp);

            // _typeWriterSettingList.DoLayoutList();
            // using (new EditorGUILayout.HorizontalScope()) {
            //     EditorGUILayout.LabelField("TypeWriterSettings", EditorStyles.boldLabel);
            //     if(GUILayout.Button("Add")) {
            //         Undo.RecordObject(effector, "Add TypeWriterSetting");
            //         effector.TypeWriters.Add(new TMPE_TypeWriterSetting());
            //     }
            // }

            // for(int i = 0; i < typeWriterSettingsProp.arraySize; i++) {
            //     if(i == _typeWriterEditors.Count) _typeWriterEditors.Add(null);

            //     using (new EditorGUILayout.HorizontalScope()) {
            //         // EditorGUILayout.Space(20, false);
            //         using var _ = new BackgroundColorScope(new Color(0.8f, 0.8f, 0.8f, 1f));
            //         using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
            //             using var __ = new BackgroundColorScope(Color.white);

            //             SerializedProperty typeWriterSettingProp = typeWriterSettingsProp.GetArrayElementAtIndex(i);
            //             SerializedProperty typeWriterProp = typeWriterSettingProp.FindPropertyRelative("_typeWriter");
            //             var typeWriter = typeWriterProp.objectReferenceValue;

                        
            //             if(_typeWriterEditors[i] != null) {
            //                 if(GUILayout.Button("Close")) {
            //                     _typeWriterEditors[i] = null;
            //                     continue;
            //                 }

            //                 EditorGUI.BeginDisabledGroup(true);
            //                 EditorGUILayout.ObjectField(typeWriterProp, GUIContent.none);
            //                 EditorGUI.EndDisabledGroup();

            //                 using var ___ = new BackgroundColorScope(new Color(0.9f, 1f, 1f));
            //                 using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
            //                     _typeWriterEditors[i].OnInspectorGUI();
            //                 }
            //             }
            //             else {
            //                 using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
            //                     using (new LabelWidthScope(40)) {
            //                         EditorGUILayout.LabelField(i.ToString());
            //                     }
                                
            //                     if(typeWriterSettingProp.isExpanded) {
            //                         if(GUILayout.Button("Hide")) {
            //                             typeWriterSettingProp.isExpanded = false;
            //                             continue;
            //                         }
            //                     }
            //                     else {
            //                         if(GUILayout.Button("Show")) {
            //                             typeWriterSettingProp.isExpanded = true;
            //                             continue;
            //                         }
            //                     }

            //                     if(GUILayout.Button("Remove")) {
            //                         typeWriterSettingsProp.DeleteArrayElementAtIndex(i);
            //                         continue;
            //                     }
            //                 }

            //                 if(typeWriterSettingProp.isExpanded) {
            //                     if(typeWriterProp.objectReferenceValue != null) {
            //                         if(GUILayout.Button("Open TypeWriter Inspector")) {
            //                             _typeWriterEditors[i] = CreateEditor(typeWriterProp.objectReferenceValue);
            //                             continue;
            //                         }
            //                     }
            //                     EditorGUILayout.PropertyField(typeWriterSettingProp);
            //                 }
            //             }
            //         }
                    
            //     }
            // }

            // EditorGUILayout.Space(4);

            // エフェクトコンテナ
            using (new EditorGUILayout.HorizontalScope()) {
                if(_effectContainerList == null) {
                    _effectContainerList = new EffectContainerReorderableList<TMPE_BasicEffectContainer>(effectContainersProp, "Basic Effect Containers");
                }
                _effectContainerList.DoLayoutList();
            }

            // if(_effectContainerEditor != null && effectContainerProp.isExpanded) {
            //     using (new EditorGUILayout.HorizontalScope()) {
            //         using var _ = new BackgroundColorScope(new Color(0.85f, 1f, 1f));
            //         using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
            //             if(effectContainerProp.isExpanded) {
            //                 _effectContainerEditor.OnInspectorGUI();
            //             }
            //         }
            //     }
            // }

            EditorGUILayout.LabelField("TypeWriter");
            using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
                if(_typeWriterList == null) {
                    _typeWriterList = new ReorderableList(serializedObject, typeWritersProp) {
                        drawHeaderCallback = (rect) => {
                            EditorGUI.LabelField(rect, "TypeWriters");
                        },
                        drawElementCallback = (rect, index, isActive, isFocused) => {
                            EditorGUI.PropertyField(
                                new Rect(rect) {height = EditorGUIUtility.singleLineHeight, y = rect.y + 2},
                                typeWritersProp.GetArrayElementAtIndex(index), new GUIContent(index.ToString())
                            );
                        }
                    };
                }
                _typeWriterList.DoLayoutList();

                SerializedProperty onTypingCompletedProp = serializedObject.FindProperty("_onTypingCompleted");
                EditorGUILayout.PropertyField(onTypingCompletedProp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}