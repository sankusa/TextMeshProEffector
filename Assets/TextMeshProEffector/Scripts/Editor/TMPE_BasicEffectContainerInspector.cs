using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_BasicEffectContainer))]
    public class TMPE_BasicEffectContainerInspector : Editor {
        private EffectReorderableList<TMPE_BasicEffect> _basicEffectList;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_BasicEffectContainer effectContainer = target as TMPE_BasicEffectContainer;

            SerializedProperty basicEffectsProp = serializedObject.FindProperty("_basicEffects");
            if(_basicEffectList == null) _basicEffectList = new EffectReorderableList<TMPE_BasicEffect>(basicEffectsProp, effectContainer.BasicEffects, "Basic Effects");
            _basicEffectList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}