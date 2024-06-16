using UnityEngine;

namespace TextMeshProEffector {
    public abstract class TMPE_TypingBehaviourBase : ScriptableObject {
        public abstract TMPE_TypingBehaviourStatus GenerateStatus(TMPE_TypeWriter typeWriter);
        public abstract bool IsValidStatus(TMPE_TypingBehaviourStatus status);
        public void MaintainStatus(TMPE_TypeWriter typeWriter, ref TMPE_TypingBehaviourStatus status) {
            if(IsValidStatus(status) == false) {
                status = GenerateStatus(typeWriter);
            }
        }

        public string FindMatchedTypeWriterControlTagName(char[] array, int arrayStartIndex, int arrayEndIndex) {
            for(int i = 0; i < AcceptableTagNames.Length; i++) {
                if(AcceptableTagNames[i].EqualsPartialCharArray(array, arrayStartIndex, arrayEndIndex)) {
                    return AcceptableTagNames[i];
                }
            }
            return null;
        }

        public abstract void InitializeStatus(TMPE_TypeWriter typeWriter, TMPE_TypingBehaviourStatus status);

        public abstract void UpdateTyping(TMPE_TypeWriter typeWriter);
    

        // TypeWriterControlTag
        private static string[] _emptyTagNames = new string[0];
        public virtual string[] AcceptableTagNames => _emptyTagNames;
        public virtual bool IsValidTypeWriterControlTag(TMPE_Tag tag) => false;
    }
}