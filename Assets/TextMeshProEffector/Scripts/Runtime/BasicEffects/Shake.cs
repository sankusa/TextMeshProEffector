using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.BasicEffects {
    [CreateAssetMenu(fileName = nameof(Shake), menuName = nameof(TextMeshProEffector) + "/" + nameof(BasicEffects) + "/" + nameof(Shake))]
    public class Shake : TMPE_BasicEffect {
        [SerializeField] private float _amplitude = 1;
        [SerializeField, Min(0)] private float _interval = 0;

        public override bool ValidateTag(TMPE_Tag tag) {
            if(tag.Value != null) return false;
            foreach(TMPE_TagAttribute attribute in tag.Attributes) {
                if(attribute.Name == "a") {
                    if(float.TryParse(attribute.Value, out float _) == false) return false;
                }
                else {
                    return false;
                }
            }
            return true;
        }

        protected override void UpdateVertexByCharacter(TMPE_Tag tag, TMPE_EffectorBase effector, int charInfoIndex) {
            TMP_TextInfo textInfo = effector.TextInfo;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charInfoIndex];
            if(charInfo.isVisible == false) return;

            float interval = _interval;
            if(interval == 0) interval = 0.001f;

            float elapsed = effector.ElapsedTimeFromTextChanged + 100 * interval * Mathf.PerlinNoise(charInfoIndex * Mathf.PI, 0);

            int phase = (int)(elapsed / interval);
            float seed = ((phase * 1000) % 100000 + charInfoIndex) * Mathf.PI;
            float amplitude = _amplitude;

            if(tag != null) {
                var a = tag.GetAttribute("a");
                if(a != null) {
                    amplitude = float.Parse(a.Value);
                }
            }

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            
            Vector3[] currentVertices = textInfo.meshInfo[materialIndex].vertices;
            float randomX = (0.5f - Mathf.PerlinNoise(seed, 0)) * amplitude;
            float randomY = (0.5f - Mathf.PerlinNoise(0, seed)) * amplitude;
            currentVertices[vertexIndex + 0].x = currentVertices[vertexIndex + 0].x + randomX;
            currentVertices[vertexIndex + 1].x = currentVertices[vertexIndex + 1].x + randomX;
            currentVertices[vertexIndex + 2].x = currentVertices[vertexIndex + 2].x + randomX;
            currentVertices[vertexIndex + 3].x = currentVertices[vertexIndex + 3].x + randomX;
            currentVertices[vertexIndex + 0].y = currentVertices[vertexIndex + 0].y + randomY;
            currentVertices[vertexIndex + 1].y = currentVertices[vertexIndex + 1].y + randomY;
            currentVertices[vertexIndex + 2].y = currentVertices[vertexIndex + 2].y + randomY;
            currentVertices[vertexIndex + 3].y = currentVertices[vertexIndex + 3].y + randomY;
        }

        public override string GetToolTip() {
return
@"a : amplitude";
        }
    }
}