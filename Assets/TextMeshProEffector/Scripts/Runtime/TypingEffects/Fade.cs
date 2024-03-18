using System.Collections;
using System.Collections.Generic;
using TextMeshProEffector;
using UnityEngine;
using TMPro;

namespace TextMeshProEffector.TypingEffects {
    [CreateAssetMenu(fileName = nameof(Fade), menuName = nameof(TextMeshProEffector) + "/" + nameof(TypingEffects) + "/" + nameof(Fade))]
    public class Fade : TMPE_TypingEffect {
        [SerializeField, Min(0)] private float _delay;
        [SerializeField, Min(0)] private float _duration;
        [SerializeField, Range(0, 1)] private float _from;
        [SerializeField, Range(0, 1)] private float _to;
        [SerializeField] private AnimationCurve _progressCurve = new AnimationCurve();

        protected override bool UpdateVertexByCharacter(TMPE_Tag tag, TMPE_EffectorBase effector, int charInfoIndex, float elapsedTime) {
            TMP_TextInfo textInfo = effector.TextInfo;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charInfoIndex];
            if(charInfo.isVisible == false) return false;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            float progress = _duration == 0 ? 1 : Mathf.Clamp01((elapsedTime - _delay) / _duration);
            float curveValue = _progressCurve.Evaluate(progress);
            float alpha = Mathf.Lerp(_from, _to, curveValue);
            
            Color32[] currentColors32 = textInfo.meshInfo[materialIndex].colors32;
            
            currentColors32[vertexIndex + 0].a = (byte)(currentColors32[vertexIndex + 0].a * alpha);
            currentColors32[vertexIndex + 1].a = (byte)(currentColors32[vertexIndex + 1].a * alpha);
            currentColors32[vertexIndex + 2].a = (byte)(currentColors32[vertexIndex + 2].a * alpha);
            currentColors32[vertexIndex + 3].a = (byte)(currentColors32[vertexIndex + 3].a * alpha);

            return progress < 1;
        }
    }
}