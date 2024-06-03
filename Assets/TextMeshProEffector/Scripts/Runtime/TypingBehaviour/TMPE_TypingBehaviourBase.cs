using UnityEngine;

namespace TextMeshProEffector {
    public abstract class TMPE_TypingBehaviourBase : ScriptableObject {
        public string FindMatchedTypeWriterControlTagName(char[] array, int arrayStartIndex, int arrayEndIndex) {
            for(int i = 0; i < AcceptableTagNames.Length; i++) {
                if(AcceptableTagNames[i].EqualsPartialCharArray(array, arrayStartIndex, arrayEndIndex)) {
                    return AcceptableTagNames[i];
                }
            }
            return null;
        }

        public abstract bool IsFinishedTyping(TMPE_TypeWriter typeWriter);

        public virtual void Tick(TMPE_TypeWriter typeWriter) {}

        protected void ChangeCharacterVisiblityIfNeed(TMPE_TypeWriter typeWriter, int characterinfoIndex) {
            TMPE_EffectorBase effector = typeWriter.Effector;
            if(typeWriter.VisualizeCharacters == CharacterVisualizationType.ToVisible) {
                effector.TypingInfo[characterinfoIndex].Visiblity = CharacterVisiblity.Visible;
            }
            else if(typeWriter.VisualizeCharacters == CharacterVisualizationType.ToInvisible) {
                effector.TypingInfo[characterinfoIndex].Visiblity = CharacterVisiblity.Invisible;
            }
        }

        public abstract void StartTyping(TMPE_TypeWriter typeWriter);
        public abstract bool IsStartedTyping(TMPE_TypeWriter typeWriter);
        public abstract void OnAttach(TMPE_TypeWriter typeWriter);
        public abstract void OnDetach(TMPE_TypeWriter typeWriter);
        public abstract void OnTextChanged(TMPE_TypeWriter typeWriter);
        public abstract void UpdateTyping(TMPE_TypeWriter typeWriter);
        public abstract void PauseTyping(TMPE_TypeWriter typeWriter);
        public abstract void DelayTyping(TMPE_TypeWriter typeWriter, float seconds);
        public abstract void ResumeTyping(TMPE_TypeWriter typeWriter);
        public abstract bool IsPausedTyping(TMPE_TypeWriter typeWriter);
        public abstract void SetTypeWriterSpeed(TMPE_TypeWriter typeWriter, float speed);
        public abstract void SetTypingSpeed(TMPE_TypeWriter typeWriter, float speed);
        public abstract void SetTypingEffectSpeed(TMPE_TypeWriter typeWriter, float speed);
        public abstract float GetElapsedTimeForTyping(TMPE_TypeWriter typeWriter);
        public abstract float GetElapsedTimeForTypingEffect(TMPE_TypeWriter typeWriter, int characterInfoIndex);
        public abstract bool IsTypedCharacter(TMPE_TypeWriter typeWriter, int characterInfoIndex);

        // TypeWriterControlTag
        private static string[] _emptyTagNames = new string[0];
        public virtual string[] AcceptableTagNames => _emptyTagNames;
        public virtual bool IsValidTypeWriterControlTag(TMPE_Tag tag) => false;
    }
}