// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor;
// using UnityEditorInternal;
// using UnityEngine;
// using UnityEngine.SocialPlatforms;

// namespace TextMeshProEffector {
//     [CustomEditor(typeof(TMPE_Effector))]
//     public class TMPE_EffectorInspector : Editor {
//         private Editor _typeWriterEditor;
//         private List<Editor> _typeWriterEditors = new List<Editor>();
//         private Editor _effectContainerEditor;
//         private static GUIStyle _groupBoxSkin;
//         private static GUIStyle GroupBoxSkin => _groupBoxSkin ??= new GUIStyle("GroupBox") {margin = new RectOffset()};

//         private ReorderableList _typeWriterSettingList;
//         private bool _typeWriterInspectorOpened;
//         private int _inspectorShowingTypeWriterSettingIndex;
//         private TMPE_TypeWriterBase _editingTypeWriter;

//         void OnEnable() {
//             TMPE_Effector.IsEditing = true;
            
//         }
//         void OnDisable() {
//             TMPE_Effector.IsEditing = false;
//         }
//         public override void OnInspectorGUI() {
//             serializedObject.Update();

//             TMPE_Effector effector = target as TMPE_Effector;

//             SerializedProperty defaultCharacterVisiblityProp = serializedObject.FindProperty("_defaultCharacterVisiblity");
//             SerializedProperty effectContainerProp = serializedObject.FindProperty("_effectContainer");
//             SerializedProperty typeWriterSettingsProp = serializedObject.FindProperty("_typeWriterSettings");
//             // SerializedProperty startTypingAutoProp = serializedObject.FindProperty("_startTypingAuto");

//             // if(_typeWriterSettingList == null) {
//             //     _typeWriterSettingList = new ReorderableList(serializedObject, typeWriterSettingsProp, false, true, true, true);
//             //     _typeWriterSettingList.drawHeaderCallback = rect => {
//             //         EditorGUI.LabelField(rect, "TypeWriterSettings", EditorStyles.boldLabel);
//             //     };
//             //     _typeWriterSettingList.drawElementCallback = (rect, index, isActive, isFocused) => {
//             //         EditorGUI.PropertyField(rect, typeWriterSettingsProp.GetArrayElementAtIndex(index));
//             //     };
//             //     _typeWriterSettingList.elementHeightCallback = index => {
//             //         return EditorGUI.GetPropertyHeight(typeWriterSettingsProp.GetArrayElementAtIndex(index));
//             //     };
//             //     _typeWriterSettingList.onAddCallback = list => {
//             //         Undo.RecordObject(effector, "Add TypeWriterSetting");
//             //         effector.TypeWriterSettings.Add(new TMPE_TypeWriterSetting());
//             //     };
//             // }
            
//             using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
//                 TMPE_Effector.ForceUpdateInEditing = EditorGUILayout.Toggle("Preview In Edit Mode (Shared)", TMPE_Effector.ForceUpdateInEditing);
//             }

//             EditorGUILayout.PropertyField(defaultCharacterVisiblityProp);

//             EditorGUILayout.Space(12);
            
//             // _typeWriterSettingList.DoLayoutList();
//             using (new EditorGUILayout.HorizontalScope()) {
//                 EditorGUILayout.LabelField("TypeWriterSettings", EditorStyles.boldLabel);
//                 if(GUILayout.Button("Add")) {
//                     Undo.RecordObject(effector, "Add TypeWriterSetting");
//                     effector.TypeWriterSettings.Add(new TMPE_TypeWriterSetting());
//                 }
//             }
//             //using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
//                 for(int i = 0; i < typeWriterSettingsProp.arraySize; i++) {
//                     using (new EditorGUILayout.HorizontalScope()) {
//                         EditorGUILayout.Space(20, false);
//                         using var _ = new BackgroundColorScope(new Color(0.8f, 0.8f, 0.8f, 1f));
//                         using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
//                             using var __ = new BackgroundColorScope(Color.white);

//                             SerializedProperty typeWriterSettingProp = typeWriterSettingsProp.GetArrayElementAtIndex(i);
//                             SerializedProperty typeWriterProp = typeWriterSettingProp.FindPropertyRelative("_typeWriter");
//                             var typeWriter = typeWriterProp.objectReferenceValue;
//                             using (new EditorGUILayout.HorizontalScope()) {
//                                 EditorGUILayout.LabelField(i.ToString());
//                                 if(GUILayout.Button("Remove")) {
//                                     typeWriterSettingsProp.DeleteArrayElementAtIndex(i);
//                                     continue;
//                                 }
//                             }
//                             EditorGUILayout.PropertyField(typeWriterSettingProp);
//                             if(typeWriterProp.objectReferenceValue != typeWriter) {
//                                 _typeWriterEditors[i] = null;
//                             }
//                             if(i == _typeWriterEditors.Count) {
//                                 _typeWriterEditors.Add(null);
//                             }
//                             if(typeWriterProp.objectReferenceValue != null) {
//                                 if(_typeWriterEditors[i] == null) _typeWriterEditors[i] = CreateEditor(typeWriterProp.objectReferenceValue);
//                                 if(typeWriterProp.isExpanded) {
//                                     if(GUILayout.Button("Hide TypeWriter Inspector")) {
//                                         typeWriterProp.isExpanded = false;
//                                     }
//                                 }
//                                 else {
//                                     if(GUILayout.Button("Open TypeWriter Inspector")) {
//                                         typeWriterProp.isExpanded = true;
//                                     }
//                                 }

