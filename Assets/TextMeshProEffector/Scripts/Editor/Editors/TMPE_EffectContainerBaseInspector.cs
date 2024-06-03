using UnityEditor;

namespace TextMeshProEffector {
    [CustomEditor(typeof(TMPE_EffectContainerBase<>), true)]
    public class TMPE_EffectContainerBaseInspector : Editor {
        private EffectContainer_EffectsReorderableList _effectList;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            TMPE_EffectContainerBase effectContainer = target as TMPE_EffectContainerBase;

            if(_effectList == null) _effectList = new EffectContainer_EffectsReorderableList(serializedObject, effectContainer, "Effects");
            _effectList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}