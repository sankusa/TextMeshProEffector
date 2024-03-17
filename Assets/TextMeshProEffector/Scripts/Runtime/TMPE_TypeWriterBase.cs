using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    public enum CharacterTypingState {Idle, Typing}
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
        public float PauseTimer {get; set;}
        public float TypeSpeed {get; set;} = 1;
        private CharStatus[] _typingStatuses;
        public CharStatus[] TypingStatuses => _typingStatuses;

        public virtual void Reset(IEffector effector) {
            TypingStarted = false;
            ElapsedTime = 0;
            IsPaused = false;
            PauseTimer = 0;
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
            else {
                foreach(CharacterTypingStatus status in _typingStatuses) {
                    status.Reset();
                }
            }
        }
    }

    public abstract class TMPE_TypeWriterGeneric<Status, CharStatus> : TMPE_TypeWriterBase where Status : TypeWriterStatus<CharStatus>, new() where CharStatus : CharacterTypingStatus, new() {
        protected Dictionary<IEffector, Status> _statusDic = new Dictionary<IEffector, Status>();

        public override bool IsFinishedTyping(IEffector effector) {
            Status status = _statusDic[effector];
            int count = 0;
            for(int i = 0; i < effector.TextInfo.characterCount; i++) {
                if(status.TypingStatuses[i].State == CharacterTypingState.Idle) count++;
            }
            return count == 0;
        }

        public override void StartTyping(IEffector effector) {
            Status status = _statusDic[effector];
            status.TypingStarted = true;
        }

        public override bool IsStartedTyping(IEffector effector) {
            Status status = _statusDic[effector];
            return status.TypingStarted;
        }

        public override void OnAttach(IEffector effector) {
            Status status = new Status();
            status.Reset(effector); 
            _statusDic[effector] = status;
        }

        public override void OnDetach(IEffector effector) {
            _statusDic.Remove(effector);
        }

        public override void OnTextChanged(IEffector effector) {
            _statusDic[effector].Reset(effector);
        }

        public sealed override void UpdateTyping(IEffector effector) {
            Status status = _statusDic[effector];

            if(status.TypingStarted == false) return;

            if(status.IsPaused) return;
            status.PauseTimer = Mathf.Max(status.PauseTimer - Time.deltaTime, 0);
            if(status.PauseTimer > 0) return;

            status.ElapsedTime += Time.deltaTime * status.TypeSpeed;

            UpdateTypingMain(effector);
        }

        protected abstract void UpdateTypingMain(IEffector effector);

        public bool TryType(IEffector effector, int characterInfoIndex, bool ignoreTypingEvent = false) {
            Status status = _statusDic[effector];
            if(status.IsPaused || status.PauseTimer > 0) return false;

            if(status.TypingStatuses[characterInfoIndex].State == CharacterTypingState.Typing) return true;

            int typeWriterIndex = effector.GetTypeWriterIndex(this);
            if(status.TypingStatuses[characterInfoIndex].BeforeTypingEventInvoked == false) {
                if(ignoreTypingEvent == false) ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.BeforeTyping, effector, typeWriterIndex, characterInfoIndex);
                status.TypingStatuses[characterInfoIndex].BeforeTypingEventInvoked = true;
            }

            if(status.IsPaused || status.PauseTimer > 0) return false;
            
            status.TypingStatuses[characterInfoIndex].State = CharacterTypingState.Typing;
            if(ignoreTypingEvent == false) ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.AfterTyping, effector, typeWriterIndex, characterInfoIndex);
            VisualizeCharacterIfNeed(effector, characterInfoIndex);
            
            return true;
        }

        public override bool UpdateVertex(IEffector effector, int typeWriterIndex) {
            TMP_TextInfo textInfo = effector.TextInfo;
            Status status = _statusDic[effector];
            for(int i = 0; i < textInfo.characterCount; i++) {
                if(status.TypingStatuses[i].State == CharacterTypingState.Typing) {
                    status.TypingStatuses[i].ElapsedTimeFromTyped += Time.deltaTime;
                }
            }
            return base.UpdateVertex(effector, typeWriterIndex);
        }

        public override void Pause(IEffector effector) {
            _statusDic[effector].IsPaused = true;
        }

        public override void Pause(IEffector effector, float seconds) {
            _statusDic[effector].PauseTimer = seconds;
        }

        public override void Resume(IEffector effector) {
            _statusDic[effector].IsPaused = false;
        }

        public override bool IsPaused(IEffector effector) => _statusDic[effector].IsPaused;

        public override void SetTypeSpeed(IEffector effector, float speed) => _statusDic[effector].TypeSpeed = speed;

        public override float GetElapsedTime(IEffector effector) {
            return _statusDic[effector].ElapsedTime;
        }

        public override float GetElapsedTimeFromTypedByCharacter(IEffector effector, int characterInfoIndex) {
            return _statusDic[effector].TypingStatuses[characterInfoIndex].ElapsedTimeFromTyped;
        }
    }

    public abstract class TMPE_TypeWriterBase : ScriptableObject {
        [SerializeField] protected bool _visualizeCharacters = true;
        [SerializeReference] protected List<TMPE_TypingEffect> _typingEffects;
        public List<TMPE_TypingEffect> TypingEffects => _typingEffects;

        [SerializeReference] private List<TMPE_TypingEventEffect> _typingEventEffects;
        public List<TMPE_TypingEventEffect> TypingEventEffects => _typingEventEffects;

        public abstract bool IsFinishedTyping(IEffector effector);

        public virtual bool UpdateVertex(IEffector effector, int typeWriterIndex) {
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

        public void ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming timing, IEffector effector, int typeWriterIndex, int typeIndex) {
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

        protected void VisualizeCharacterIfNeed(IEffector effector, int characterinfoIndex) {
            if(_visualizeCharacters) effector.TypingInfo[characterinfoIndex].Visiblity = CharacterVisiblity.Visible;
        }

        public abstract void StartTyping(IEffector effector);
        public abstract bool IsStartedTyping(IEffector effector);
        public virtual void OnAttach(IEffector effector) {}
        public virtual void OnDetach(IEffector effector) {}
        public virtual void OnTextChanged(IEffector effector) {}
        public virtual void UpdateTyping(IEffector effector) {}
        public virtual void Pause(IEffector effector) => Debug.LogWarning(nameof(Pause) + " is not implemented");
        public virtual void Pause(IEffector effector, float seconds) => Debug.LogWarning(nameof(Pause) + " is not implemented");
        public virtual void Resume(IEffector effector) => Debug.LogWarning(nameof(Resume) + " is not implemented");
        public virtual bool IsPaused(IEffector effector) => false;
        public virtual void SetTypeSpeed(IEffector effector, float speed) => Debug.LogWarning(nameof(SetTypeSpeed) + " is not implemented");
        public virtual float GetElapsedTime(IEffector effector) {
            Debug.LogWarning(nameof(GetElapsedTime) + " is not implemented");
            return 0;
        }
        public virtual float GetElapsedTimeFromTypedByCharacter(IEffector effector, int characterInfoIndex) {
            Debug.LogWarning(nameof(GetElapsedTimeFromTypedByCharacter) + " is not implemented");
            return 0;
        }

        // TypeWriterControlTag
        private static string[] _emptyTagNames = new string[0];
        public virtual string[] AcceptableTagNames => _emptyTagNames;
        public virtual bool IsValidTypeWriterControlTag(TMPE_Tag tag) => false;
    }
}