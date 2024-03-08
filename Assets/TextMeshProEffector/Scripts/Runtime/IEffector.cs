using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    public interface IEffector {
        public TMP_Text TextComponent {get;}
        public float ElapsedTimeFromTextChanged {get;}
        public float[] ElapsedTimesFromTyped {get;}
        public float TypingPauseTimer {get; set;}
    }
}