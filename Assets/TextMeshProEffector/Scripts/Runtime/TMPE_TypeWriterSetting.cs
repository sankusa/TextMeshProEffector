using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    public enum AutoStartTypingType {
        None,
        Auto,
        OnOtherTypeWriterStartedTyping,
    }

    [Serializable]
    public class TMPE_TypeWriterSetting {
        [SerializeField] private TMPE_TypeWriterBase _typeWriter;
        public TMPE_TypeWriterBase TypeWriter => _typeWriter;

        [SerializeField] private AutoStartTypingType _startTypingAuto = AutoStartTypingType.Auto;
        public AutoStartTypingType StartTypingAuto => _startTypingAuto;

#region OnOtherTypeWriterStartedTyping
        [SerializeField, Min(0)] private int _targetTypeWriterIndex;
        public int TargetTypeWriterIndex => _targetTypeWriterIndex;

        [SerializeField, Min(0)] private float _delayFromTargetTypeWriterStartedTyping;
        public float DelayFromTargetTypeWriterStartedTyping => _delayFromTargetTypeWriterStartedTyping;
#endregion

        [SerializeField] private bool _repeat;
        public bool Repeat => _repeat;

        [SerializeField, Min(0)] private float _repeatInterval;
        public float RepeatInterval => _repeatInterval;
    }
}