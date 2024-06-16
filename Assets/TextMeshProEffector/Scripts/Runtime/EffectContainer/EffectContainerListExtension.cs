using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    public static class EffectContainerListExtension {
        public static bool ValidateTag<T>(this IReadOnlyList<T> containers, TMPE_Tag tag) where T : TMPE_EffectContainerBase {
            bool isValidTypingTag = false;
            for(int i = 0; i < containers.Count; i++) {
                TMPE_EffectContainerBase typingEffectContainer = containers[i];
                if(typingEffectContainer == null) continue;
                isValidTypingTag |= typingEffectContainer.ValidateTag(tag);
            }
            return isValidTypingTag;
        }
    }
}