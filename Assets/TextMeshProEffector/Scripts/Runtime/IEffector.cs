using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    public interface IEffector {
        public TMP_Text TextComponent {get;}
        public TMP_TextInfo TextInfo {get;}
        public TMPE_EffectContainer EffectContainer {get;}
        public TMPE_TagContainer TagContainer {get;}
        public float ElapsedTimeFromTextChanged {get;}
        public TMPE_TypingInfo[] TypingInfo {get;}
        public float TypingPauseTimer {get; set;}

        public int GetTypeWriterIndex(TMPE_TypeWriterBase typeWriter);
        public List<TMPE_TypeWriterSetting> TypeWriterSettings {get;}
    }
}