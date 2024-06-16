using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TextMeshProEffector {
    public enum AutoStartTypingType {
        None = 0,
        Auto = 1,
        OnOtherTypeWriterStartedTyping = 2,
    }
    
    [RequireComponent(typeof(TMPE_EffectorBase))]
    public class TMPE_TypeWriter : MonoBehaviour {
        [SerializeField] private TMPE_EffectorBase _effector;
        public TMPE_EffectorBase Effector => _effector;

        [SerializeField] private TMPE_TypingBehaviourBase _typingBehaviour;
        public TMPE_TypingBehaviourBase TypingBehaviour => _typingBehaviour;

        [SerializeField] private List<TMPE_TypingEffectContainer> _typingEffectContainers;
        public IReadOnlyList<TMPE_TypingEffectContainer> TypingEffectContainers => _typingEffectContainers;

        [SerializeField] private List<TMPE_TypingEventEffectContainer> _typingEventEffectContainers;
        public IReadOnlyList<TMPE_TypingEventEffectContainer> TypingEventEffectContainers => _typingEventEffectContainers;

        [SerializeField] private bool _setToTyped = true;
        public bool SetToTyped => _setToTyped;

        [SerializeField] private AutoStartTypingType _startTypingAuto = AutoStartTypingType.Auto;
        public AutoStartTypingType StartTypingAuto => _startTypingAuto;

#region OnOtherTypeWriterStartedTyping
        [Serializable]
        public class OtherTypewriterWaitSettingData {
            [SerializeField, Min(0)] private int _targetTypeWriterIndex;
            public int TargetTypeWriterIndex {
                get => _targetTypeWriterIndex;
                set => _targetTypeWriterIndex = Mathf.Max(value, 0);
            }

            [SerializeField, Min(0)] private float _delay;
            public float Delay {
                get => _delay;
                set => _delay = Mathf.Max(value, 0);
            }
        }

        [SerializeField] private OtherTypewriterWaitSettingData _otherTypewriterWaitSetting;
        public OtherTypewriterWaitSettingData OtherTypewriterWaitSetting => _otherTypewriterWaitSetting;
#endregion

        [SerializeField] private bool _repeat;
        public bool Repeat {
            get => _repeat;
            set => _repeat = value;
        }

        [SerializeField, Min(0)] private float _repeatInterval;
        public float RepeatInterval {
            get => _repeatInterval;
            set => _repeatInterval = Mathf.Max(value, 0);
        }

#region Event
        [SerializeField] private UnityEvent<int> _onCharacterTyped;
        public UnityEvent<int> OnCharacterTyped => _onCharacterTyped;

        [SerializeField] private OneShotUnityEvent _onTypingCompleted;
        public UnityEvent OnTypingCompleted => _onTypingCompleted.UnityEvent;
#endregion

        private bool _isPlayingTypingEffect;
        public bool IsPlayingTypingEffect => _isPlayingTypingEffect;

        // タグ
        private readonly TMPE_TagContainer _typingTagContainer = new TMPE_TagContainer();
        internal TMPE_TagContainer TypingTagContainer => _typingTagContainer;
        private readonly TMPE_TagContainer _typingEventTagContainer = new TMPE_TagContainer();
        internal TMPE_TagContainer TypingEventTagContainer => _typingEventTagContainer;
        private readonly TMPE_TagContainer _typeWriterControlTagContainer = new TMPE_TagContainer();
        internal TMPE_TagContainer TypeWriterControlTagContainer => _typeWriterControlTagContainer;

        // 状態
        private TMPE_CharacterTypingStatus[] _characterTypingStatuses;
        public TMPE_CharacterTypingStatus[] CharacterTypingStatuses => _characterTypingStatuses;

        protected float _typingElapsedTime;
        public float TypingElapsedTime {
            get => _typingElapsedTime;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _typingElapsedTime = value;
            }
        }

        private bool _hasStartedTyping;
        protected float _speed = 1;
        public float Speed {
            get => _speed;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _speed = value;
            }
        }

        protected float _typingSpeed = 1;
        public float TypingSpeed {
            get => _typingSpeed;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _typingSpeed = value;
            }
        }

        protected float _typingEffectSpeed = 1;
        public float TypingEffectSpeed {
            get => _typingEffectSpeed;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _typingEffectSpeed = value;
            }
        }

        private bool _isPausedTyping;
        public bool IsPausedTyping {
            get => _isPausedTyping;
            set => _isPausedTyping = value;
        }

        protected float _typingDelay;
        public float TypingDelay {
            get => _typingDelay;
            set {
                if(value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _typingDelay = value;
            }
        }

        private TMPE_TypingBehaviourStatus _typingBehaviourStatus;

        // Unityイベント関数
        void Reset() {
            _effector = GetComponent<TMPE_EffectorBase>();
        }

        internal TMPE_TypingBehaviourStatus GetTypingBehaviourStatus() {
            MaintainTypingBehaviourStatus();
            return _typingBehaviourStatus;
        }

        private void MaintainTypingBehaviourStatus() {
            if(_typingBehaviour == null) {
                if((_typingBehaviourStatus is TMPE_TypingBehaviourStatus) == false) {
                    _typingBehaviourStatus = new TMPE_TypingBehaviourStatus();
                    _typingBehaviourStatus.Reset(this);
                }
            }
            else {
                _typingBehaviour.MaintainStatus(this, ref _typingBehaviourStatus);
            }
        }

        internal void StartAutoTypingIfNeed() {
            if(_hasStartedTyping == false) {
                if(_startTypingAuto == AutoStartTypingType.Auto) {
                    Play();
                }
                else if(_startTypingAuto == AutoStartTypingType.OnOtherTypeWriterStartedTyping) {
                    TMPE_TypingBehaviourStatus targetStatus = _effector.TypeWriters[_otherTypewriterWaitSetting.TargetTypeWriterIndex].GetTypingBehaviourStatus();
                    if(targetStatus != null) {
                        if(TypingElapsedTime > _otherTypewriterWaitSetting.Delay) {
                            Play();
                        }
                    }
                }
            }
            else {
                if(_repeat && _repeatInterval > 0 && TypingElapsedTime > _repeatInterval) {
                    Play(false);
                }
            }
        }

        internal void UpdateTyping() {
            MaintainTypingBehaviourStatus();

            if(_hasStartedTyping == false) return;
            if(_isPausedTyping) return;

            _typingDelay = Mathf.Max(_typingDelay - Time.deltaTime, 0);
            if(_typingDelay > 0) return;

            TypingElapsedTime += Time.deltaTime * Speed * TypingSpeed;

            if(_typingBehaviour == null) {
                for(int i = 0; i < _effector.TextInfo.characterCount; i++) {
                    if(_characterTypingStatuses[i].IsTyped()) continue;
                    if(TryType(i) == false) break;
                }
            }
            else {
                _typingBehaviour?.UpdateTyping(this);
            }
        }

        internal void UpdateVertex() {
            TickTypingEffect(Speed * TypingEffectSpeed);

            _isPlayingTypingEffect = false;
            foreach(TMPE_TypingEffectContainer typingEffectContainer in  _typingEffectContainers) {
                if(typingEffectContainer == null) continue;
                _isPlayingTypingEffect |= typingEffectContainer.UpdateVertex(this);
            }
        }

        public bool TryType(int characterInfoIndex, bool force = false, bool ignoreTypingEvent = false) {
            if(force == false) {
                if(_isPausedTyping || _typingDelay > 0) return false;
            }

            TMPE_CharacterTypingStatus charStatus = _characterTypingStatuses[characterInfoIndex];

            if(charStatus.IsTyped()) return true;

            if(charStatus.BeforeTypingEventInvoked == false) {
                if(ignoreTypingEvent == false) {
                    for(int i = 0; i < TypingEventEffectContainers.Count; i++) {
                        TMPE_TypingEventEffectContainer typingEventEffectContainer = TypingEventEffectContainers[i];
                        if(typingEventEffectContainer == null) continue;
                        typingEventEffectContainer.ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.BeforeTyping, this, characterInfoIndex);
                    }
                }
                charStatus.BeforeTypingEventInvoked = true;
            }

            if(force == false) {
                if(_isPausedTyping || _typingDelay > 0) return false;
            }
            
            charStatus.DoType();
            _onCharacterTyped?.Invoke(characterInfoIndex);
            if(ignoreTypingEvent == false) {
                for(int i = 0; i < TypingEventEffectContainers.Count; i++) {
                    TMPE_TypingEventEffectContainer typingEventEffectContainer = TypingEventEffectContainers[i];
                    if(typingEventEffectContainer == null) continue;
                    typingEventEffectContainer.ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.AfterTyping, this, characterInfoIndex);
                }
            }

            // 可視化
            if(_setToTyped == true) {
                Effector.TypingInfo[characterInfoIndex].DoType();
            }
            
            return true;
        }

        internal void InvokeOnTypingCompletedIfNeed() {
            if(_onTypingCompleted.IsInvoked == false && IsFinished()) {
                _onTypingCompleted.Invoke();
            }
        }

        internal void ClearTag() {
            _typingTagContainer.Clear();
            _typingEventTagContainer.Clear();
            _typeWriterControlTagContainer.Clear();
        }

        public void ResetTypinRelatedValues(bool resetSpeed = true) {
            _onTypingCompleted.Reset();
            _isPlayingTypingEffect = false;

            TMPE_TypingBehaviourStatus status = GetTypingBehaviourStatus();
            status?.Reset(this);
            _typingBehaviour?.InitializeStatus(this, status);

            int characterCount = _effector.TextInfo.characterCount;
            if(_characterTypingStatuses == null) {
                _characterTypingStatuses = new TMPE_CharacterTypingStatus[Mathf.NextPowerOfTwo(characterCount)];
                for(int i = 0; i < _characterTypingStatuses.Length; i++) {
                    _characterTypingStatuses[i] = new TMPE_CharacterTypingStatus();
                }
            }
            else if(_characterTypingStatuses.Length < characterCount) {
                Array.Resize(ref _characterTypingStatuses, Mathf.NextPowerOfTwo(characterCount));
                for(int i = 0; i < _characterTypingStatuses.Length; i++) {
                    if(_characterTypingStatuses[i] == null) {
                        _characterTypingStatuses[i] = new TMPE_CharacterTypingStatus();
                    }
                }
            }
            foreach(TMPE_CharacterTypingStatus charStatus in _characterTypingStatuses) {
                charStatus.Reset();
            }

            _hasStartedTyping = false;

            TypingElapsedTime = 0;
            TypingDelay = 0;

            if(resetSpeed) {
                Speed = 1;
                TypingSpeed = 1;
                TypingEffectSpeed = 1;
            }
        }

        public virtual void TickTypingEffect(float speed) {
            for(int i = 0; i < _effector.TextInfo.characterCount; i++) {
                TMPE_CharacterTypingStatus charStatus = _characterTypingStatuses[i];
                if(charStatus.IsTyped()) {
                    charStatus.ElapsedTimeForTypingEffect += Time.deltaTime * speed;
                }
            }
        }

        public void Play(bool resetSpeed = true) {
            ResetTypinRelatedValues(resetSpeed);
            _hasStartedTyping = true;
        }

        public void PauseTyping() => IsPausedTyping = true;
        public void ResumeTyping() => IsPausedTyping = false;

        public bool IsStartedTyping() {
            return _hasStartedTyping;
        }
        public bool IsFinishedTyping() {
            int count = 0;
            for(int i = 0; i < Effector.TextInfo.characterCount; i++) {
                if(_characterTypingStatuses[i].IsTyped() == false) count++;
            }
            return count == 0;
        }
        public bool IsPlaying() => IsTyping() || IsPlayingTypingEffect;
        public bool IsFinished() => IsFinishedTyping() && IsPlayingTypingEffect == false;
        public bool IsTyping() {
            return _hasStartedTyping && IsFinishedTyping() == false;
        }
    }
}