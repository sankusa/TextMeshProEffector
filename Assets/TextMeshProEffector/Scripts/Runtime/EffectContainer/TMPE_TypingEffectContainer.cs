using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TMPE_TypingEffectContainer), fileName = nameof(TMPE_TypingEffectContainer))]
    public class TMPE_TypingEffectContainer : TMPE_EffectContainerBase<TMPE_TypingEffect> {
        public virtual bool UpdateVertex(TMPE_TypeWriter typeWriter) {
            bool isPlaying = false;

            // タグ無し
            foreach(TMPE_TypingEffect typingEffect in _effects) {
                if(typingEffect == null) continue;
                if(typingEffect.HasTag() == false) {
                    isPlaying |= typingEffect.UpdateVertex(null, typeWriter);
                }
            }

            // タグ有り
            IReadOnlyList<TMPE_Tag> tags = typeWriter.TypingTagContainer.Tags;
            for(int i = 0; i < tags.Count; i++) {
                TMPE_Tag tag = tags[i];
                foreach(TMPE_TypingEffect typingEffect in _effects) {
                    if(typingEffect == null) continue;
                    if(typingEffect.TagName == tag.Name) {
                        isPlaying |= typingEffect.UpdateVertex(tag, typeWriter);
                    }
                }
            }

            return isPlaying;
        }
    }
}