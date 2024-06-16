using System;
using System.Collections.Generic;

namespace TextMeshProEffector {
    /// <summary>
    /// リッチテキストタグ
    /// </summary>
    public class TMPE_Tag {
        public const int INDEX_UNSETTED = -1;

        private static Stack<TMPE_Tag> _objectPool = new Stack<TMPE_Tag>();

        internal static TMPE_Tag GetInstance(string name) {
            if(_objectPool.Count == 0) {
                return new TMPE_Tag(name);
            }
            else {
                TMPE_Tag tag = _objectPool.Pop();
                tag.SetUp(name);
                return tag;
            }
        }

        internal static void ReleaseInstance(TMPE_Tag tag) {
            if(tag == null) throw new ArgumentNullException(nameof(tag));
            if(_objectPool.Contains(tag)) throw new InvalidOperationException($"Tag has already been released.");

            tag.Clear();
            _objectPool.Push(tag);
        }



        private string _name;
        public string Name => _name;

        private string _value;
        public string Value {
            get => _value;
            set {
                if(value == null) throw new ArgumentNullException(nameof(value));
                if(value.Length == 0) throw new ArgumentException("Parameter cannot be empty", nameof(value));

                _value = value;
            }
        }

        private readonly List<TMPE_TagAttribute> _attributes = new List<TMPE_TagAttribute>();
        public IReadOnlyList<TMPE_TagAttribute> Attributes => _attributes;

        private int _startIndex = INDEX_UNSETTED;
        public int StartIndex => _startIndex;

        private int _endIndex = INDEX_UNSETTED;
        public int EndIndex => _endIndex;

        private TMPE_Tag(string name) {
            SetUp(name);
        }

        private void SetUp(string name) {
            if(name == null) throw new ArgumentNullException(nameof(name));
            if(name.Length == 0) throw new ArgumentException("Parameter cannot be empty", nameof(name));
            
            _name = name;
        }

        internal void AddAttribute(string name, string value) {
            _attributes.Add(TMPE_TagAttribute.GetInstance(name, value));
        }

        public void Open(int startIndex) {
            if(startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
            _startIndex = startIndex;
        }

        public bool IsClosed() {
            return _endIndex != INDEX_UNSETTED;
        }

        internal void Close(int endIndex) {
            if(_startIndex == INDEX_UNSETTED) throw new InvalidOperationException("Tag is not opened");
            if(endIndex < 0) throw new ArgumentOutOfRangeException(nameof(endIndex));
            _endIndex = endIndex;
        }

        internal void CloseIfUnclosed(int endIndex) {
            if(IsClosed() == false) Close(endIndex);
        }

        public bool ContainsIndex(int index) {
            if(IsClosed() == false) throw new InvalidOperationException("Tag needs to be closed");

            return _startIndex <= index && index <= _endIndex;
        }

        public TMPE_TagAttribute FindAttribute(string name) => _attributes.Find(x => x.Name == name);

        private void Clear() {
            _name = null;
            _value = null;
            _startIndex = INDEX_UNSETTED;
            _endIndex = INDEX_UNSETTED;
            ClearAttributes();
        }

        private void ClearAttributes() {
            foreach(TMPE_TagAttribute attribute in _attributes) {
                TMPE_TagAttribute.ReleaseInstance(attribute);
            }
            _attributes.Clear();
        }
    }
}