using System.Collections;
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
        public List<TMPE_TypingEffectContainer> TypingEffectContainers => _typingEffectContainers;

        [SerializeField] private List<TMPE_TypingEventEffectContainer> _typingEventEffectContainers;
        public List<TMPE_TypingEventEffectContainer> TypingEventEffectContainers => _typingEventEffectContainers;

        [SerializeField] private CharacterVisualizationType _visualizeCharacters = CharacterVisualizationType.ToVisible;
        public CharacterVisualizationType VisualizeCharacters => _visualizeCharacters;

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
#region Event
        [SerializeField] private UnityEvent<int> _onCharacterTyped;
        public UnityEvent<int> OnCharacterTyped => _onCharacterTyped;

        [SerializeField] private OneShotUnityEvent _onTypingCompleted;
        internal OneShotUnityEvent OnTypingCompletedInternal => _onTypingCompleted;
        public UnityEvent OnTypingCompleted => _onTypingCompleted.UnityEvent;
#endregion

        private bool _isPlaying;
        public bool IsPlaying => _isPlaying;

        public void Reset() {
            _effector = GetComponent<TMPE_EffectorBase>();
        }

        public bool UpdateVertex() {
            _typingBehaviour.Tick(_effector);

            _isPlaying = false;
            foreach(TMPE_TypingEffectContainer typingEffectContainer in  _typingEffectContainers) {
                if(typingEffectContainer == null) continue;
                _isPlaying |= typingEffectContainer.UpdateVertex(_effector, _effector.TypeWriters.FindIndex(x => x == this));
            }
            return _isPlaying;
        }

        public bool IsValidTypingTag(TMPE_Tag tag) {
            bool isValidTypingTag = false;
            foreach(TMPE_TypingEffectContainer typingEffectContainer in  _typingEffectContainers) {
                if(typingEffectContainer == null) continue;
                isValidTypingTag |= typingEffectContainer.IsValidTypingTag(tag);
            }
            return isValidTypingTag;
        }

        public bool IsValidTypingEventTag(TMPE_Tag tag) {
            bool isValidTypingEventTag = false;
            foreach(TMPE_TypingEventEffectContainer typingEventEffectContainer in  _typingEventEffectContainers) {
                if(typingEventEffectContainer == null) continue;
                isValidTypingEventTag |= typingEventEffectContainer.IsValidTypingEventTag(tag);
            }
            return isValidTypingEventTag;
        }

        public void ResetTypinRelatedValues() {
            _onTypingCompleted.Reset();
            _isPlaying = false;
        }
    }
}