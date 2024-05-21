using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEventEffects {
    public class PauseIfTargetCharacter : TMPE_TypingEventEffect {
        [SerializeField] private string _targetCharacters;
        [SerializeField] private float _seconds;
        public override void OnEventTriggerd(TMPE_Tag tag, TMPE_TypeWriter typeWriter, TMPE_TypingBehaviourBase typingBehaviour, int characterInfoIndex) {
            TMP_CharacterInfo characterInfo = typeWriter.Effector.TextInfo.characterInfo[characterInfoIndex];
            if(_targetCharacters.IndexOf(characterInfo.character) != -1) {
                typingBehaviour.DelayTyping(typeWriter, _seconds);
            }
        }
    }
}