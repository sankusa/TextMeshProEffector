using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TMPE_EffectContainer), fileName = nameof(TMPE_EffectContainer))]
    public class TMPE_EffectContainer : ScriptableObject {
        [SerializeReference] private List<TMPE_BasicEffect> _basicEffects;
        public List<TMPE_BasicEffect> BasicEffects => _basicEffects;

        [SerializeReference] private List<TMPE_TypingEffect> _typingEffects;
        public List<TMPE_TypingEffect> TypingEffects => _typingEffects;

        [SerializeReference] private List<TMPE_TypingEventEffect> _typingEventEffects;
        public List<TMPE_TypingEventEffect> TypingEventEffects => _typingEventEffects;

        public void UpdateTextInfo_BasicEffect(IEffector effector, TMP_TextInfo textInfo, List<TMPE_Tag> tags) {
            // if(tags == null) return;

            foreach(TMPE_BasicEffect basicEffect in _basicEffects) {
                if(string.IsNullOrEmpty(basicEffect.TagName)) {
                    basicEffect.UpdateTextInfo(null, effector, textInfo);
                }
            }

            foreach(TMPE_Tag tag in tags) {
                foreach(TMPE_BasicEffect basicEffect in _basicEffects) {
                    if(basicEffect.TagName == tag.Name) {
                        basicEffect.UpdateTextInfo(tag, effector, textInfo);
                    }
                }
            }
        }

        public bool UpdateTextInfo_TypingEffect(IEffector effector, TMP_TextInfo textInfo, List<TMPE_Tag> tags) {
            bool isPlaying = false;
            foreach(TMPE_TypingEffect typingEffect in _typingEffects) {
                if(string.IsNullOrEmpty(typingEffect.TagName)) {
                    isPlaying |= typingEffect.UpdateTextInfo(null, effector, textInfo);
                }
            }

            foreach(TMPE_Tag tag in tags) {
                foreach(TMPE_TypingEffect typingEffect in _typingEffects) {
                    if(typingEffect.TagName == tag.Name) {
                        isPlaying |= typingEffect.UpdateTextInfo(tag, effector, textInfo);
                    }
                }
            }

            return isPlaying;
        }

        public void ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming timing, IEffector effector, TMP_TextInfo textInfo, List<TMPE_Tag> tags, int typeIndex) {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[typeIndex];
            int indexInTmpeTagRemovedText = characterInfo.index;

            foreach(TMPE_TypingEventEffect typingEventEffect in _typingEventEffects) {
                if(timing == typingEventEffect.Timing && string.IsNullOrEmpty(typingEventEffect.TagName)) {
                    typingEventEffect.OnEventTriggerd(null, effector, textInfo, characterInfo, typeIndex);
                }
            }
            
            foreach(TMPE_Tag tag in tags) {
                if(tag.EndIndex == -1 && tag.StartIndex != indexInTmpeTagRemovedText) continue;
                if(tag.EndIndex != -1 && (indexInTmpeTagRemovedText < tag.StartIndex || tag.EndIndex < indexInTmpeTagRemovedText)) continue;

                foreach(TMPE_TypingEventEffect typingEventEffect in _typingEventEffects) {
                    if(timing == typingEventEffect.Timing && typingEventEffect.TagName == tag.Name) {
                        typingEventEffect.OnEventTriggerd(tag, effector, textInfo, characterInfo, typeIndex);
                    }
                }
            }
        }

        public bool IsValidBasicTag(TMPE_Tag tag) {
            foreach(TMPE_BasicEffect basicEffect in _basicEffects) {
                if(basicEffect.TagName == tag.Name && basicEffect.ValidateTag(tag)) {
                    return true;
                }
            }
            return false;
        }

        public bool IsValidTypingTag(TMPE_Tag tag) {
            foreach(TMPE_TypingEffect typingEffect in _typingEffects) {
                if(typingEffect.TagName == tag.Name && typingEffect.ValidateTag(tag)) {
                    return true;
                }
            }
            return false;
        }

        public bool IsValidTypingEventTag(TMPE_Tag tag) {
            foreach(TMPE_TypingEventEffect typingEventEffect in _typingEventEffects) {
                if(typingEventEffect.TagName == tag.Name && typingEventEffect.ValidateTag(tag)) {
                    return true;
                }
            }
            return false;
        }
    }
}