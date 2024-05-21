using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [Serializable]
    public abstract class TMPE_TypingEventEffect : TMPE_EffectBase {
        public enum TriggerTiming {
            BeforeTyping,
            AfterTyping,
        }

        public virtual TriggerTiming Timing => TriggerTiming.AfterTyping;
        public virtual void OnEventTriggerd(TMPE_Tag tag, TMPE_TypeWriter typeWriter, TMPE_TypingBehaviourBase typingBehaviour, int characterInfoIndex) {}

        public override string GetCaption() => string.IsNullOrEmpty(_tagName) ? "" : $"<!{_tagName}>";
    }
}