using System;
using TMPro;

namespace TextMeshProEffector {
    [Serializable]
    public abstract class TMPE_TypingEffect : TMPE_EffectBase {
        public virtual bool UpdateVertex(TMPE_Tag tag, TMPE_TypeWriter typeWriter) {
            TMPE_EffectorBase effector = typeWriter.Effector;
            TMP_TextInfo textInfo = effector.TextInfo;
            bool isPlaying = false;
            for(int i = 0; i < textInfo.characterCount; i++) {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                TMPE_CharacterTypingStatus charStatus = typeWriter.CharacterTypingStatuses[i];
                if(tag == null || tag.ContainsIndex(charInfo.index)) {
                    if(effector.TypingInfo[i].IsTyped() && charStatus.IsTyped()) {
                        isPlaying |= UpdateVertexByCharacter(tag, effector, i, charStatus.ElapsedTimeForTypingEffect);
                    }
                }
            }
            return isPlaying;
        }

        protected virtual bool UpdateVertexByCharacter(TMPE_Tag tag, TMPE_EffectorBase effector, int charInfoIndex, float elapsedTime) => false;

        public override string GetCaption() => string.IsNullOrEmpty(_tagName) ? "" : $"<{TMPE_TagSyntax.TAG_PREFIX_TYPING}{_tagName}>";
    }
}