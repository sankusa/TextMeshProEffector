using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TMPE_TypingEventEffectContainer), fileName = nameof(TMPE_TypingEventEffectContainer))]
    public class TMPE_TypingEventEffectContainer : TMPE_EffectContainerBase<TMPE_TypingEventEffect> {
        public void ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming timing, TMPE_TypeWriter typeWriter, int typeIndex) {
            TMPE_EffectorBase effector = typeWriter.Effector;
            TMP_TextInfo textInfo = effector.TextInfo;
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[typeIndex];
            int indexInTmpeTagRemovedText = characterInfo.index;

            // タグ無し
            foreach(TMPE_TypingEventEffect effect in _effects) {
                if(effect == null) continue;
                if(timing == effect.Timing && effect.HasTag() == false) {
                    effect.OnEventTriggerd(null, typeWriter, typeIndex);
                }
            }
            
            // タグ有り
            IReadOnlyList<TMPE_Tag> tags = typeWriter.TypingEventTagContainer.Tags;
            for(int i = 0; i < tags.Count; i++) {
                TMPE_Tag tag = tags[i];
                if(tag.IsClosed() == false && tag.StartIndex != indexInTmpeTagRemovedText) continue;
                if(tag.IsClosed() && tag.ContainsIndex(indexInTmpeTagRemovedText) == false) continue;

                foreach(TMPE_TypingEventEffect effect in _effects) {
                    if(effect == null) continue;
                    if(timing == effect.Timing && effect.TagName == tag.Name) {
                        effect.OnEventTriggerd(tag, typeWriter, typeIndex);
                    }
                }
            }
        }
    }
}