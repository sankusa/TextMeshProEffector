using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [Serializable]
    public abstract class TMPE_TypingEffect : TMPE_EffectBase {
        public virtual bool UpdateVertex(TMPE_Tag tag, TMPE_TypeWriter typeWriter, TMPE_TypingBehaviourBase typingBehaviour) {
            TMPE_EffectorBase effector = typeWriter.Effector;
            TMP_TextInfo textInfo = effector.TextInfo;
            bool isPlaying = false;
            for(int i = 0; i < textInfo.characterCount; i++) {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if(tag == null || (tag.StartIndex <= charInfo.index && charInfo.index <= tag.EndIndex)) {
                    if(effector.TypingInfo[i].Visiblity == CharacterVisiblity.Visible && typingBehaviour.IsTypedCharacter(typeWriter, i)) {
                        isPlaying |= UpdateVertexByCharacter(tag, effector, i, typingBehaviour.GetElapsedTimeForTypingEffect(typeWriter, i));
                    }
                }
            }
            return isPlaying;
        }

        protected virtual bool UpdateVertexByCharacter(TMPE_Tag tag, TMPE_EffectorBase effector, int charInfoIndex, float elapsedTime) => false;

        public override string GetCaption() => string.IsNullOrEmpty(_tagName) ? "" : $"<+{_tagName}>";
    }
}