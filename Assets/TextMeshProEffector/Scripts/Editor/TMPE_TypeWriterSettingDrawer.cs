using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomPropertyDrawer(typeof(TMPE_TypeWriterSetting))]
    public class TMPE_TypeWriterSettingDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            IEffector effector = property.serializedObject.targetObject as IEffector;
            SerializedProperty typeWriterProp = property.FindPropertyRelative("_typeWriter");
            SerializedProperty startTypingAutoProp = property.FindPropertyRelative("_startTypingAuto");

            Rect typeWriterRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Rect startTypingAutoRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            Object newTypeWriterObj = EditorGUI.ObjectField(typeWriterRect, ObjectNames.NicifyVariableName(typeWriterProp.name), typeWriterProp.objectReferenceValue, typeof(TMPE_TypeWriterBase), false);
            if(newTypeWriterObj != typeWriterProp.objectReferenceValue) {
                TMPE_TypeWriterBase oldTypeWriter = typeWriterProp.objectReferenceValue as TMPE_TypeWriterBase;
                TMPE_TypeWriterBase newTypeWriter = newTypeWriterObj as TMPE_TypeWriterBase;

                if(newTypeWriter != null && effector.TypeWriterSettings.Where(x => x.TypeWriter == newTypeWriter).Count() > 0) {
                    Debug.Log("TypeWriter cannot be duplicated");
                }
                else {
                    if(oldTypeWriter != null) {
                        oldTypeWriter.OnDetach(effector);
                    }

                    typeWriterProp.objectReferenceValue = newTypeWriterObj;

                    property.serializedObject.ApplyModifiedProperties();
                    
                    if(newTypeWriter != null) {
                        newTypeWriter.OnAttach(effector);
                        newTypeWriter.OnTextChanged(effector);
                    }
                    
                }
            }

            EditorGUI.PropertyField(startTypingAutoRect, startTypingAutoProp);

            if(startTypingAutoProp.enumValueIndex == (int)AutoStartTypingType.OnOtherTypeWriterStartedTyping) {
                SerializedProperty targetTypeWriterIndexProp = property.FindPropertyRelative("_targetTypeWriterIndex");
                SerializedProperty delayFromTargetTypeWriterStartedTypingProp = property.FindPropertyRelative("_delayFromTargetTypeWriterStartedTyping");

                Rect targetTypeWriterIndexRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};
                position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                Rect delayFromTargetTypeWriterStartedTypingRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};
                position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(targetTypeWriterIndexRect, targetTypeWriterIndexProp);
                EditorGUI.PropertyField(delayFromTargetTypeWriterStartedTypingRect, delayFromTargetTypeWriterStartedTypingProp);
                EditorGUI.indentLevel--;
            }

            SerializedProperty repeatProp = property.FindPropertyRelative("_repeat");
            Rect repeatRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(repeatRect, repeatProp);

            if(repeatProp.boolValue) {
                SerializedProperty repeatIntervalProp = property.FindPropertyRelative("_repeatInterval");
                Rect repeatIntervalRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};
                position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(repeatIntervalRect, repeatIntervalProp);
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty startTypingAutoProp = property.FindPropertyRelative("_startTypingAuto");

            float height = EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
            if(startTypingAutoProp.enumValueIndex == (int)AutoStartTypingType.OnOtherTypeWriterStartedTyping) {
                height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
            }

            SerializedProperty repeatProp = property.FindPropertyRelative("_repeat");
            if(repeatProp.boolValue) {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }
    }
}