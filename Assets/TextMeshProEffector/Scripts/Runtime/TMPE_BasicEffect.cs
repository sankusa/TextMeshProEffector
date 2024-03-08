using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [Serializable]
    public abstract class TMPE_BasicEffect : TMPE_EffectBase {
        public virtual void UpdateTextInfo(TMPE_Tag tag, IEffector effector, TMP_TextInfo textInfo) {
            for(int i = 0; i < textInfo.characterCount; i++) {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if(tag == null || (tag.StartIndex <= charInfo.index && charInfo.index <= tag.EndIndex)) {
                    UpdateTextInfoByCharacter(tag, effector, textInfo, charInfo, i);
                }
            }
        }

        protected virtual void UpdateTextInfoByCharacter(TMPE_Tag tag, IEffector effector, TMP_TextInfo textInfo, TMP_CharacterInfo charInfo, int charInfoIndex) {}
    }
}