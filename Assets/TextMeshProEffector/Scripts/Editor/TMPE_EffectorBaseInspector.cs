using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_EffectorBase), true)]
    public class TMPE_EffectorBaseInspector : Editor {
        private Editor _typeWriterEditor;
        private List<Editor> _typeWriterEditors = new List<Editor>();
        private Editor _effectContainerEditor;
        private static GUIStyle _groupBoxSkin;
        private static GUIStyle GroupBoxSkin => _groupBoxSkin ??= new GUIStyle("GroupBox") {margin = new RectOffset()};

        private ReorderableList _typeWriterSettingList;
        private bool _typeWriterInspectorOpened;
        private int _inspectorShowingTypeWriterSettingIndex;
        private TMPE_TypeWriterBase _editingTypeWriter;

        void OnEnable() {
            TMPE_EffectorBase.IsEditing = true;
            
        }
        void OnDisable() {
            TMPE_EffectorBase.IsEditing = false;
        }
        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_EffectorBase effector = target as TMPE_EffectorBase;

            SerializedProperty defaultCharacterVisiblityProp = serializedObject.FindProperty("_defaultCharacterVisiblity");
            SerializedProperty effectContainerProp = serializedObject.FindProperty("_effectContainer");
            SerializedProperty typeWriterSettingsProp = serializedObject.FindProperty("_typeWriterSettings");
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

            EditorGUILayout.PropertyField(defaultCharacterVisiblityProp);
            
            // _typeWriterSettingList.DoLayoutList();
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField("TypeWriterSettings", EditorStyles.boldLabel);
                if(GUILayout.Button("Add")) {
                    Undo.RecordObject(effector, "Add TypeWriterSetting");
                    effector.TypeWriterSettings.Add(new TMPE_TypeWriterSetting());
                }
            }

            for(int i = 0; i < typeWriterSettingsProp.arraySize; i++) {
                if(i == _typeWriterEditors.Count) _typeWriterEditors.Add(null);

                using (new EditorGUILayout.HorizontalScope()) {
                    // EditorGUILayout.Space(20, false);
                    using var _ = new BackgroundColorScope(new Color(0.8f, 0.8f, 0.8f, 1f));
                    using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
                        using var __ = new BackgroundColorScope(Color.white);

                        SerializedProperty typeWriterSettingProp = typeWriterSettingsProp.GetArrayElementAtIndex(i);
                        SerializedProperty typeWriterProp = typeWriterSettingProp.FindPropertyRelative("_typeWriter");
                        var typeWriter = typeWriterProp.objectReferenceValue;

                        
                        if(_typeWriterEditors[i] != null) {
                            if(GUILayout.Button("Close")) {
                                _typeWriterEditors[i] = null;
                                continue;
                            }

                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(typeWriterProp, GUIContent.none);
                            EditorGUI.EndDisabledGroup();

                            using var ___ = new BackgroundColorScope(new Color(0.9f, 1f, 1f));
                            using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
                                _typeWriterEditors[i].OnInspectorGUI();
                            }
                        }
                        else {
                            using (new EditorGUILayout.HorizontalScope(GUI.skin.box)) {
                                using (new LabelWidthScope(40)) {
                                    EditorGUILayout.LabelField(i.ToString());
                                }
                                
                                if(typeWriterSettingProp.isExpanded) {
                                    if(GUILayout.Button("Hide")) {
                                        typeWriterSettingProp.isExpanded = false;
                                        continue;
                                    }
                                }
                                else {
                                    if(GUILayout.Button("Show")) {
                                        typeWriterSettingProp.isExpanded = true;
                                        continue;
                                    }
                                }

                                if(GUILayout.Button("Remove")) {
                                    typeWriterSettingsProp.DeleteArrayElementAtIndex(i);
                                    continue;
                                }
                            }

                            if(typeWriterSettingProp.isExpanded) {
                                if(typeWriterProp.objectReferenceValue != null) {
                                    if(GUILayout.Button("Open TypeWriter Inspector")) {
                                        _typeWriterEditors[i] = CreateEditor(typeWriterProp.objectReferenceValue);
                                        continue;
                                    }
                                }
                                EditorGUILayout.PropertyField(typeWriterSettingProp);
                            }
                        }
                    }
                    
                }
            }

            EditorGUILayout.Space(4);

            // エフェクトコンテナ
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(effectContainerProp);
            if(EditorGUI.EndChangeCheck()) {
                _effectContainerEditor = null;
            }
            if(_effectContainerEditor == null && effectContainerProp.objectReferenceValue != null) {
                _effectContainerEditor = CreateEditor(effectContainerProp.objectReferenceValue);
            }
            if(_effectContainerEditor != null) {
                using (new EditorGUILayout.HorizontalScope()) {
                    using var _ = new BackgroundColorScope(new Color(0.8f, 0.8f, 0.8f, 1f));
                    using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
                        using var __ = new BackgroundColorScope(Color.white);
                        if(effectContainerProp.isExpanded) {
                            if(GUILayout.Button("Hide EffectContainer Inspector")) {
                                effectContainerProp.isExpanded = false;
                            }
                        }
                        else {
                            if(GUILayout.Button("Open EffectContainer Inspector")) {
                                effectContainerProp.isExpanded = true;
                            }
                        }

                        if(effectContainerProp.isExpanded) {
                            _effectContainerEditor.OnInspectorGUI();
                        }
                    }
                }
            }

            EditorGUILayout.Space(4);

            SerializedProperty onTypingCompletedProp = serializedObject.FindProperty("_onTypingCompleted");
            EditorGUILayout.PropertyField(onTypingCompletedProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}