using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    public abstract class TMPE_EffectContainerBase : ScriptableObject {
        public abstract void AddEffect(object effect);
        public abstract string FindTag(char[] charArrayContainsTag, int tagNameStartIndex, int tagNameEndIndex);
        public abstract bool ValidateTag(TMPE_Tag tag);
    }

    public abstract class TMPE_EffectContainerBase<T> : TMPE_EffectContainerBase where T : TMPE_EffectBase {
        [SerializeReference] protected List<T> _effects;
        public IReadOnlyList<T> Effects => _effects;

        public sealed override void AddEffect(object effect){
            AddEffect((T)effect);
        }

        public void AddEffect(T effect) {
            _effects.Add(effect);
        }

        public override bool ValidateTag(TMPE_Tag tag) {
            foreach(T effect in _effects) {
                if(effect == null) continue;
                if(effect.ValidateTag(tag)) {
                    return true;
                }
            }
            return false;
        }

        public override string FindTag(char[] charArrayContainsTag, int tagNameStartIndex, int tagNameEndIndex) {
            foreach(T effect in _effects) {
                if(effect.TagName.EqualsPartialCharArray(charArrayContainsTag, tagNameStartIndex, tagNameEndIndex)) {
                    return effect.TagName;
                }
            }
            return null;
        }
    }
}