using UnityEngine;

namespace TextMeshProEffector {
    public class TMPE_Effector : TMPE_EffectorBase {
        [SerializeField, TextArea(5, 10)] private string _text;

        protected override void Initialize() {
            _textPreprocessor.ProcessText(this, _text);
            _textComponent.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
            base.Initialize();
        }
    }
}