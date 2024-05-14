using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TMPE_BasicEffectContainer), fileName = nameof(TMPE_BasicEffectContainer))]
    public class TMPE_BasicEffectContainer : ScriptableObject {
        [SerializeReference] private List<TMPE_BasicEffect> _basicEffects;
        public List<TMPE_BasicEffect> BasicEffects => _basicEffects;

        public void UpdateVertex(TMPE_EffectorBase effector) {
            List<TMPE_Tag> tags = effector.TagContainer.BasicTags;
            foreach(TMPE_BasicEffect basicEffect in _basicEffects) {
                if(basicEffect == null) continue;
                if(string.IsNullOrEmpty(basicEffect.TagName)) {
                    basicEffect.UpdateVertex(null, effector);
                }
            }

            foreach(TMPE_Tag tag in tags) {
                foreach(TMPE_BasicEffect basicEffect in _basicEffects) {
                    if(basicEffect == null) continue;
                    if(basicEffect.TagName == tag.Name) {
                        basicEffect.UpdateVertex(tag, effector);
                    }
                }
            }
        }

        public bool IsValidBasicTag(TMPE_Tag tag) {
            foreach(TMPE_BasicEffect basicEffect in _basicEffects) {
                if(basicEffect == null) continue;
                if(basicEffect.TagName == tag.Name && basicEffect.ValidateTag(tag)) {
                    return true;
                }
            }
            return false;
        }
    }
}