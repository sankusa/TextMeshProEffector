using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_EffectBase), true)]
    public class TMPE_EffectBaseInspector : Editor {
        private static Type[] types;
        public override void OnInspectorGUI() {
            serializedObject.Update();

            if(types == null) types = TypeCache.GetTypesDerivedFrom<TMPE_EffectBase>().Where(x => x.IsAbstract == false).ToArray();

            SerializedProperty tagNameProp = serializedObject.FindProperty("_tagName");

            TMPE_EffectBase effect = target as TMPE_EffectBase;

            SerializedProperty iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);
            int depth = iterator.depth;
            do {
                if(iterator.name == "m_Script") continue;
                EditorGUILayout.PropertyField(iterator);
            } while(iterator.NextVisible(false) && iterator.depth == depth);

            serializedObject.ApplyModifiedProperties();
        }
    }
}