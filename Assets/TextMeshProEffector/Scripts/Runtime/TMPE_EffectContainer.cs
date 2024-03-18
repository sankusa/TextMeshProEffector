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

        public void UpdateTextInfo_BasicEffect(TMPE_EffectorBase effector) {
            List<TMPE_Tag> tags = effector.TagContainer.BasicTags;
            foreach(TMPE_BasicEffect basicEffect in _basicEffects) {
                if(string.IsNullOrEmpty(basicEffect.TagName)) {
                    basicEffect.UpdateVertex(null, effector);
                }
            }

            foreach(TMPE_Tag tag in tags) {
                foreach(TMPE_BasicEffect basicEffect in _basicEffects) {
                    if(basicEffect.TagName == tag.Name) {
                        basicEffect.UpdateVertex(tag, effector);
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
    }
}