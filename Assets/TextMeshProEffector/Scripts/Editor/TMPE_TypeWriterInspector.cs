using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_TypeWriter), true)]
    public class TMPE_TypeWriterInspector : Editor {
        private static GUIStyle _groupBoxSkin;
        private static GUIStyle GroupBoxSkin => _groupBoxSkin ??= new GUIStyle("GroupBox") {margin = new RectOffset()};

        private Editor _settingInspector;

        private EffectContainerReorderableList<TMPE_TypingEffectContainer> _typingEffectContainer;
        private EffectContainerReorderableList<TMPE_TypingEventEffectContainer> _typingEventEffectContainer;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_TypeWriter typeWriter = target as TMPE_TypeWriter;
            TMPE_EffectorBase effector = typeWriter.Effector;

            SerializedProperty typingBehaviourProp = serializedObject.FindProperty("_typingBehaviour");
            SerializedProperty typingEffectContainersProp = serializedObject.FindProperty("_typingEffectContainers");
            SerializedProperty typingEventEffectContainersProp = serializedObject.FindProperty("_typingEventEffectContainers");
            SerializedProperty visualizeCharactersProp = serializedObject.FindProperty("_visualizeCharacters");
            SerializedProperty startTypingAutoProp = serializedObject.FindProperty("_startTypingAuto");

            using (new EditorGUILayout.HorizontalScope()) {
                Object newTypeWriterObj = EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(typingBehaviourProp.name), typingBehaviourProp.objectReferenceValue, typeof(TMPE_TypingBehaviourBase), false);
                if(newTypeWriterObj != typingBehaviourProp.objectReferenceValue) {
                    TMPE_TypingBehaviourBase oldTB = typingBehaviourProp.objectReferenceValue as TMPE_TypingBehaviourBase;
                    TMPE_TypingBehaviourBase newTB = newTypeWriterObj as TMPE_TypingBehaviourBase;
Debug.Log(oldTB + "/ " + newTB + "/ " + effector); 
                    if(newTB != null && effector.TypeWriters.Where(x => x != null &&  x.TypingBehaviour == newTB).Count() > 0) {
                        Debug.Log("TypeWriter cannot be duplicated");
                    }
                    else {
                        if(oldTB != null) {
                            oldTB.OnDetach(effector);
                        }

                        typingBehaviourProp.objectReferenceValue = newTypeWriterObj;

                        serializedObject.ApplyModifiedProperties();
                        
                        if(newTB != null) {
                            newTB.OnAttach(effector);
                            newTB.OnTextChanged(effector);
                        }

                        _settingInspector = null;
                    }
                }

                if(GUILayout.Button("Inspector", GUILayout.Width(70), GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
                    InspectorUtil.OpenAnotherInspector(typingBehaviourProp.objectReferenceValue);
                }
            }

            EditorGUILayout.Separator();

            if(_typingEffectContainer == null) _typingEffectContainer = new EffectContainerReorderableList<TMPE_TypingEffectContainer>(typingEffectContainersProp, "Typing Effect Containers");
            _typingEffectContainer.DoLayoutList();

            EditorGUILayout.Separator();

            if(_typingEventEffectContainer == null) _typingEventEffectContainer = new EffectContainerReorderableList<TMPE_TypingEventEffectContainer>(typingEventEffectContainersProp, "Typing Event Effect Containers");
            _typingEventEffectContainer.DoLayoutList();

            EditorGUILayout.Separator();

            // Rect separatorRect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            // EditorGUI.DrawRect(separatorRect, Color.gray);

            EditorGUILayout.PropertyField(visualizeCharactersProp);
            EditorGUILayout.PropertyField(startTypingAutoProp);

            if(startTypingAutoProp.enumValueIndex == (int)AutoStartTypingType.OnOtherTypeWriterStartedTyping) {
                SerializedProperty targetTypeWriterIndexProp = serializedObject.FindProperty("_targetTypeWriterIndex");
                SerializedProperty delayFromTargetTypeWriterStartedTypingProp = serializedObject.FindProperty("_delayFromTargetTypeWriterStartedTyping");

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(targetTypeWriterIndexProp);
                EditorGUILayout.PropertyField(delayFromTargetTypeWriterStartedTypingProp);
                EditorGUI.indentLevel--;
            }

            SerializedProperty repeatProp = serializedObject.FindProperty("_repeat");
            EditorGUILayout.PropertyField(repeatProp);

            if(repeatProp.boolValue) {
                SerializedProperty repeatIntervalProp = serializedObject.FindProperty("_repeatInterval");

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(repeatIntervalProp);
                EditorGUI.indentLevel--;
            }

            SerializedProperty onCharacterTypedProp = serializedObject.FindProperty("_onCharacterTyped");
            EditorGUILayout.PropertyField(onCharacterTypedProp);

            SerializedProperty onTypingCompletedProp = serializedObject.FindProperty("_onTypingCompleted");
            EditorGUILayout.PropertyField(onTypingCompletedProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
