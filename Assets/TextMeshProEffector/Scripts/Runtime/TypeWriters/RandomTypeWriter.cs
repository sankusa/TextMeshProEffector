using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.Commands;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TextMeshProEffector {
    public class RandomTypeWriterStatus : TypeWriterStatus<CharacterTypingStatus> {
        public int NextTypeIndex {get; set;}
        public float NextTypeInterval {get; set;}
        public float Timer {get; set;}
        public override void Reset(IEffector effector) {
            base.Reset(effector);
            NextTypeIndex = 0;
            NextTypeInterval = 0;
            Timer = 0;
        }
    }

    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TypeWriters) + "/" + nameof(RandomTypeWriter), fileName = nameof(RandomTypeWriter))]
    public class RandomTypeWriter : TMPE_TypeWriterGeneric<RandomTypeWriterStatus, CharacterTypingStatus> {
        [SerializeField, Min(0)] private float _intervalPerChar;
        [SerializeField, Min(0)] private float _intervalPerCharRandomness;

        public override void OnTextChanged(IEffector effector) {
            base.OnTextChanged(effector);
            _statusDic[effector].NextTypeIndex = (int) (Random.value * effector.TextInfo.characterCount);
        }

        protected override void UpdateTypingMain(IEffector effector) {
            TMP_TextInfo textInfo = effector.TextInfo;
            RandomTypeWriterStatus status = _statusDic[effector];

            status.Timer += Time.deltaTime * status.TypeSpeed;

            while(status.Timer > status.NextTypeInterval) {
                status.Timer -= status.NextTypeInterval;

                if(TryType(effector, status.NextTypeIndex) == false) break;

                // 次のタイピングの情報を生成
                status.NextTypeInterval = Mathf.Max(_intervalPerChar + (2f * Random.value - 1) * _intervalPerCharRandomness, 0);
                int notTypedNum = 0;
                for(int i = 0; i < textInfo.characterCount; i++) {
                    if(status.TypingStatuses[i].State == CharacterTypingState.Idle) notTypedNum++;
                }
                if(notTypedNum == 0) break;
                
                int targetNotTypedNum = (int)(Random.value * notTypedNum);
                int notTypedCounter = 0;
                for(int i = 0; i < textInfo.characterCount; i++) {
                    if(status.TypingStatuses[i].State == CharacterTypingState.Idle) {
                        if(notTypedCounter == targetNotTypedNum) {
                            status.NextTypeIndex = i;
                            break;
                        }
                        else {
                            notTypedCounter++;
                        }
                    }
                }
            }
        }
    }
}