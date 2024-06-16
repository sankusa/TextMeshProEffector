using System;

namespace TextMeshProEffector {
    public class TMPE_CharacterTypingStatus {
        private enum TypingState {Untyped, Typed}

        private TypingState _state;
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
            _state = TypingState.Untyped;
            ElapsedTimeForTypingEffect = 0;
            BeforeTypingEventInvoked = false;
        }

        public void DoType() {
            _state = TypingState.Typed;
        }
        public bool IsTyped() => _state == TypingState.Typed;
    }
}