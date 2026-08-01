using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEventEffects {
    [System.Serializable]
    public class DelayTyping : TMPE_TypingEventEffect {
        public override TriggerTiming Timing => TriggerTiming.BeforeTyping;

        [SerializeField] private float _seconds;
        
        public override void OnEventTriggerd(TMPE_Tag tag, TMPE_TypeWriter typeWriter, int characterInfoIndex) {
            typeWriter.TypingDelay = _seconds;
        }
    }
}