using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [Serializable]
    public abstract class TMPE_TypingEffect : TMPE_EffectBase {
        public virtual bool UpdateVertex(TMPE_Tag tag, IEffector effector, TMPE_TypeWriterBase typeWriter) {
            TMP_TextInfo textInfo = effector.TextInfo;
            bool isPlaying = false;
            for(int i = 0; i < textInfo.characterCount; i++) {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if(tag == null || (tag.StartIndex <= charInfo.index && charInfo.index <= tag.EndIndex)) {
                    if(effector.TypingInfo[i].Visiblity == CharacterVisiblity.Visible) {
                        isPlaying |= UpdateTextInfoByCharacter(tag, effector, i, typeWriter.GetElapsedTimeFromTypedByCharacter(effector, i));
                    }
                }
            }
            return isPlaying;
        }

        protected virtual bool UpdateTextInfoByCharacter(TMPE_Tag tag, IEffector effector, int charInfoIndex, float elapsedTime) => false;
    }
}