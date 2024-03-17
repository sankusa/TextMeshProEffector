using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEventEffects {
    public class PauseTyping : TMPE_TypingEventEffect {
        public override TriggerTiming Timing => TriggerTiming.BeforeTyping;

        [SerializeField] private float _pauseTime;
        public override void OnEventTriggerd(TMPE_Tag tag, IEffector effector, TMPE_TypeWriterBase typeWriter, TMP_CharacterInfo characterInfo, int characterInfoIndex) {
            typeWriter.Pause(effector, _pauseTime);
        }
    }
}