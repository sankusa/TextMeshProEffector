using System;
using System.Collections.Generic;

namespace TextMeshProEffector {
    /// <summary>
    /// リッチテキストタグの属性
    /// </summary>
    public class TMPE_TagAttribute {
        private readonly static Stack<TMPE_TagAttribute> _objectPool = new Stack<TMPE_TagAttribute>();

        internal static TMPE_TagAttribute GetInstance(string name, string value) {
            if(_objectPool.Count == 0) {
                return new TMPE_TagAttribute(name, value);
            }
            else {
                TMPE_TagAttribute attribute = _objectPool.Pop();
                attribute.SetUp(name, value);
                return attribute;
            }
        }

        internal static void ReleaseInstance(TMPE_TagAttribute attribute) {
            if(attribute == null) throw new ArgumentNullException(nameof(attribute));
            if(_objectPool.Contains(attribute)) throw new InvalidOperationException($"Attribute has already been released.");

            attribute.Clear();
            _objectPool.Push(attribute);
        }



        private string _name;
        public string Name => _name;

        private string _value;
        public string Value => _value;

        private TMPE_TagAttribute(string name, string value) {
            SetUp(name, value);
        }

        public bool IsMatchingIntAttribute(string name) {
            return _name == name && int.TryParse(_value, out int _);
        }

        public bool IsMatchingFloatAttribute(string name) {
            return _name == name && float.TryParse(_value, out float _);
        }

        public bool IsMatchingStringAttribute(string name) {
            return _name == name;
        }

        private void SetUp(string name, string value) {
            if(name == null) throw new ArgumentNullException(nameof(name));
            if(name.Length == 0) throw new ArgumentException("Parameter cannot be empty", nameof(name));
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(value.Length == 0) throw new ArgumentException("Parameter cannot be empty", nameof(value));

            _name = name;
            _value = value;
        }

        private void Clear() {
            _name = null;
            _value = null;
        }
    }
}