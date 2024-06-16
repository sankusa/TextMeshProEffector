using UnityEditor;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_TypeWriter), true)]
    public class TMPE_TypeWriterInspector : Editor {
        private EffectContainersReorderableList<TMPE_TypingEffectContainer> _typingEffectContainer;
        private EffectContainersReorderableList<TMPE_TypingEventEffectContainer> _typingEventEffectContainer;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_TypeWriter typeWriter = target as TMPE_TypeWriter;

            SerializedProperty typingBehaviourProp = serializedObject.FindProperty("_typingBehaviour");
            SerializedProperty typingEffectContainersProp = serializedObject.FindProperty("_typingEffectContainers");
            SerializedProperty typingEventEffectContainersProp = serializedObject.FindProperty("_typingEventEffectContainers");
            SerializedProperty setToTypedProp = serializedObject.FindProperty("_setToTyped");
            SerializedProperty startTypingAutoProp = serializedObject.FindProperty("_startTypingAuto");

            using (new EditorGUILayout.HorizontalScope()) {
                Object newTypeWriterObj = EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(typingBehaviourProp.name), typingBehaviourProp.objectReferenceValue, typeof(TMPE_TypingBehaviourBase), false);
                if(newTypeWriterObj != typingBehaviourProp.objectReferenceValue) {
                    typingBehaviourProp.objectReferenceValue = newTypeWriterObj;
                    serializedObject.ApplyModifiedProperties();

                    typeWriter.ResetTypinRelatedValues();
                }

                if(GUILayout.Button("Inspector", GUILayout.Width(70), GUILayout.Height(EditorGUIUtility.singleLineHeight))) {
                    InspectorUtil.OpenAnotherInspector(typingBehaviourProp.objectReferenceValue);
                }
            }

            EditorGUILayout.Separator();

            if(_typingEffectContainer == null) _typingEffectContainer = new EffectContainersReorderableList<TMPE_TypingEffectContainer>(typingEffectContainersProp, "Typing Effect Containers");
            _typingEffectContainer.DoLayoutList();

            EditorGUILayout.Separator();

            if(_typingEventEffectContainer == null) _typingEventEffectContainer = new EffectContainersReorderableList<TMPE_TypingEventEffectContainer>(typingEventEffectContainersProp, "Typing Event Effect Containers");
            _typingEventEffectContainer.DoLayoutList();

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(setToTypedProp);
            EditorGUILayout.PropertyField(startTypingAutoProp);

            if(startTypingAutoProp.enumValueIndex == (int)AutoStartTypingType.OnOtherTypeWriterStartedTyping) {
                SerializedProperty otherTypewriterWaitSetting = serializedObject.FindProperty("_otherTypewriterWaitSetting");
                SerializedProperty waitTargetTypeWriterIndexProp = otherTypewriterWaitSetting.FindPropertyRelative("_targetTypeWriterIndex");
                SerializedProperty delayFromTargetTypeWriterStartedTypingProp = otherTypewriterWaitSetting.FindPropertyRelative("_delay");

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(waitTargetTypeWriterIndexProp);
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
