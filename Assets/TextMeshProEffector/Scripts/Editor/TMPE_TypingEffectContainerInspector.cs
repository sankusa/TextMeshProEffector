using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_TypingEffectContainer))]
    public class TMPE_TypingEffectContainerInspector : Editor {
        private EffectReorderableList<TMPE_TypingEffect> _typingEffectList;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_TypingEffectContainer effectContainer = target as TMPE_TypingEffectContainer;

            SerializedProperty typingEffectsProp = serializedObject.FindProperty("_typingEffects");
            if(_typingEffectList == null) _typingEffectList = new EffectReorderableList<TMPE_TypingEffect>(typingEffectsProp, effectContainer.TypingEffects, "Typing Effects");
            _typingEffectList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}