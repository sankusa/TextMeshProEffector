using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEventEffects {
    public class PauseIfTargetCharacter : TMPE_TypingEventEffect {
        [SerializeField] private string _targetCharacters;
        [SerializeField] private float _seconds;
        public override void OnEventTriggerd(TMPE_Tag tag, IEffector effector, TMPE_TypeWriterBase typeWriter, TMP_CharacterInfo characterInfo, int characterInfoIndex) {
            if(_targetCharacters.IndexOf(characterInfo.character) != -1) {
                typeWriter.Pause(effector, _seconds);
            }
        }
    }
}