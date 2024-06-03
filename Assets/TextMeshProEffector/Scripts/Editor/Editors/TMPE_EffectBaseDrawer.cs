using UnityEditor;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomPropertyDrawer(typeof(TMPE_EffectBase), true)]
    public class TMPE_EffectBaseDrawer : PropertyDrawer {
        private static GUIStyle _rightLabelStyle;
        private static GUIStyle RightLabelStyle => _rightLabelStyle ??= new GUIStyle(EditorStyles.label) {alignment = TextAnchor.MiddleRight};

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            TMPE_EffectBase effect = property.GetObject() as TMPE_EffectBase;

            // ヘッダ
            {
                Rect headerRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};
                position.yMin += headerRect.height;

                if(GUI.Button(headerRect, "")) {
                    property.isExpanded = !property.isExpanded;
                }
                // タグが設定されているエフェクトは区別しやすいよう色を付ける
                if(effect.HasTag()) {
                    EditorGUI.DrawRect(headerRect, new Color(0.05f, 0.2f, 0.1f, 0.5f));
                }

                Rect headerLabelRect = new Rect(headerRect) {xMin = headerRect.xMin + 8, xMax = headerRect.xMax - 8};
                EditorGUI.LabelField(headerLabelRect, effect.GetCaption(), RightLabelStyle);
                EditorGUI.LabelField(headerLabelRect, effect.GetType().Name);
            }

            // 全要素のPropertyFieldを描画
            if(property.isExpanded) {
                property.NextVisible(true);
                int depth = property.depth;
                do {
                    float propertyHeight = EditorGUI.GetPropertyHeight(property);
                    EditorGUI.PropertyField(new Rect(position) {height = propertyHeight}, property);
                    position.yMin += propertyHeight;
                } while(property.NextVisible(false) && property.depth == depth);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = 0;
            height += EditorGUIUtility.singleLineHeight;

            if(property.isExpanded) {
                property.NextVisible(true);
                int depth = property.depth;
                do {
                    height += EditorGUI.GetPropertyHeight(property);
                } while(property.NextVisible(false) && property.depth == depth);
            }
            return height;
        }
    }
}