using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypeWriters {
    public class BasicTypingCharacterStatus : CharacterTypingStatus {
        public float FixedTypeTiming {get; set;} = -1;
        public override void Reset() {
            base.Reset();
            FixedTypeTiming = -1;
        }
    }

    public class BasicTypingStatus : TMPE_TypingBehaviourStatus<BasicTypingCharacterStatus> {
        public int TypedCharacterCount {get; set;}
        public override void Reset(TMPE_TypeWriter typeWriter) {
            base.Reset(typeWriter);
            TypedCharacterCount = 0;
        }
    }

    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TypeWriters) + "/" + nameof(BasicTyping), fileName = nameof(BasicTyping))]
    public class BasicTyping : TMPE_TypingBehaviour<BasicTypingStatus, BasicTypingCharacterStatus> {
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

        public override void OnTextChanged(TMPE_TypeWriter typeWriter) {
            base.OnTextChanged(typeWriter);
            BasicTypingStatus status = _statusDic[typeWriter];
            IReadOnlyList<TMPE_Tag> typeWriterControlTags = typeWriter.TypeWriterControlTagContainer.Tags;
            for(int i = 0; i < typeWriterControlTags.Count; i++) {
                TMPE_Tag tag = typeWriterControlTags[i];
                if(tag.Name == TAG_FIXED_TIME_TYPING) {
                    float time = float.Parse(tag.Value);
                    for(int j = tag.StartIndex; j <= tag.EndIndex; j++) {
                        status.CharacterTypingStatuses[j].FixedTypeTiming = time;
                    }
                }
            }
        }

        protected override void UpdateTypingMain(TMPE_TypeWriter typeWriter) {
            TMPE_EffectorBase effector = typeWriter.Effector;
            TMP_TextInfo textInfo = effector.TextInfo;
            BasicTypingStatus status = _statusDic[typeWriter];

            for(int i = 0; i < textInfo.characterCount; i++) {
                BasicTypingCharacterStatus charStatus = status.CharacterTypingStatuses[i];
                if(charStatus.State == CharacterTypingState.Idle && charStatus.FixedTypeTiming >= 0 && status.ElapsedTimeForTyping >= charStatus.FixedTypeTiming) {
                    TryType(typeWriter, i);
                }
            }

            // 表示文字数計算
            int characterNumShouldBeTyped;
            if (_intervalPerChar == 0) {
                characterNumShouldBeTyped = textInfo.characterCount;
            }
            else {
                characterNumShouldBeTyped = Mathf.Min(Mathf.CeilToInt(status.ElapsedTimeForTyping / _intervalPerChar), textInfo.characterCount);
            }

            int typeNum = characterNumShouldBeTyped - status.TypedCharacterCount;
            if(typeNum > 0) {
                int typeStartIndex = status.TypedCharacterCount;
                int typeEndIndex = characterNumShouldBeTyped - 1;
                for(int i = typeStartIndex; i <= typeEndIndex; i++) {
                    if(status.CharacterTypingStatuses[i].FixedTypeTiming < 0) {
                        if(TryType(typeWriter, i) == false) break;
                    }

                    status.TypedCharacterCount++;
                }
            }
        }
    }
}