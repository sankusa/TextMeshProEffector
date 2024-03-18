using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEffects {
    [CreateAssetMenu(fileName = nameof(Scale), menuName = nameof(TextMeshProEffector) + "/" + nameof(TypingEffects) + "/" + nameof(Scale))]
    public class Scale : TMPE_TypingEffect {
        [SerializeField, Min(0)] private float _delay;
        [SerializeField, Min(0)] private float _duration;
        [SerializeField] private float _from;
        [SerializeField] private float _to;
        [SerializeField] private AnimationCurve _progressCurve = new AnimationCurve();

        protected override bool UpdateVertexByCharacter(TMPE_Tag tag, TMPE_EffectorBase effector, int charInfoIndex, float elapsedTime) {
            TMP_TextInfo textInfo = effector.TextInfo;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charInfoIndex];
            
            if(charInfo.isVisible == false) return false;

            float elapsed = elapsedTime;
            float progress = _duration == 0 ? 1 : Mathf.Clamp01((elapsedTime - _delay) / _duration);
            float curveValue = _progressCurve.Evaluate(progress);
            float scale = Mathf.Lerp(_from, _to, curveValue);

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            Vector3 center
                =(vertices[vertexIndex + 0]
                + vertices[vertexIndex + 1]
                + vertices[vertexIndex + 2]
                + vertices[vertexIndex + 3])
                / 4;

            vertices[vertexIndex + 0] = (vertices[vertexIndex + 0] - center) * scale + center;
            vertices[vertexIndex + 1] = (vertices[vertexIndex + 1] - center) * scale + center;
            vertices[vertexIndex + 2] = (vertices[vertexIndex + 2] - center) * scale + center;
            vertices[vertexIndex + 3] = (vertices[vertexIndex + 3] - center) * scale + center;

            return progress < 1;
        }
    }
}