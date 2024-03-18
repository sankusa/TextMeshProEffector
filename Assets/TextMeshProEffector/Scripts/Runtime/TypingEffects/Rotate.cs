using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEffects {
    [CreateAssetMenu(fileName = nameof(Rotate), menuName = nameof(TextMeshProEffector) + "/" + nameof(TypingEffects) + "/" + nameof(Rotate))]
    public class Rotate : TMPE_TypingEffect {
        [SerializeField, Min(0)] private float _delay;
        [SerializeField, Min(0)] private float _duration;
        [SerializeField] private Vector3 _from;
        [SerializeField] private Vector3 _to;
        [SerializeField] private Vector3 _pivot;
        [SerializeField] private AnimationCurve _progressCurve = new AnimationCurve();

        protected override bool UpdateVertexByCharacter(TMPE_Tag tag, TMPE_EffectorBase effector, int charInfoIndex, float elapsedTime) {
            TMP_TextInfo textInfo = effector.TextInfo;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charInfoIndex];
            
            if(charInfo.isVisible == false) return false;

            float elapsed = elapsedTime;
            float progress = _duration == 0 ? 1 : Mathf.Clamp01((elapsedTime - _delay) / _duration);
            float curveValue = _progressCurve.Evaluate(progress);
            Vector3 rotation = Vector3.Lerp(_from, _to, curveValue);

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            Vector3 center
                =(vertices[vertexIndex + 0]
                + vertices[vertexIndex + 1]
                + vertices[vertexIndex + 2]
                + vertices[vertexIndex + 3])
                / 4
                + _pivot;

            Quaternion quaternion = Quaternion.Euler(rotation);

            vertices[vertexIndex + 0] = quaternion * (vertices[vertexIndex + 0] - center) + center;
            vertices[vertexIndex + 1] = quaternion * (vertices[vertexIndex + 1] - center) + center;
            vertices[vertexIndex + 2] = quaternion * (vertices[vertexIndex + 2] - center) + center;
            vertices[vertexIndex + 3] = quaternion * (vertices[vertexIndex + 3] - center) + center;

            return progress < 1;
        }
    }
}