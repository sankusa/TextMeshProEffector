using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [Serializable]
    public abstract class TMPE_BasicEffect : TMPE_EffectBase {
        public virtual void UpdateVertex(TMPE_Tag tag, TMPE_EffectorBase effector) {
            TMP_TextInfo textInfo = effector.TextInfo;
            for(int i = 0; i < textInfo.characterCount; i++) {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if(tag == null || (tag.StartIndex <= charInfo.index && charInfo.index <= tag.EndIndex)) {
                    UpdateVertexByCharacter(tag, effector, i);
                }
            }
        }

        protected virtual void UpdateVertexByCharacter(TMPE_Tag tag, TMPE_EffectorBase effector, int charInfoIndex) {}
    }
}