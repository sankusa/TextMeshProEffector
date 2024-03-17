using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEffects {
    public class Move : TMPE_TypingEffect {
        [SerializeField, Min(0)] private float _delay;
        [SerializeField, Min(0)] private float _duration;
        [SerializeField] private Vector3 _from;
        [SerializeField] private Vector3 _to;
        [SerializeField] private AnimationCurve _curve = new AnimationCurve();

        protected override bool UpdateVertexByCharacter(TMPE_Tag tag, TMPE_EffectorBase effector, int charInfoIndex, float elapsedTime) {
            TMP_TextInfo textInfo = effector.TextInfo;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charInfoIndex];
            
            if(charInfo.isVisible == false) return false;

            float elapsed = elapsedTime;
            float progress = _duration == 0 ? 1 : Mathf.Clamp01((elapsedTime - _delay) / _duration);
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