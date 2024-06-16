using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypeWriters {
    public class BasicTypingStatus : TMPE_TypingBehaviourStatus {
        public int TypedCharacterCount {get; set;}
        public override void Reset(TMPE_TypeWriter typeWriter) {
            base.Reset(typeWriter);
            TypedCharacterCount = 0;
        }
    }

    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TypeWriters) + "/" + nameof(BasicTyping), fileName = nameof(BasicTyping))]
    public class BasicTyping : TMPE_TypingBehaviour<BasicTypingStatus> {
        private const string TAG_FIXED_TIME_TYPING = "time";
        private static string[] _acceptableTagNames = new string[]{TAG_FIXED_TIME_TYPING};
        public override string[] AcceptableTagNames => _acceptableTagNames;

        [SerializeField, Min(0)] private float _intervalPerChar;

        public override bool IsValidTypeWriterControlTag(TMPE_Tag tag) {
            if(tag.Name == TAG_FIXED_TIME_TYPING) {
                return float.TryParse(tag.Value, out _) && tag.Attributes.Count == 0;
            }
            return false;
        }

        protected override void UpdateTypingMain(TMPE_TypeWriter typeWriter, BasicTypingStatus status) {
            TMPE_EffectorBase effector = typeWriter.Effector;
            TMP_TextInfo textInfo = effector.TextInfo;
            IReadOnlyList<TMPE_Tag> typewriterControlTags = typeWriter.TypeWriterControlTagContainer.Tags;

            for(int i = 0; i < textInfo.characterCount; i++) {
                TMPE_CharacterTypingStatus charStatus = typeWriter.CharacterTypingStatuses[i];
                if(charStatus.IsTyped()) continue;

                TMPE_Tag targetTag = null;
                for(int j = typewriterControlTags.Count - 1; j >= 0; j--) {
                    TMPE_Tag tag = typewriterControlTags[j];
                    if(tag.Name != TAG_FIXED_TIME_TYPING) continue;
                    if(tag.ContainsIndex(i)) {
                        targetTag = tag;
                        break;
                    }
                }
                if(targetTag == null) continue;

                float fixedTime = float.Parse(targetTag.Value);
                if(typeWriter.TypingElapsedTime >= fixedTime) {
                    typeWriter.TryType(i, true);
                }
            }

            // 表示文字数計算
            int characterNumShouldBeTyped;
            if (_intervalPerChar == 0) {
                characterNumShouldBeTyped = textInfo.characterCount;
            }
            else {
                characterNumShouldBeTyped = Mathf.Min(Mathf.CeilToInt(typeWriter.TypingElapsedTime / _intervalPerChar), textInfo.characterCount);
            }

            int typeNum = characterNumShouldBeTyped - status.TypedCharacterCount;
            if(typeNum > 0) {
                int typeStartIndex = status.TypedCharacterCount;
                int typeEndIndex = characterNumShouldBeTyped - 1;
                for(int i = typeStartIndex; i <= typeEndIndex; i++) {
                    TMPE_Tag fixedTimeTag = null;
                    for(int j = typewriterControlTags.Count - 1; j >= 0; j--) {
                        TMPE_Tag tag = typewriterControlTags[j];
                        if(tag.Name != TAG_FIXED_TIME_TYPING) continue;
                        if(tag.ContainsIndex(i)) {
                            fixedTimeTag = tag;
                            break;
                        }
                    }
                    if(fixedTimeTag != null) continue;
                    
                    if(typeWriter.TryType(i) == false) break;

                    status.TypedCharacterCount++;
                }
            }
        }
    }
}