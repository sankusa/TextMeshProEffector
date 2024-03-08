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
        public virtual void OnEventTriggerd(TMPE_Tag tag, IEffector effector, TMP_TextInfo textInfo, TMP_CharacterInfo characterInfo, int characterInfoIndex) {}
    }
}