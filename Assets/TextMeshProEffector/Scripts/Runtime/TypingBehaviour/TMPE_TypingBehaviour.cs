using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    public abstract class TMPE_TypingBehaviour<Status, CharStatus> : TMPE_TypingBehaviourBase where Status : TMPE_TypingBehaviourStatus<CharStatus>, new() where CharStatus : CharacterTypingStatus, new() {
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

        public bool TryType(TMPE_TypeWriter typeWriter, int characterInfoIndex, bool force = false, bool ignoreTypingEvent = false) {
            Status status = _statusDic[typeWriter];

            if(force == false) {
                if(status.IsPaused || status.DelayTimer > 0) return false;
            }

            if(status.CharacterTypingStatuses[characterInfoIndex].State == CharacterTypingState.Typed) return true;

            if(status.CharacterTypingStatuses[characterInfoIndex].BeforeTypingEventInvoked == false) {
                if(ignoreTypingEvent == false) {
                    for(int i = 0; i < typeWriter.TypingEventEffectContainers.Count; i++) {
                        TMPE_TypingEventEffectContainer typingEventEffectContainer = typeWriter.TypingEventEffectContainers[i];
                        if(typingEventEffectContainer == null) continue;
                        typingEventEffectContainer.ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.BeforeTyping, typeWriter, characterInfoIndex);
                    }
                }
                status.CharacterTypingStatuses[characterInfoIndex].BeforeTypingEventInvoked = true;
            }

            if(force == false) {
                if(status.IsPaused || status.DelayTimer > 0) return false;
            }
            
            status.CharacterTypingStatuses[characterInfoIndex].State = CharacterTypingState.Typed;
            typeWriter.OnCharacterTyped?.Invoke(characterInfoIndex);
            if(ignoreTypingEvent == false) {
                for(int i = 0; i < typeWriter.TypingEventEffectContainers.Count; i++) {
                    TMPE_TypingEventEffectContainer typingEventEffectContainer = typeWriter.TypingEventEffectContainers[i];
                    if(typingEventEffectContainer == null) continue;
                    typingEventEffectContainer.ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.AfterTyping, typeWriter, characterInfoIndex);
                }
            }

            ChangeCharacterVisiblityIfNeed(typeWriter, characterInfoIndex);
            
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

        public override bool IsPausedTyping(TMPE_TypeWriter typeWriter) {
            return _statusDic[typeWriter].IsPaused;
        }

        public override void SetTypeWriterSpeed(TMPE_TypeWriter typeWriter, float speed) {
            _statusDic[typeWriter].Speed = speed;
        }

        public override void SetTypingSpeed(TMPE_TypeWriter typeWriter, float speed) {
            _statusDic[typeWriter].TypingSpeed = speed;
        }

        public override void SetTypingEffectSpeed(TMPE_TypeWriter typeWriter, float speed) {
            _statusDic[typeWriter].TypingEffectSpeed = speed;
        }

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
}
