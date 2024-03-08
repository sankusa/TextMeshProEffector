using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEffects {
    public class Position : TMPE_TypingEffect {
        [SerializeField, Min(0)] private float _delay;
        [SerializeField, Min(0)] private float _duration;
        [SerializeField] private Vector3 _from;
        [SerializeField] private Vector3 _to;
        [SerializeField] private AnimationCurve _curve = new AnimationCurve();

        protected override bool UpdateTextInfoByCharacter(TMPE_Tag tag, IEffector effector, TMP_TextInfo textInfo, TMP_CharacterInfo charInfo, int charInfoIndex) {
            if(charInfo.isVisible == false) return false;

            float elapsed = effector.ElapsedTimesFromTyped[charInfoIndex];
            float progress = Mathf.Clamp01((elapsed - _delay) / _duration);
            float curveValue = _curve.Evaluate(progress);
            Vector3 offset = Vector3.Lerp(_from, _to, curveValue);

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            vertices[vertexIndex + 0] = vertices[vertexIndex + 0] + offset;
            vertices[vertexIndex + 1] = vertices[vertexIndex + 1] + offset;
            vertices[vertexIndex + 2] = vertices[vertexIndex + 2] + offset;
            vertices[vertexIndex + 3] = vertices[vertexIndex + 3] + offset;

            return progress < 1;
        }
    }
}