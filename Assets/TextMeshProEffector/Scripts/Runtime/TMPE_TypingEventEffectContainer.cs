using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TMPE_TypingEventEffectContainer), fileName = nameof(TMPE_TypingEventEffectContainer))]
    public class TMPE_TypingEventEffectContainer : ScriptableObject {
        [SerializeReference] private List<TMPE_TypingEventEffect> _typingEventEffects;
        public List<TMPE_TypingEventEffect> TypingEventEffects => _typingEventEffects;

        public void ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming timing, TMPE_EffectorBase effector, int typeWriterIndex, int typeIndex) {
            TMP_TextInfo textInfo = effector.TextInfo;
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[typeIndex];
            int indexInTmpeTagRemovedText = characterInfo.index;
            List<TMPE_Tag> tags = effector.TagContainer.TypingEventTags[typeWriterIndex];
            TMPE_TypeWriter typeWriter = effector.TypeWriters[typeWriterIndex];

            foreach(TMPE_TypingEventEffect typingEventEffect in _typingEventEffects) {
                if(typingEventEffect == null) continue;
                if(timing == typingEventEffect.Timing && string.IsNullOrEmpty(typingEventEffect.TagName)) {
                    typingEventEffect.OnEventTriggerd(null, effector, typeWriter.TypingBehaviour, typeIndex);
                }
            }
            
            foreach(TMPE_Tag tag in tags) {
                if(tag.EndIndex == -1 && tag.StartIndex != indexInTmpeTagRemovedText) continue;
                if(tag.EndIndex != -1 && (indexInTmpeTagRemovedText < tag.StartIndex || tag.EndIndex < indexInTmpeTagRemovedText)) continue;

                foreach(TMPE_TypingEventEffect typingEventEffect in _typingEventEffects) {
                    if(typingEventEffect == null) continue;
                    if(timing == typingEventEffect.Timing && typingEventEffect.TagName == tag.Name) {
                        typingEventEffect.OnEventTriggerd(tag, effector, typeWriter.TypingBehaviour, typeIndex);
                    }
                }
            }
        }

        public bool IsValidTypingEventTag(TMPE_Tag tag) {
            foreach(TMPE_TypingEventEffect typingEventEffect in _typingEventEffects) {
                if(typingEventEffect == null) continue;
                if(typingEventEffect.TagName == tag.Name && typingEventEffect.ValidateTag(tag)) {
                    return true;
                }
            }
            return false;
        }
    }
}