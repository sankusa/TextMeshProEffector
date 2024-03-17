using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TextMeshProEffector {
    [Serializable]
    public class OneShotUnityEvent {
        [NonSerialized] private bool _isInvoked;
        public bool IsInvoked => _isInvoked;

        [SerializeField] private UnityEvent _unityEvent;
        public UnityEvent UnityEvent => _unityEvent;

        public void Invoke() {
            if(_isInvoked) throw new InvalidOperationException(" UnityEvent has already been invoked. Reset() requied.");
            _unityEvent.Invoke();
            _isInvoked = true;
        }

        public void Reset() {
            _isInvoked = false;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(OneShotUnityEvent))]
    public class OneShotUnityEventDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("_unityEvent"), label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_unityEvent"), label);
        }
    }

#endif
}