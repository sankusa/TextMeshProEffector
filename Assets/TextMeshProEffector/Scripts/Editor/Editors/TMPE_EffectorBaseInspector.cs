using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_EffectorBase), true)]
    public class TMPE_EffectorBaseInspector : Editor {
        private EffectContainersReorderableList<TMPE_BasicEffectContainer> _effectContainerList;
        private static GUIStyle _groupBoxSkin;
        private static GUIStyle GroupBoxSkin => _groupBoxSkin ??= new GUIStyle("GroupBox") {margin = new RectOffset()};

        private ReorderableList _typeWriterList;

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
            SerializedProperty basicEffectContainersProp = serializedObject.FindProperty("_basicEffectContainers");
            SerializedProperty typeWritersProp = serializedObject.FindProperty("_typeWriters");
            
            using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
                TMPE_EffectorBase.ForceUpdateInEditing = EditorGUILayout.Toggle("Preview In Edit Mode (Shared)", TMPE_Effector.ForceUpdateInEditing);
            }

            if(effector is TMPE_Effector) {
                SerializedProperty textProp = serializedObject.FindProperty("_text");
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(textProp);
                if(EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(effector.TextComponent, "Text Changed");
                    effector.SetText(textProp.stringValue);
                }
            }

            EditorGUILayout.PropertyField(defaultVisiblityProp);

            // エフェクトコンテナ
            using (new EditorGUILayout.HorizontalScope()) {
                if(_effectContainerList == null) {
                    _effectContainerList = new EffectContainersReorderableList<TMPE_BasicEffectContainer>(basicEffectContainersProp, "Basic Effect Containers");
                }
                _effectContainerList.DoLayoutList();
            }

            // タイプライター
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