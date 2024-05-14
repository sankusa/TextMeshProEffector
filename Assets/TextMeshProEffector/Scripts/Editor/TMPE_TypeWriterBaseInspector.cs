// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor;
// using UnityEditorInternal;
// using UnityEngine;

// namespace TextMeshProEffector {
//     [CustomEditor(typeof(TMPE_TypeWriterBase), true)]
//     public class TMPE_TypeWriterBaseInspector : Editor {
//         private EffectReorderableList<TMPE_TypingEffect> _typingEffectList;
//         private EffectReorderableList<TMPE_TypingEventEffect> _typingEventEffectList;

//         public override void OnInspectorGUI() {
//             serializedObject.Update();

//             TMPE_TypeWriterBase typeWriter = target as TMPE_TypeWriterBase;

//             // // TypingEffects
//             // SerializedProperty typingEffectsProp = serializedObject.FindProperty("_typingEffects");
//             // if(_typingEffectList == null) _typingEffectList = new EffectReorderableList<TMPE_TypingEffect>(typingEffectsProp, typeWriter.TypingEffects, "Typing Effects");
//             // _typingEffectList.DoLayoutList();

//             // // TypingEffects
//             // SerializedProperty typingEventEffectsProp = serializedObject.FindProperty("_typingEventEffects");
//             // if(_typingEventEffectList == null) _typingEventEffectList = new EffectReorderableList<TMPE_TypingEventEffect>(typingEventEffectsProp, typeWriter.TypingEventEffects, "Typing Event Effects");
//             // _typingEventEffectList.DoLayoutList();

//             base.OnInspectorGUI();

//             serializedObject.ApplyModifiedProperties();
//         }
//     }
// }