using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TextMeshProEffector {
    public class RandomTypingStatus : TMPE_TypingBehaviourStatus {
        public int NextTypeIndex {get; set;}
        public float NextTypeInterval {get; set;}
        public float Timer {get; set;}
        public override void Reset(TMPE_TypeWriter typeWriter) {
            base.Reset(typeWriter);
            NextTypeIndex = 0;
            NextTypeInterval = 0;
            Timer = 0;
        }
    }

    [CreateAssetMenu(menuName = nameof(TextMeshProEffector) + "/" + nameof(TypeWriters) + "/" + nameof(RandomTyping), fileName = nameof(RandomTyping))]
    public class RandomTyping : TMPE_TypingBehaviour<RandomTypingStatus> {
        [SerializeField, Min(0)] private float _intervalPerChar;
        [SerializeField, Min(0)] private float _intervalPerCharRandomness;

        protected override void InitializeStatusMain(TMPE_TypeWriter typeWriter, RandomTypingStatus status) {
            status.NextTypeIndex = (int) (Random.value * typeWriter.Effector.TextInfo.characterCount);
        }

        protected override void UpdateTypingMain(TMPE_TypeWriter typeWriter, RandomTypingStatus status) {
            TMPE_EffectorBase effector = typeWriter.Effector;
            TMP_TextInfo textInfo = effector.TextInfo;

            status.Timer += Time.deltaTime * typeWriter.TypingSpeed;

            if(textInfo.characterCount == 0) return;

            while(status.Timer > status.NextTypeInterval) {
                status.Timer -= status.NextTypeInterval;

                if(typeWriter.TryType(status.NextTypeIndex) == false) break;

                // 次のタイピングの情報を生成
                status.NextTypeInterval = Mathf.Max(_intervalPerChar + (2f * Random.value - 1) * _intervalPerCharRandomness, 0);
                int notTypedNum = 0;
                for(int i = 0; i < textInfo.characterCount; i++) {
                    if(typeWriter.CharacterTypingStatuses[i].IsTyped() == false) notTypedNum++;
                }
                if(notTypedNum == 0) break;
                
                int targetNotTypedNum = (int)(Random.value * notTypedNum);
                int notTypedCounter = 0;
                for(int i = 0; i < textInfo.characterCount; i++) {
                    if(typeWriter.CharacterTypingStatuses[i].IsTyped() == false) {
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