using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TextMeshProEffector {
    public class TMPE_Effector : TMPE_EffectorBase {
        [SerializeField, TextArea(5, 10)] private string _text;

        protected override void OnEnable() {
            base.OnEnable();
#if UNITY_EDITOR
            if(EditorApplication.isPlaying == false) {
                SetText(_text);
            }
#endif
        }

        protected override void Start() {
            base.Start();
#if UNITY_EDITOR
            if(EditorApplication.isPlaying) {
                SetText(_text);
            }
#else
            SetText(_text);
#endif
        }
    }
}