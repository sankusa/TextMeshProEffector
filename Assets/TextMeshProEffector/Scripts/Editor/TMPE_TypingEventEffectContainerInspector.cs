using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_TypingEventEffectContainer))]
    public class TMPE_TypingEventEffectContainerInspector : Editor {
        private EffectReorderableList<TMPE_TypingEventEffect> _effectList;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_TypingEventEffectContainer effectContainer = target as TMPE_TypingEventEffectContainer;

            SerializedProperty effectsProp = serializedObject.FindProperty("_typingEventEffects");
            if(_effectList == null) _effectList = new EffectReorderableList<TMPE_TypingEventEffect>(effectsProp, effectContainer.TypingEventEffects, "Typing Effects");
            _effectList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}