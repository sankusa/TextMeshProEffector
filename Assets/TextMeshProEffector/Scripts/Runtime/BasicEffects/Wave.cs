using System.Collections;
using System.Collections.Generic;
using TextMeshProEffector;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.BasicEffects {
    public class Wave : TMPE_BasicEffect {
        [SerializeField] private Vector3 _amplitude = Vector3.zero;
        [SerializeField] private Vector3 _duration = Vector3.zero;
        [SerializeField] private Vector3 _phase = Vector3.zero;
        [SerializeField] private Vector3 _phaseByCharacter = Vector3.zero;
        [SerializeField] private AnimationCurve _progressCurveX = new AnimationCurve();
        [SerializeField] private AnimationCurve _progressCurveY = new AnimationCurve();
        [SerializeField] private AnimationCurve _progressCurveZ = new AnimationCurve();

        protected override void UpdateVertexByCharacter(TMPE_Tag tag, TMPE_EffectorBase effector, int charInfoIndex) {
            TMP_TextInfo textInfo = effector.TextInfo;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charInfoIndex];
            if(charInfo.isVisible == false) return;

            float elapsed = effector.ElapsedTimeFromTextChanged;

            // float amplitude = _amplitude;
            // float speed = _speed;
            // float delay = _delay;

            // if(tag != null) {
            //     var a = tag.GetAttribute("a");
            //     if(a != null) {
            //         amplitude = float.Parse(a.Value);
            //     }
                
            //     var s = tag.GetAttribute("s");
            //     if(s != null) {
            //         speed = float.Parse(s.Value);
            //     }

            //     var d = tag.GetAttribute("d");
            //     if(d != null) {
            //         delay = float.Parse(d.Value);
            //     }
            // }

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            
            Vector3[] currentVertices = textInfo.meshInfo[materialIndex].vertices;

            float waveX;
            if(_duration.x == 0) {
                waveX = 0;
            }
            else {
                float progresX = (elapsed / _duration.x + _phase.x + _phaseByCharacter.x * charInfoIndex) % 1;
                if(progresX < 0) progresX++;
                waveX = _amplitude.x * Mathf.Sin(2f * Mathf.PI * _progressCurveX.Evaluate(progresX));
            }
            float waveY;
            if(_duration.y == 0) {
                waveY = 0;
            }
            else {
                float progresY = (elapsed / _duration.y + _phase.y + _phaseByCharacter.y * charInfoIndex) % 1;
                if(progresY < 0) progresY++;
                waveY = _amplitude.y * Mathf.Sin(2f * Mathf.PI * _progressCurveY.Evaluate(progresY));
            }
            float waveZ;
            if(_duration.z == 0) {
                waveZ = 0;
            }
            else {
                float progresZ = (elapsed / _duration.z + _phase.z + _phaseByCharacter.z * charInfoIndex) % 1;
                if(progresZ < 0) progresZ++;
                waveZ = _amplitude.z * Mathf.Sin(2f * Mathf.PI * _progressCurveZ.Evaluate(progresZ));
            }
                
            currentVertices[vertexIndex + 0].x = currentVertices[vertexIndex + 0].x + waveX;
            currentVertices[vertexIndex + 1].x = currentVertices[vertexIndex + 1].x + waveX;
            currentVertices[vertexIndex + 2].x = currentVertices[vertexIndex + 2].x + waveX;
            currentVertices[vertexIndex + 3].x = currentVertices[vertexIndex + 3].x + waveX;
            currentVertices[vertexIndex + 0].y = currentVertices[vertexIndex + 0].y + waveY;
            currentVertices[vertexIndex + 1].y = currentVertices[vertexIndex + 1].y + waveY;
            currentVertices[vertexIndex + 2].y = currentVertices[vertexIndex + 2].y + waveY;
            currentVertices[vertexIndex + 3].y = currentVertices[vertexIndex + 3].y + waveY;
            currentVertices[vertexIndex + 0].z = currentVertices[vertexIndex + 0].z + waveZ;
            currentVertices[vertexIndex + 1].z = currentVertices[vertexIndex + 1].z + waveZ;
            currentVertices[vertexIndex + 2].z = currentVertices[vertexIndex + 2].z + waveZ;
            currentVertices[vertexIndex + 3].z = currentVertices[vertexIndex + 3].z + waveZ;
        }

        public override string GetToolTip() {
return
@"a : amplitude
s : speed
d : delay";
        }
    }
}