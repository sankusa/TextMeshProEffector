using System;
using UnityEngine;

namespace TextMeshProEffector {
    [Serializable]
    public abstract class TMPE_EffectBase {
        [SerializeField] protected string _tagName;
        public string TagName => _tagName;

        public bool HasTag() => string.IsNullOrEmpty(_tagName) == false;

        public bool ValidateTag(TMPE_Tag tag) => tag.Name == _tagName && ValidateTagWithoutName(tag);
        public virtual bool ValidateTagWithoutName(TMPE_Tag tag) => tag.Value == null && tag.Attributes.Count == 0;

        public virtual string GetCaption() => "";
        public virtual string GetToolTip() => "";
    }
}