using System;
using UnityEngine;

namespace TextMeshProEffector {
    public enum CharacterTypingState {Idle, Typed}
    public class CharacterTypingStatus {
        public CharacterTypingState State {get; set;}
        private float _elapsedTimeForTypingEffect;
        public float ElapsedTimeForTypingEffect {
            get => _elapsedTimeForTypingEffect;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _elapsedTimeForTypingEffect = value;
            }
        }
        public bool BeforeTypingEventInvoked {get; set;}

        public virtual void Reset() {
            State = CharacterTypingState.Idle;
            ElapsedTimeForTypingEffect = 0;
            BeforeTypingEventInvoked = false;
        }
    }

    public class TMPE_TypingBehaviourStatus<CharStatus> where CharStatus : CharacterTypingStatus, new() {
        public bool TypingStarted {get; set;}
        private float _elapsedTimeForTyping;
        public float ElapsedTimeForTyping {
            get => _elapsedTimeForTyping;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _elapsedTimeForTyping = value;
            }
        }
        public bool IsPaused {get; set;}
        private float _delayTimer;
        public float DelayTimer {
            get => _delayTimer;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _delayTimer = value;
            }
        }
        private float _speed = 1;
        public float Speed {
            get => _speed;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _speed = value;
            }
        }
        private float _typingSpeed = 1;
        public float TypingSpeed {
            get => _typingSpeed;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _typingSpeed = value;
            }
        }
        private float _typingEffectSpeed = 1;
        public float TypingEffectSpeed {
            get => _typingEffectSpeed;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _typingEffectSpeed = value;
            }
        }
        private CharStatus[] _typingStatuses;
        public CharStatus[] CharacterTypingStatuses => _typingStatuses;

        public virtual void Reset(TMPE_TypeWriter typeWriter) {
            if(typeWriter == null) throw new ArgumentNullException(nameof(typeWriter));
            
            TMPE_EffectorBase effector = typeWriter.Effector;
            TypingStarted = false;
            ElapsedTimeForTyping = 0;
            IsPaused = false;
            DelayTimer = 0;
            Speed = 1;
            TypingSpeed = 1;
            TypingEffectSpeed = 1;
            if(CharacterTypingStatuses == null) {
                _typingStatuses = new CharStatus[Mathf.NextPowerOfTwo(effector.TextInfo.characterCount)];
                for(int i = 0; i < _typingStatuses.Length; i++) {
                    _typingStatuses[i] = new CharStatus();
                }
            }
            else if(CharacterTypingStatuses.Length < effector.TextInfo.characterCount) {
                Array.Resize(ref _typingStatuses, Mathf.NextPowerOfTwo(effector.TextInfo.characterCount));
                for(int i = 0; i < _typingStatuses.Length; i++) {
                    if(_typingStatuses[i] == null) {
                        _typingStatuses[i] = new CharStatus();
                    }
                }
            }

            foreach(CharacterTypingStatus status in _typingStatuses) {
                status.Reset();
            }
        }
    }
}