using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEventEffects {
    public class Log : TMPE_TypingEventEffect {
        public override void OnEventTriggerd(TMPE_Tag tag, IEffector effector, TMP_TextInfo textInfo, TMP_CharacterInfo characterInfo, int characterInfoIndex) {
            Debug.Log(characterInfo.character);
        }
    }
}