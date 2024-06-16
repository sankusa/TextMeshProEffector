using System;
using TMPro;

namespace TextMeshProEffector {
    [Serializable]
    public abstract class TMPE_BasicEffect : TMPE_EffectBase {
        public virtual void UpdateVertex(TMPE_Tag tag, TMPE_EffectorBase effector) {
            TMP_TextInfo textInfo = effector.TextInfo;
            for(int i = 0; i < textInfo.characterCount; i++) {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if(tag == null || tag.ContainsIndex(charInfo.index)) {
                    UpdateVertexByCharacter(tag, effector, i);
                }
            }
        }

        protected virtual void UpdateVertexByCharacter(TMPE_Tag tag, TMPE_EffectorBase effector, int charInfoIndex) {}

        public override string GetCaption() => string.IsNullOrEmpty(_tagName) ? "" : $"<{_tagName}>";
    }
}