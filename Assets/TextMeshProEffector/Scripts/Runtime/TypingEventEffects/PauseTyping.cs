using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEventEffects {
    public class PauseTyping : TMPE_TypingEventEffect {
        public override TriggerTiming Timing => TriggerTiming.BeforeTyping;

        [SerializeField] private float _pauseTime;
        public override void OnEventTriggerd(TMPE_Tag tag, IEffector effector, TMP_TextInfo textInfo, TMP_CharacterInfo characterInfo, int characterInfoIndex) {
            effector.TypingPauseTimer = _pauseTime;
        }
    }
}