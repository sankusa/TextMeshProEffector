using System.Collections;
using System.Collections.Generic;
using Kodama.TextMeshProEffector;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    public class Position : TMPE_BasicEffect {
        [SerializeField] private float _duration;
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private Vector3 _from;
        [SerializeField] private Vector3 _to;
        

        // public override void PerformElement(TMPE_Effector effector, TMP_TextInfo textInfo, TMP_CharacterInfo charInfo, TMPE_Tag tag, int charIndex) {
        //     if(charInfo.isVisible == false) return;

        //     int materialIndex = charInfo.materialReferenceIndex;
        //     int vertexIndex = charInfo.vertexIndex;

        //     Vector3[] currentVertices = textInfo.meshInfo[materialIndex].vertices;

        //     Vector3 offset = Vector3.Lerp(_from, _to, animationCurve.Evaluate(effector.ElapsedTimeFromTextChanged / _duration));

        //     currentVertices[vertexIndex + 0] = currentVertices[vertexIndex + 0] + offset;
        //     currentVertices[vertexIndex + 1] = currentVertices[vertexIndex + 1] + offset;
        //     currentVertices[vertexIndex + 2] = currentVertices[vertexIndex + 2] + offset;
        //     currentVertices[vertexIndex + 3] = currentVertices[vertexIndex + 3] + offset;
        // }
    }
}