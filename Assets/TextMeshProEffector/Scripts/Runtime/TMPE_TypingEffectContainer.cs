using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TMPE_TypingEffectContainer), fileName = nameof(TMPE_TypingEffectContainer))]
    public class TMPE_TypingEffectContainer : ScriptableObject {
        [SerializeReference] protected List<TMPE_TypingEffect> _typingEffects;
        public List<TMPE_TypingEffect> TypingEffects => _typingEffects;

        public virtual bool UpdateVertex(TMPE_TypeWriter typeWriter, int typeWriterIndex) {
            TMPE_TypingBehaviourBase typingBehaviour = typeWriter.TypingBehaviour;
            List<TMPE_Tag> tags = typeWriter.Effector.TagContainer.TypingTags[typeWriterIndex];
            bool isPlaying = false;
            foreach(TMPE_TypingEffect typingEffect in _typingEffects) {
                if(typingEffect == null) continue;
                if(string.IsNullOrEmpty(typingEffect.TagName)) {
                    isPlaying |= typingEffect.UpdateVertex(null, typeWriter, typingBehaviour);
                }
            }

            foreach(TMPE_Tag tag in tags) {
                foreach(TMPE_TypingEffect typingEffect in _typingEffects) {
                    if(typingEffect == null) continue;
                    if(typingEffect.TagName == tag.Name) {
                        isPlaying |= typingEffect.UpdateVertex(tag, typeWriter, typingBehaviour);
                    }
                }
            }

            return isPlaying;
        }

        public bool IsValidTypingTag(TMPE_Tag tag) {
            foreach(TMPE_TypingEffect typingEffect in _typingEffects) {
                if(typingEffect == null) continue;
                if(typingEffect.TagName == tag.Name && typingEffect.ValidateTag(tag)) {
                    return true;
                }
            }
            return false;
        }
    }
}