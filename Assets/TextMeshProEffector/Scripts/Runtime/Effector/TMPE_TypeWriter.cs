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

    public enum CharacterVisualizationType {
        None = 0,
        ToVisible = 1,
        ToInvisible = 2,
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

        [SerializeField] private CharacterVisualizationType _visualizeCharacters = CharacterVisualizationType.ToVisible;
        public CharacterVisualizationType VisualizeCharacters => _visualizeCharacters;

        [SerializeField] private AutoStartTypingType _startTypingAuto = AutoStartTypingType.Auto;
        public AutoStartTypingType StartTypingAuto => _startTypingAuto;

#region OnOtherTypeWriterStartedTyping
        [Serializable]
        public class OtherTypewriterWaitSetting {
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

        [SerializeField] private OtherTypewriterWaitSetting _otherTypewriterWaitSetting;
        public OtherTypewriterWaitSetting OtherTypewriterSetting => _otherTypewriterWaitSetting;
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
        internal OneShotUnityEvent OnTypingCompletedInternal => _onTypingCompleted;
        public UnityEvent OnTypingCompleted => _onTypingCompleted.UnityEvent;
#endregion

        private bool _isPlaying;
        public bool IsPlaying => _isPlaying;

        private readonly TMPE_TagContainer _typingTagContainer = new TMPE_TagContainer();
        internal TMPE_TagContainer TypingTagContainer => _typingTagContainer;

        private readonly TMPE_TagContainer _typingEventTagContainer = new TMPE_TagContainer();
        internal TMPE_TagContainer TypingEventTagContainer => _typingEventTagContainer;

        private TMPE_TagContainer _typeWriterControlTagContainer = new TMPE_TagContainer();
        internal TMPE_TagContainer TypeWriterControlTagContainer => _typeWriterControlTagContainer;

        // Unityイベント関数
        void Reset() {
            _effector = GetComponent<TMPE_EffectorBase>();
        }

        internal bool UpdateVertex() {
            _typingBehaviour.Tick(this);

            _isPlaying = false;
            foreach(TMPE_TypingEffectContainer typingEffectContainer in  _typingEffectContainers) {
                if(typingEffectContainer == null) continue;
                _isPlaying |= typingEffectContainer.UpdateVertex(this, _effector.TypeWriters.FindIndex(x => x == this));
            }
            return _isPlaying;
        }

        internal void ClearTag() {
            _typingTagContainer.Clear();
            _typingEventTagContainer.Clear();
            _typeWriterControlTagContainer.Clear();
        }
        
        internal bool IsValidTypingTag(TMPE_Tag tag) {
            bool isValidTypingTag = false;
            foreach(TMPE_TypingEffectContainer typingEffectContainer in  _typingEffectContainers) {
                if(typingEffectContainer == null) continue;
                isValidTypingTag |= typingEffectContainer.ValidateTag(tag);
            }
            return isValidTypingTag;
        }

        internal bool IsValidTypingEventTag(TMPE_Tag tag) {
            bool isValidTypingEventTag = false;
            foreach(TMPE_TypingEventEffectContainer typingEventEffectContainer in  _typingEventEffectContainers) {
                if(typingEventEffectContainer == null) continue;
                isValidTypingEventTag |= typingEventEffectContainer.ValidateTag(tag);
            }
            return isValidTypingEventTag;
        }

        internal void ResetTypinRelatedValues() {
            _onTypingCompleted.Reset();
            _isPlaying = false;
        }
    }
}