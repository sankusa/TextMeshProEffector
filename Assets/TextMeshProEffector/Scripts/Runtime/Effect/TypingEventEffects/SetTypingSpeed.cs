using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypingEventEffects {
    public class SetTypingSpeed : TMPE_TypingEventEffect {
        [SerializeField, Min(0)] private float _speed;

        public override bool ValidateTagWithoutName(TMPE_Tag tag) {
            if(tag.Value != null && float.TryParse(tag.Value, out float _) == false) return false;
            if(tag.Attributes.Count != 0) return false;
            return true;
        }

        public override void OnEventTriggerd(TMPE_Tag tag, TMPE_TypeWriter typeWriter, int characterInfoIndex) {
            float speed = _speed;
            if(tag != null && string.IsNullOrEmpty(tag.Value) == false) {
                speed = float.Parse(tag.Value);
            }
            typeWriter.TypingSpeed = speed;
        }
    }
}