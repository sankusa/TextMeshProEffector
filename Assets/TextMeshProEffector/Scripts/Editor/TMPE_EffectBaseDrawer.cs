using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace TextMeshProEffector {
    [CustomPropertyDrawer(typeof(TMPE_EffectBase))]
    public class TMPE_EffectBaseDrawer : PropertyDrawer {
        private static Type[] types;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if(types == null) types = TypeCache.GetTypesDerivedFrom<TMPE_EffectBase>().Where(x => x.IsAbstract == false).ToArray();

            SerializedProperty tagNameProp = property.FindPropertyRelative("_tagName");
            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            position.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            int typeNameStartIndex = property.type.LastIndexOf('<') + 1;
            string[] name = property.managedReferenceFullTypename.Split(' ');
            Type type = types.FirstOrDefault(x => x.FullName == name[1]);
            MethodInfo toolTipGetter = type.GetMethod("GetToolTip", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            string tooltip = (string) toolTipGetter?.Invoke(null, null);

            string tagHeader = "";
            if(type.IsSubclassOf(typeof(TMPE_TypingEffect))) {
                tagHeader = "+";
            }
            else if(type.IsSubclassOf(typeof(TMPE_TypingEventEffect))) {
                tagHeader = "!";
            }

            if(GUI.Button(headerRect, new GUIContent(tagNameProp.stringValue.Length > 0 ? $"{type.Name} -------- <{tagHeader}{tagNameProp.stringValue}>" : $"{type.Name} -------- All Characters", tooltip))) {
                property.isExpanded = !property.isExpanded;
            }
            if(property.isExpanded) {
                property.NextVisible(true);
                int depth = property.depth;
                do {
                    float propertyHeight = EditorGUI.GetPropertyHeight(property);
                    Rect propertyRect = new Rect(position.x, position.y, position.width, propertyHeight);
                    position.yMin += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(propertyRect, property);
                } while(property.NextVisible(false) && property.depth == depth);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if(property.isExpanded) {
                property.NextVisible(true);
                int depth = property.depth;
                do {
                    height += EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.standardVerticalSpacing;
                } while(property.NextVisible(false) && property.depth == depth);
            }
            return height;
        }
    }
}