using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEventEffects {
    [CreateAssetMenu(fileName = nameof(PauseIfTargetCharacter), menuName = nameof(TextMeshProEffector) + "/" + nameof(TypingEventEffects) + "/" + nameof(PauseIfTargetCharacter))]
    public class PauseIfTargetCharacter : TMPE_TypingEventEffect {
        [SerializeField] private string _targetCharacters;
        [SerializeField] private float _seconds;
        public override void OnEventTriggerd(TMPE_Tag tag, TMPE_EffectorBase effector, TMPE_TypeWriterBase typeWriter, int characterInfoIndex) {
            TMP_CharacterInfo characterInfo = effector.TextInfo.characterInfo[characterInfoIndex];
            if(_targetCharacters.IndexOf(characterInfo.character) != -1) {
                typeWriter.DelayTyping(effector, _seconds);
            }
        }
    }
}