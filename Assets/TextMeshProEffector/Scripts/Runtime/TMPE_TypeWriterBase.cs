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
        public float ElapsedTimeFromTyped {get; set;}
        public bool BeforeTypingEventInvoked {get; set;}

        public virtual void Reset() {
            State = CharacterTypingState.Idle;
            ElapsedTimeFromTyped = 0;
            BeforeTypingEventInvoked = false;
        }
    }

    public class TypeWriterStatus<CharStatus> where CharStatus : CharacterTypingStatus, new() {
        public bool TypingStarted {get; set;}
        public float ElapsedTime {get; set;}
        public bool IsPaused {get; set;}
        public float DelayTimer {get; set;}
        public float TypeSpeed {get; set;} = 1;
        public bool IsPlaying {get; set;}
        private CharStatus[] _typingStatuses;
        public CharStatus[] TypingStatuses => _typingStatuses;

        public virtual void Reset(TMPE_EffectorBase effector) {
            TypingStarted = false;
            ElapsedTime = 0;
            IsPaused = false;
            DelayTimer = 0;
            TypeSpeed = 1;
            if(TypingStatuses == null) {
                _typingStatuses = new CharStatus[Mathf.NextPowerOfTwo(effector.TextInfo.characterCount)];
                for(int i = 0; i < _typingStatuses.Length; i++) {
                    _typingStatuses[i] = new CharStatus();
                }
            }
            else if(TypingStatuses.Length < effector.TextInfo.characterCount) {
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

    public abstract class TMPE_TypeWriterGeneric<Status, CharStatus> : TMPE_TypeWriterBase where Status : TypeWriterStatus<CharStatus>, new() where CharStatus : CharacterTypingStatus, new() {
        protected Dictionary<TMPE_EffectorBase, Status> _statusDic = new Dictionary<TMPE_EffectorBase, Status>();

        public override bool IsFinishedTyping(TMPE_EffectorBase effector) {
            Status status = _statusDic[effector];
            int count = 0;
            for(int i = 0; i < effector.TextInfo.characterCount; i++) {
                if(status.TypingStatuses[i].State == CharacterTypingState.Idle) count++;
            }
            return count == 0;
        }

        public override void StartTyping(TMPE_EffectorBase effector) {
            Status status = _statusDic[effector];
            status.TypingStarted = true;
        }

        public override bool IsStartedTyping(TMPE_EffectorBase effector) {
            Status status = _statusDic[effector];
            return status.TypingStarted;
        }

        public override void OnAttach(TMPE_EffectorBase effector) {
            Status status = new Status();
            status.Reset(effector); 
            _statusDic[effector] = status;
        }

        public override void OnDetach(TMPE_EffectorBase effector) {
            _statusDic.Remove(effector);
        }

        public override void OnTextChanged(TMPE_EffectorBase effector) {
            _statusDic[effector].Reset(effector);
        }

        public sealed override void UpdateTyping(TMPE_EffectorBase effector) {
            Status status = _statusDic[effector];

            if(status.TypingStarted == false) return;

            if(status.IsPaused) return;
            status.DelayTimer = Mathf.Max(status.DelayTimer - Time.deltaTime, 0);
            if(status.DelayTimer > 0) return;

            status.ElapsedTime += Time.deltaTime * status.TypeSpeed;

            UpdateTypingMain(effector);
        }

        protected abstract void UpdateTypingMain(TMPE_EffectorBase effector);

        public bool TryType(TMPE_EffectorBase effector, int characterInfoIndex, bool force = false, bool ignoreTypingEvent = false) {
            Status status = _statusDic[effector];

            if(force == false) {
                if(status.IsPaused || status.DelayTimer > 0) return false;
            }

            if(status.TypingStatuses[characterInfoIndex].State == CharacterTypingState.Typed) return true;

            int typeWriterIndex = effector.GetTypeWriterIndex(this);
            if(status.TypingStatuses[characterInfoIndex].BeforeTypingEventInvoked == false) {
                if(ignoreTypingEvent == false) ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.BeforeTyping, effector, typeWriterIndex, characterInfoIndex);
                status.TypingStatuses[characterInfoIndex].BeforeTypingEventInvoked = true;
            }

            if(force == false) {
                if(status.IsPaused || status.DelayTimer > 0) return false;
            }
            
            status.TypingStatuses[characterInfoIndex].State = CharacterTypingState.Typed;
            effector.TypeWriterSettings[effector.GetTypeWriterIndex(this)].OnCharacterTyped?.Invoke(characterInfoIndex);
            if(ignoreTypingEvent == false) ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.AfterTyping, effector, typeWriterIndex, characterInfoIndex);
            VisualizeCharacterIfNeed(effector, characterInfoIndex);
            
            return true;
        }

        public override bool UpdateVertex(TMPE_EffectorBase effector, int typeWriterIndex) {
            TMP_TextInfo textInfo = effector.TextInfo;
            Status status = _statusDic[effector];
            for(int i = 0; i < textInfo.characterCount; i++) {
                if(status.TypingStatuses[i].State == CharacterTypingState.Typed) {
                    status.TypingStatuses[i].ElapsedTimeFromTyped += Time.deltaTime;
                }
            }
            status.IsPlaying = base.UpdateVertex(effector, typeWriterIndex);
            return status.IsPlaying;
        }

        public override bool IsPlaying(TMPE_EffectorBase effector) {
            return _statusDic[effector].IsPlaying;
        }

        public override void PauseTyping(TMPE_EffectorBase effector) {
            _statusDic[effector].IsPaused = true;
        }

        public override void DelayTyping(TMPE_EffectorBase effector, float seconds) {
            _statusDic[effector].DelayTimer = seconds;
        }

        public override void ResumeTyping(TMPE_EffectorBase effector) {
            _statusDic[effector].IsPaused = false;
        }

        public override bool IsPausedTyping(TMPE_EffectorBase effector) => _statusDic[effector].IsPaused;

        public override void SetTypingSpeed(TMPE_EffectorBase effector, float speed) => _statusDic[effector].TypeSpeed = speed;

        public override float GetElapsedTime(TMPE_EffectorBase effector) {
            return _statusDic[effector].ElapsedTime;
        }

        public override float GetElapsedTimeFromTypedByCharacter(TMPE_EffectorBase effector, int characterInfoIndex) {
            return _statusDic[effector].TypingStatuses[characterInfoIndex].ElapsedTimeFromTyped;
        }

        public override bool IsTypedCharacter(TMPE_EffectorBase effector, int characterInfoIndex) {
            return _statusDic[effector].TypingStatuses[characterInfoIndex].State == CharacterTypingState.Typed;
        }
    }

    public abstract class TMPE_TypeWriterBase : ScriptableObject {
        [SerializeField] protected bool _visualizeCharacters = true;
        [SerializeReference] protected List<TMPE_TypingEffect> _typingEffects;
        public List<TMPE_TypingEffect> TypingEffects => _typingEffects;

        [SerializeReference] private List<TMPE_TypingEventEffect> _typingEventEffects;
        public List<TMPE_TypingEventEffect> TypingEventEffects => _typingEventEffects;

        public abstract bool IsFinishedTyping(TMPE_EffectorBase effector);

        public virtual bool UpdateVertex(TMPE_EffectorBase effector, int typeWriterIndex) {
            List<TMPE_Tag> tags = effector.TagContainer.TypingTags[typeWriterIndex];
            bool isPlaying = false;
            foreach(TMPE_TypingEffect typingEffect in _typingEffects) {
                if(string.IsNullOrEmpty(typingEffect.TagName)) {
                    isPlaying |= typingEffect.UpdateVertex(null, effector, this);
                }
            }

            foreach(TMPE_Tag tag in tags) {
                foreach(TMPE_TypingEffect typingEffect in _typingEffects) {
                    if(typingEffect.TagName == tag.Name) {
                        isPlaying |= typingEffect.UpdateVertex(tag, effector, this);
                    }
                }
            }

            return isPlaying;
        }

        public void ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming timing, TMPE_EffectorBase effector, int typeWriterIndex, int typeIndex) {
            TMP_TextInfo textInfo = effector.TextInfo;
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[typeIndex];
            int indexInTmpeTagRemovedText = characterInfo.index;
            List<TMPE_Tag> tags = effector.TagContainer.TypingEventTags[typeWriterIndex];

            foreach(TMPE_TypingEventEffect typingEventEffect in _typingEventEffects) {
                if(timing == typingEventEffect.Timing && string.IsNullOrEmpty(typingEventEffect.TagName)) {
                    typingEventEffect.OnEventTriggerd(null, effector, this, characterInfo, typeIndex);
                }
            }
            
            foreach(TMPE_Tag tag in tags) {
                if(tag.EndIndex == -1 && tag.StartIndex != indexInTmpeTagRemovedText) continue;
                if(tag.EndIndex != -1 && (indexInTmpeTagRemovedText < tag.StartIndex || tag.EndIndex < indexInTmpeTagRemovedText)) continue;

                foreach(TMPE_TypingEventEffect typingEventEffect in _typingEventEffects) {
                    if(timing == typingEventEffect.Timing && typingEventEffect.TagName == tag.Name) {
                        typingEventEffect.OnEventTriggerd(tag, effector, this, characterInfo, typeIndex);
                    }
                }
            }
        }

        public bool IsValidTypingTag(TMPE_Tag tag) {
            foreach(TMPE_TypingEffect typingEffect in _typingEffects) {
                if(typingEffect.TagName == tag.Name && typingEffect.ValidateTag(tag)) {
                    return true;
                }
            }
            return false;
        }

        public bool IsValidTypingEventTag(TMPE_Tag tag) {
            foreach(TMPE_TypingEventEffect typingEventEffect in _typingEventEffects) {
                if(typingEventEffect.TagName == tag.Name && typingEventEffect.ValidateTag(tag)) {
                    return true;
                }
            }
            return false;
        }

        protected void VisualizeCharacterIfNeed(TMPE_EffectorBase effector, int characterinfoIndex) {
            if(_visualizeCharacters) effector.TypingInfo[characterinfoIndex].Visiblity = CharacterVisiblity.Visible;
        }

        public abstract void StartTyping(TMPE_EffectorBase effector);
        public abstract bool IsStartedTyping(TMPE_EffectorBase effector);
        public virtual void OnAttach(TMPE_EffectorBase effector) {}
        public virtual void OnDetach(TMPE_EffectorBase effector) {}
        public virtual void OnTextChanged(TMPE_EffectorBase effector) {}
        public virtual void UpdateTyping(TMPE_EffectorBase effector) {}
        public abstract bool IsPlaying(TMPE_EffectorBase effector);
        public virtual void PauseTyping(TMPE_EffectorBase effector) => Debug.LogWarning(nameof(PauseTyping) + " is not implemented");
        public virtual void DelayTyping(TMPE_EffectorBase effector, float seconds) => Debug.LogWarning(nameof(PauseTyping) + " is not implemented");
        public virtual void ResumeTyping(TMPE_EffectorBase effector) => Debug.LogWarning(nameof(ResumeTyping) + " is not implemented");
        public virtual bool IsPausedTyping(TMPE_EffectorBase effector) => false;
        public virtual void SetTypingSpeed(TMPE_EffectorBase effector, float speed) => Debug.LogWarning(nameof(SetTypingSpeed) + " is not implemented");
        public virtual float GetElapsedTime(TMPE_EffectorBase effector) {
            Debug.LogWarning(nameof(GetElapsedTime) + " is not implemented");
            return 0;
        }
        public virtual float GetElapsedTimeFromTypedByCharacter(TMPE_EffectorBase effector, int characterInfoIndex) {
            Debug.LogWarning(nameof(GetElapsedTimeFromTypedByCharacter) + " is not implemented");
            return 0;
        }
        public abstract bool IsTypedCharacter(TMPE_EffectorBase effector, int characterInfoIndex);

        // TypeWriterControlTag
        private static string[] _emptyTagNames = new string[0];
        public virtual string[] AcceptableTagNames => _emptyTagNames;
        public virtual bool IsValidTypeWriterControlTag(TMPE_Tag tag) => false;
    }
}