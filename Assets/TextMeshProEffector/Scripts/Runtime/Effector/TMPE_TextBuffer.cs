using System.Text;
using UnityEngine;

namespace TextMeshProEffector {
    public interface IReadOnlyTMPE_TextBuffer {
        char[] Array {get;}
        int Length {get;}
        abstract string ToString();
    }

    /// <summary>
    /// バッファと文字列長をまとめて取りまわすためのオブジェクト
    /// </summary>
    public class TMPE_TextBuffer : IReadOnlyTMPE_TextBuffer {
        private char[] _array;
        public char[] Array => _array;

        private int _length = 0;
        public int Length {
            get => _length;
            set => _length = value;
        }

        public int Capacity => _array != null ? _array.Length : 0;

        public TMPE_TextBuffer() {}

        public TMPE_TextBuffer(int requiredSize) {
            _array = new char[Mathf.NextPowerOfTwo(requiredSize)];
        }

        public void Initialize(int requiredSize) {
            if(_array == null || _array.Length < requiredSize) {
                _array = new char[Mathf.NextPowerOfTwo(requiredSize)];
            }
            _length = 0;
        }

        public void Initialize(string sourceText) {
            if(sourceText == null) {
                Initialize(0);
                return;
            }

            Initialize(sourceText.Length);
            sourceText.CopyTo(0, _array, _length, sourceText.Length);
            _length = sourceText.Length;
        }

        public void Initialize(char[] sourceText) {
            if(sourceText == null) {
                Initialize(0);
                return;
            }

            Initialize(sourceText == null ? 0 : sourceText.Length);
            System.Array.Copy(sourceText, _array, sourceText.Length);
            _length = sourceText.Length;
        }

        public void Initialize(uint[] sourceText, int length = -1) {
            if(sourceText == null) {
                Initialize(0);
                return;
            }

            if(length == -1) length = sourceText.Length;
            Initialize(length);
            for(int i = 0; i < length; i++) {
                _array[i] = (char) sourceText[i];
            }
            _length = length;
        }

        public void Initialize(StringBuilder sourceText) {
            if(sourceText == null) {
                Initialize(0);
                return;
            }

            Initialize(sourceText.Length);
            sourceText.CopyTo(0, _array, _length, sourceText.Length);
            _length = sourceText.Length;
        }

        public void Resize(int size) {
            System.Array.Resize(ref _array, Mathf.NextPowerOfTwo(size));
        }

        public char this[int i] {
            get => _array[i];
            set => _array[i] = value;
        }

        public void Append(char value) {
            _array[_length] = value;
            _length++;
        }

        public void Append(char[] value, int startIndex, int endIndex) {
            int length = endIndex - startIndex + 1;
            for(int i = 0; i < length; i++) {
                _array[_length + i] = value[startIndex + i];
            }
            _length += length;
        }

        public void Clear() {
            _length = 0;
        }

        public override string ToString() {
            return new string(_array, 0, _length);
        }
    }
}