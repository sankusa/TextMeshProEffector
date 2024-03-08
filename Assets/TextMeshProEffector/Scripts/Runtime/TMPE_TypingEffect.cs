using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [Serializable]
    public abstract class TMPE_TypingEffect : TMPE_EffectBase {
        public virtual bool UpdateTextInfo(TMPE_Tag tag, IEffector effector, TMP_TextInfo textInfo) {
            bool isPlaying = false;
            for(int i = 0; i < textInfo.characterCount; i++) {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if(tag == null || (tag.StartIndex <= charInfo.index && charInfo.index <= tag.EndIndex)) {
                    if(i < effector.TextComponent.maxVisibleCharacters) {
                        isPlaying |= UpdateTextInfoByCharacter(tag, effector, textInfo, charInfo, i);
                    }
                }
            }
            return isPlaying;
        }

        protected virtual bool UpdateTextInfoByCharacter(TMPE_Tag tag, IEffector effector, TMP_TextInfo textInfo, TMP_CharacterInfo charInfo, int charInfoIndex) => false;
    }
}