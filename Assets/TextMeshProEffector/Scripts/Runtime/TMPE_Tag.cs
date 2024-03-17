using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    public class TMPE_TagAttribute {
        private static Stack<TMPE_TagAttribute> _pool = new Stack<TMPE_TagAttribute>();
        public static TMPE_TagAttribute Get(string name, string value) {
            if(_pool.Count > 0) {
                TMPE_TagAttribute attribute = _pool.Pop();
                attribute._name = name;
                attribute._value = value;
                return attribute;
            }
            else {
                return new TMPE_TagAttribute(name, value);
            }
        }

        public static void Release(TMPE_TagAttribute attribute) {
            attribute._name = null;
            attribute._value = null;
            _pool.Push(attribute);
        }

        private string _name;
        public string Name => _name;

        private string _value;
        public string Value => _value;

        private TMPE_TagAttribute(string name, string value) {
            _name = name;
            _value = value;
        }
    }

    public class TMPE_Tag {
        private static Stack<TMPE_Tag> _pool = new Stack<TMPE_Tag>();
        public static TMPE_Tag Get(string name) {
            if(_pool.Count > 0) {
                TMPE_Tag tag = _pool.Pop();
                tag._name = name;
                return tag;
            }
            else {
                return new TMPE_Tag(name);
            }
        }

        public static void Release(TMPE_Tag tag) {
            tag._name = null;
            tag._value = null;
            tag._startIndex = -1;
            tag._endIndex = -1;
            foreach(TMPE_TagAttribute attribute in tag._attributes) {
                TMPE_TagAttribute.Release(attribute);
            }
            tag._attributes.Clear();
            _pool.Push(tag);
        }

        private string _name;
        public string Name => _name;

        private string _value;
        public string Value {
            get => _value;
            set => _value = value;
        }

        private readonly List<TMPE_TagAttribute> _attributes = new List<TMPE_TagAttribute>();
        public List<TMPE_TagAttribute> Attributes => _attributes;

        private int _startIndex = -1;
        public int StartIndex {
            get => _startIndex;
            set => _startIndex = value;
        }

        private int _endIndex = -1;
        public int EndIndex {
            get => _endIndex;
            set => _endIndex = value;
        }

        private TMPE_Tag(string name) {
            _name = name;
        }

        public TMPE_TagAttribute GetAttribute(string name) {
            foreach(TMPE_TagAttribute attribute in _attributes) {
                if(attribute.Name == name) return attribute;
            }
            return null;
        }
    }
}