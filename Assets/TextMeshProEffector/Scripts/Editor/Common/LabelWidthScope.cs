using UnityEditor;
using UnityEngine;

namespace TextMeshProEffector {
    public class LabelWidthScope : GUI.Scope {
        private float _originalWidth;

        public LabelWidthScope(float overWriteWidth) {
            _originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = overWriteWidth;
        }

        protected override void CloseScope() {
            EditorGUIUtility.labelWidth = _originalWidth;
        }
    }
}