using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector.TypeWriters {
    public class BasicTypeWriterCharacterTypingStatus : CharacterTypingStatus {
        public float FixedTypeTiming {get; set;} = -1;
        public override void Reset() {
            base.Reset();
            FixedTypeTiming = -1;
        }
    }
    public class BasicTypeWriterStatus : TypeWriterStatus<BasicTypeWriterCharacterTypingStatus> {
        public int TypedCharacterCount {get; set;}
        public override void Reset(TMPE_TypeWriter typeWriter) {
            base.Reset(typeWriter);
            TypedCharacterCount = 0;
        }
    }

    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TypeWriters) + "/" + nameof(BasicTypeWriter), fileName = nameof(BasicTypeWriter))]
    public class BasicTypeWriter : TMPE_TypingBehaviour<BasicTypeWriterStatus, BasicTypeWriterCharacterTypingStatus> {
        private static string[] _acceptableTagNames = new string[]{"time"};
        public override string[] AcceptableTagNames => _acceptableTagNames;

        [SerializeField, Min(0)] private float _intervalPerChar;

        public override bool IsValidTypeWriterControlTag(TMPE_Tag tag) {
            if(tag.Name == "time") {
                return float.TryParse(tag.Value, out _) && tag.Attributes.Count == 0;
            }
            return false;
        }

        public override void OnTextChanged(TMPE_TypeWriter typeWriter) {
            base.OnTextChanged(typeWriter);
            BasicTypeWriterStatus status = _statusDic[typeWriter];
            List<TMPE_Tag> typeWriterControlTags = typeWriter.Effector.TagContainer.TypeWriterControlTags[typeWriter.Effector.GetTypeWriterIndex(this)];
            for(int i = 0; i < typeWriterControlTags.Count; i++) {
                TMPE_Tag tag = typeWriterControlTags[i];
                if(tag.Name == "time") {
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
            BasicTypeWriterStatus status = _statusDic[typeWriter];

            for(int i = 0; i < textInfo.characterCount; i++) {
                var charStatus = status.CharacterTypingStatuses[i];
                if(charStatus.State == CharacterTypingState.Idle && charStatus.FixedTypeTiming >= 0 && status.ElapsedTimeForTyping >= charStatus.FixedTypeTiming) {
                    TryType(effector, typeWriter, i); 
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
                        if(TryType(effector, typeWriter, i) == false) break;
                    }

                    status.TypedCharacterCount++;
                }
            }
        }
    }
}