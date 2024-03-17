using System.Collections;
using System.Collections.Generic;
using TextMeshProEffector;
using TMPro;
using UnityEngine;

namespace Kodama.TextMeshProEffector {
    public class Wave : TMPE_BasicEffect {
        [SerializeField] private float _amplitude = 1;
        [SerializeField] private float _delay = 0.5f;
        [SerializeField] private float _speed = 1;

        static string GetToolTip() {
return
@"a : amplitude
s : speed
d : delay";
        }

        public override bool ValidateTag(TMPE_Tag tag) {
            if(tag.Value != null) return false;
            foreach(TMPE_TagAttribute attribute in tag.Attributes) {
                if(attribute.Name == "a" || attribute.Name == "s" || attribute.Name == "d") {
                    if(float.TryParse(attribute.Value, out float _) == false) return false;
                }
                else {
                    return false;
                }
            }
            return true;
        }

        protected override void UpdateVertexByCharacter(TMPE_Tag tag, IEffector effector, int charInfoIndex) {
            TMP_TextInfo textInfo = effector.TextInfo;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[charInfoIndex];
            if(charInfo.isVisible == false) return;

            float amplitude = _amplitude;
            float speed = _speed;
            float delay = _delay;

            if(tag != null) {
                var a = tag.GetAttribute("a");
                if(a != null) {
                    amplitude = float.Parse(a.Value);
                }
                
                var s = tag.GetAttribute("s");
                if(s != null) {
                    speed = float.Parse(s.Value);
                }

                var d = tag.GetAttribute("d");
                if(d != null) {
                    delay = float.Parse(d.Value);
                }
            }

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            
            Vector3[] currentVertices = textInfo.meshInfo[materialIndex].vertices;
            
            float sinWaveOffset = _delay * charInfoIndex;
            float sinWave = amplitude *  Mathf.Sin(effector.ElapsedTimeFromTextChanged * speed + sinWaveOffset);
            currentVertices[vertexIndex + 0].y = currentVertices[vertexIndex + 0].y + sinWave;
            currentVertices[vertexIndex + 1].y = currentVertices[vertexIndex + 1].y + sinWave;
            currentVertices[vertexIndex + 2].y = currentVertices[vertexIndex + 2].y + sinWave;
            currentVertices[vertexIndex + 3].y = currentVertices[vertexIndex + 3].y + sinWave;
        }
    }
}