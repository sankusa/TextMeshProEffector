using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TMPE_BasicEffectContainer), fileName = nameof(TMPE_BasicEffectContainer))]
    public class TMPE_BasicEffectContainer : TMPE_EffectContainerBase<TMPE_BasicEffect> {
        public void UpdateVertex(TMPE_EffectorBase effector) {
            // タグ無し
            foreach(TMPE_BasicEffect basicEffect in _effects) {
                if(basicEffect == null) continue;
                if(basicEffect.HasTag() == false) {
                    basicEffect.UpdateVertex(null, effector);
                }
            }

            // タグ有り
            IReadOnlyList<TMPE_Tag> tags = effector.BasicTagContainer.Tags;
            for(int i = 0; i < tags.Count; i++) {
                TMPE_Tag tag = tags[i];
                foreach(TMPE_BasicEffect basicEffect in _effects) {
                    if(basicEffect == null) continue;
                    if(basicEffect.TagName == tag.Name) {
                        basicEffect.UpdateVertex(tag, effector);
                    }
                }
            }
        }
    }
}