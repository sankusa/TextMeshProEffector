using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEventEffects {
    [CreateAssetMenu(fileName = nameof(PauseTyping), menuName = nameof(TextMeshProEffector) + "/" + nameof(TypingEventEffects) + "/" + nameof(PauseTyping))]
    public class PauseTyping : TMPE_TypingEventEffect {
        public override TriggerTiming Timing => TriggerTiming.BeforeTyping;

        [SerializeField] private float _pauseTime;
        public override void OnEventTriggerd(TMPE_Tag tag, TMPE_EffectorBase effector, TMPE_TypeWriterBase typeWriter, int characterInfoIndex) {
            typeWriter.DelayTyping(effector, _pauseTime);
        }
    }
}