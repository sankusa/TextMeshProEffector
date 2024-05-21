using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEventEffects {
    public class PauseTyping : TMPE_TypingEventEffect {
        public override TriggerTiming Timing => TriggerTiming.BeforeTyping;

        [SerializeField] private float _pauseTime;
        public override void OnEventTriggerd(TMPE_Tag tag, TMPE_TypeWriter typeWriter, TMPE_TypingBehaviourBase typingBehaviour, int characterInfoIndex) {
            typingBehaviour.DelayTyping(typeWriter, _pauseTime);
        }
    }
}