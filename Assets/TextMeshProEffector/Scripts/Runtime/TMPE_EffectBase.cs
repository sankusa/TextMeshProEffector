using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    [Serializable]
    public abstract class TMPE_EffectBase {
        [SerializeField] private string _tagName;
        public string TagName => _tagName;

        public virtual bool ValidateTag(TMPE_Tag tag) => tag.Value == null && tag.Attributes.Count == 0;
    }
}