//                                 if(typeWriterProp.isExpanded) {
//                                     using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
//                                         _typeWriterEditors[i].OnInspectorGUI();
//                                     }
//                                 }
//                             }
//                         }
//                     }
//                 }
//             //}

//             EditorGUILayout.Space(12);

//             // エフェクトコンテナ
//             EditorGUI.BeginChangeCheck();
//             EditorGUILayout.PropertyField(effectContainerProp);
//             if(EditorGUI.EndChangeCheck()) {
//                 _effectContainerEditor = null;
//             }
//             if(_effectContainerEditor == null && effectContainerProp.objectReferenceValue != null) {
//                 _effectContainerEditor = CreateEditor(effectContainerProp.objectReferenceValue);
//             }
//             if(_effectContainerEditor != null) {
//                 using (new EditorGUILayout.HorizontalScope()) {
//                     EditorGUILayout.Space(20, false);
//                     using var _ = new BackgroundColorScope(new Color(0.8f, 0.8f, 0.8f, 1f));
//                     using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
//                         using var __ = new BackgroundColorScope(Color.white);
//                         if(effectContainerProp.isExpanded) {
//                             if(GUILayout.Button("Hide EffectContainer Inspector")) {
//                                 effectContainerProp.isExpanded = false;
//                             }
//                         }
//                         else {
//                             if(GUILayout.Button("Open EffectContainer Inspector")) {
//                                 effectContainerProp.isExpanded = true;
//                             }
//                         }

//                         if(effectContainerProp.isExpanded) {
//                             _effectContainerEditor.OnInspectorGUI();
//                         }
//                     }
//                 }
//             }

//             SerializedProperty onTypingCompletedProp = serializedObject.FindProperty("_onTypingCompleted");
//             EditorGUILayout.PropertyField(onTypingCompletedProp);

//             // タイプライターのインスペクタを表示
//             // using (new EditorGUILayout.VerticalScope(GroupBoxSkin)) {
//             //     if(typeWriterSettingsProp.arraySize > 0) {
//             //         if(_typeWriterInspectorOpened) {
//             //             if(GUILayout.Button("Hide TypeWriter Inspector")) {
//             //                 _typeWriterInspectorOpened = false;
//             //             }
//             //         }
//             //         else {
//             //             if(GUILayout.Button("Open TypeWriter Inspector")) {
//             //                 _typeWriterInspectorOpened = true;
//             //             }
//             //         }

//             //         if(_typeWriterInspectorOpened) {
//             //             int[] typeWriterSettingIndexSelections = Enumerable.Range(0, typeWriterSettingsProp.arraySize).ToArray();

//             //             if(_inspectorShowingTypeWriterSettingIndex >= typeWriterSettingsProp.arraySize) _inspectorShowingTypeWriterSettingIndex = -1;

//             //             _inspectorShowingTypeWriterSettingIndex = EditorGUILayout.IntPopup(
//             //                 "Index",
//             //                 _inspectorShowingTypeWriterSettingIndex,
//             //                 typeWriterSettingIndexSelections.Select(x => x.ToString()).ToArray(),
//             //                 typeWriterSettingIndexSelections
//             //             );

//             //             if(_inspectorShowingTypeWriterSettingIndex >= 0) {
//             //                 SerializedProperty typeWriterProp = typeWriterSettingsProp.GetArrayElementAtIndex(_inspectorShowingTypeWriterSettingIndex)
//             //                     .FindPropertyRelative("_typeWriter");

//             //                 EditorGUI.BeginDisabledGroup(true);
//             //                 EditorGUILayout.PropertyField(typeWriterProp);
//             //                 EditorGUI.EndDisabledGroup();

//             //                 // 別のタイプライターが指定されたらインスペクタを作成し直す
//             //                 if(typeWriterProp.objectReferenceValue != _editingTypeWriter) {
//             //                     _editingTypeWriter = typeWriterProp.objectReferenceValue as TMPE_TypeWriterBase;
//             //                     if(_editingTypeWriter == null) {
//             //                         _typeWriterEditor = null;
//             //                     }
//             //                     else {
//             //                         _typeWriterEditor = CreateEditor(_editingTypeWriter);
//             //                     }
//             //                 }

//             //                 if(_typeWriterEditor != null) {
//             //                     using var _ = new EditorGUILayout.VerticalScope(GroupBoxSkin);
//             //                     _typeWriterEditor.OnInspectorGUI();
//             //                 }
//             //             }
//             //         }
//             //             // EditorGUI.BeginChangeCheck();
//             //             // Object newTypeWriter = EditorGUILayout.ObjectField("Type Writer", typeWriterProp.objectReferenceValue, typeof(TMPE_TypeWriterBase), false);
//             //             // if(EditorGUI.EndChangeCheck()) {
//             //             //     if(newTypeWriter != typeWriterProp.objectReferenceValue) {
//             //             //         Undo.RecordObject(effector, "TypeWriter Changed");
//             //             //         effector.SetTypeWriter(newTypeWriter as TMPE_TypeWriterBase);
//             //             //         _typeWriterEditor = null;
//             //             //     }
//             //             // }
//             //     }
//             // }

//             serializedObject.ApplyModifiedProperties();
//         }
//     }
// }