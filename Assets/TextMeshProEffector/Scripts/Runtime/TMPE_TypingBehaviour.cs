using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    public enum CharacterTypingState {Idle, Typed}
    public class CharacterTypingStatus {
        public CharacterTypingState State {get; set;}
        public float ElapsedTimeForTypingEffect {get; set;}
        public bool BeforeTypingEventInvoked {get; set;}

        public virtual void Reset() {
            State = CharacterTypingState.Idle;
            ElapsedTimeForTypingEffect = 0;
            BeforeTypingEventInvoked = false;
        }
    }

    public class TypeWriterStatus<CharStatus> where CharStatus : CharacterTypingStatus, new() {
        public bool TypingStarted {get; set;}
        public float ElapsedTimeForTyping {get; set;}
        public bool IsPaused {get; set;}
        public float DelayTimer {get; set;}
        public float Speed {get; set;} = 1;
        public float TypingSpeed {get; set;} = 1;
        public float TypingEffectSpeed {get; set;} = 1;
        private CharStatus[] _typingStatuses;
        public CharStatus[] CharacterTypingStatuses => _typingStatuses;

        public virtual void Reset(TMPE_TypeWriter typeWriter) {
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

    public abstract class TMPE_TypingBehaviour<Status, CharStatus> : TMPE_TypingBehaviourBase where Status : TypeWriterStatus<CharStatus>, new() where CharStatus : CharacterTypingStatus, new() {
        protected Dictionary<TMPE_TypeWriter, Status> _statusDic = new Dictionary<TMPE_TypeWriter, Status>();

        public override bool IsFinishedTyping(TMPE_TypeWriter typeWriter) {
            Status status = _statusDic[typeWriter];
            int count = 0;
            for(int i = 0; i < typeWriter.Effector.TextInfo.characterCount; i++) {
                if(status.CharacterTypingStatuses[i].State == CharacterTypingState.Idle) count++;
            }
            return count == 0;
        }

        public override void StartTyping(TMPE_TypeWriter typeWriter) {
            Status status = _statusDic[typeWriter];
            status.TypingStarted = true;
        }

        public override bool IsStartedTyping(TMPE_TypeWriter typeWriter) {
            Status status = _statusDic[typeWriter];
            return status.TypingStarted;
        }

        public override void OnAttach(TMPE_TypeWriter typeWriter) {
            Status status = new Status();
            status.Reset(typeWriter); 
            _statusDic[typeWriter] = status;
        }

        public override void OnDetach(TMPE_TypeWriter typeWriter) {
            _statusDic.Remove(typeWriter);
        }

        public override void OnTextChanged(TMPE_TypeWriter typeWriter) {
            _statusDic[typeWriter].Reset(typeWriter);
        }

        public sealed override void UpdateTyping(TMPE_TypeWriter typeWriter) {
            Status status = _statusDic[typeWriter];

            if(status.TypingStarted == false) return;

            if(status.IsPaused) return;
            status.DelayTimer = Mathf.Max(status.DelayTimer - Time.deltaTime, 0);
            if(status.DelayTimer > 0) return;

            status.ElapsedTimeForTyping += Time.deltaTime * status.Speed * status.TypingSpeed;

            UpdateTypingMain(typeWriter);
        }

        protected abstract void UpdateTypingMain(TMPE_TypeWriter typeWriter);

        public bool TryType(TMPE_EffectorBase effector, TMPE_TypeWriter typeWriter, int characterInfoIndex, bool force = false, bool ignoreTypingEvent = false) {
            Status status = _statusDic[typeWriter];

            if(force == false) {
                if(status.IsPaused || status.DelayTimer > 0) return false;
            }

            if(status.CharacterTypingStatuses[characterInfoIndex].State == CharacterTypingState.Typed) return true;

            int typeWriterIndex = effector.GetTypeWriterIndex(this);
            if(status.CharacterTypingStatuses[characterInfoIndex].BeforeTypingEventInvoked == false) {
                if(ignoreTypingEvent == false) {
                    foreach(TMPE_TypingEventEffectContainer typingEventEffectContainer in typeWriter.TypingEventEffectContainers) {
                        if(typingEventEffectContainer == null) continue;
                        typingEventEffectContainer.ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.BeforeTyping, typeWriter, typeWriterIndex, characterInfoIndex);
                    }
                }
                status.CharacterTypingStatuses[characterInfoIndex].BeforeTypingEventInvoked = true;
            }

            if(force == false) {
                if(status.IsPaused || status.DelayTimer > 0) return false;
            }
            
            status.CharacterTypingStatuses[characterInfoIndex].State = CharacterTypingState.Typed;
            effector.TypeWriters[effector.GetTypeWriterIndex(this)].OnCharacterTyped?.Invoke(characterInfoIndex);
            if(ignoreTypingEvent == false) {
                foreach(TMPE_TypingEventEffectContainer typingEventEffectContainer in typeWriter.TypingEventEffectContainers) {
                    if(typingEventEffectContainer == null) continue;
                    typingEventEffectContainer.ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.AfterTyping, typeWriter, typeWriterIndex, characterInfoIndex);
                }
            }
            ChangeCharacterVisiblityIfNeed(effector, characterInfoIndex);
            
            return true;
        }

        public override void Tick(TMPE_TypeWriter typeWriter) {
            TMP_TextInfo textInfo = typeWriter.Effector.TextInfo;
            Status status = _statusDic[typeWriter];
            for(int i = 0; i < textInfo.characterCount; i++) {
                if(status.CharacterTypingStatuses[i].State == CharacterTypingState.Typed) {
                    status.CharacterTypingStatuses[i].ElapsedTimeForTypingEffect += Time.deltaTime * status.Speed * status.TypingEffectSpeed;
                }
            }
        }

        public override void PauseTyping(TMPE_TypeWriter typeWriter) {
            _statusDic[typeWriter].IsPaused = true;
        }

        public override void DelayTyping(TMPE_TypeWriter typeWriter, float seconds) {
            _statusDic[typeWriter].DelayTimer = seconds;
        }

        public override void ResumeTyping(TMPE_TypeWriter typeWriter) {
            _statusDic[typeWriter].IsPaused = false;
        }

        public override bool IsPausedTyping(TMPE_TypeWriter typeWriter) => _statusDic[typeWriter].IsPaused;

        public override void SetTypeWriterSpeed(TMPE_TypeWriter typeWriter, float speed) => _statusDic[typeWriter].Speed = speed;
        public override void SetTypingSpeed(TMPE_TypeWriter typeWriter, float speed) => _statusDic[typeWriter].TypingSpeed = speed;
        public override void SetTypingEffectSpeed(TMPE_TypeWriter typeWriter, float speed) => _statusDic[typeWriter].TypingEffectSpeed = speed;

        public override float GetElapsedTimeForTyping(TMPE_TypeWriter typeWriter) {
            return _statusDic[typeWriter].ElapsedTimeForTyping;
        }

        public override float GetElapsedTimeForTypingEffect(TMPE_TypeWriter typeWriter, int characterInfoIndex) {
            return _statusDic[typeWriter].CharacterTypingStatuses[characterInfoIndex].ElapsedTimeForTypingEffect;
        }

        public override bool IsTypedCharacter(TMPE_TypeWriter typeWriter, int characterInfoIndex) {
            return _statusDic[typeWriter].CharacterTypingStatuses[characterInfoIndex].State == CharacterTypingState.Typed;
        }
    }

    public abstract class TMPE_TypingBehaviourBase : ScriptableObject {
        public abstract bool IsFinishedTyping(TMPE_TypeWriter typeWriter);

        public virtual void Tick(TMPE_TypeWriter typeWriter) {}

        protected void ChangeCharacterVisiblityIfNeed(TMPE_EffectorBase effector, int characterinfoIndex) {
            TMPE_TypeWriter typeWriter = effector.TypeWriters[effector.GetTypeWriterIndex(this)];
            if(typeWriter.VisualizeCharacters == CharacterVisualizationType.ToVisible) {
                effector.TypingInfo[characterinfoIndex].Visiblity = CharacterVisiblity.Visible;
            }
            else if(typeWriter.VisualizeCharacters == CharacterVisualizationType.ToInvisible) {
                effector.TypingInfo[characterinfoIndex].Visiblity = CharacterVisiblity.Invisible;
            }
        }

        public abstract void StartTyping(TMPE_TypeWriter typeWriter);
        public abstract bool IsStartedTyping(TMPE_TypeWriter typeWriter);
        public virtual void OnAttach(TMPE_TypeWriter typeWriter) {}
        public virtual void OnDetach(TMPE_TypeWriter typeWriter) {}
        public virtual void OnTextChanged(TMPE_TypeWriter typeWriter) {}
        public virtual void UpdateTyping(TMPE_TypeWriter typeWriter) {}
        public virtual void PauseTyping(TMPE_TypeWriter typeWriter) => Debug.LogWarning(nameof(PauseTyping) + " is not implemented");
        public virtual void DelayTyping(TMPE_TypeWriter typeWriter, float seconds) => Debug.LogWarning(nameof(PauseTyping) + " is not implemented");
        public virtual void ResumeTyping(TMPE_TypeWriter typeWriter) => Debug.LogWarning(nameof(ResumeTyping) + " is not implemented");
        public virtual bool IsPausedTyping(TMPE_TypeWriter typeWriter) => false;
        public virtual void SetTypeWriterSpeed(TMPE_TypeWriter typeWriter, float speed) => Debug.LogWarning(nameof(SetTypeWriterSpeed) + " is not implemented");
        public virtual void SetTypingSpeed(TMPE_TypeWriter typeWriter, float speed) => Debug.LogWarning(nameof(SetTypingSpeed) + " is not implemented");
        public virtual void SetTypingEffectSpeed(TMPE_TypeWriter typeWriter, float speed) => Debug.LogWarning(nameof(SetTypingEffectSpeed) + " is not implemented");
        public virtual float GetElapsedTimeForTyping(TMPE_TypeWriter typeWriter) {
            Debug.LogWarning(nameof(GetElapsedTimeForTyping) + " is not implemented");
            return 0;
        }
        public virtual float GetElapsedTimeForTypingEffect(TMPE_TypeWriter typeWriter, int characterInfoIndex) {
            Debug.LogWarning(nameof(GetElapsedTimeForTypingEffect) + " is not implemented");
            return 0;
        }
        public abstract bool IsTypedCharacter(TMPE_TypeWriter typeWriter, int characterInfoIndex);

        // TypeWriterControlTag
        private static string[] _emptyTagNames = new string[0];
        public virtual string[] AcceptableTagNames => _emptyTagNames;
        public virtual bool IsValidTypeWriterControlTag(TMPE_Tag tag) => false;
    }
}
