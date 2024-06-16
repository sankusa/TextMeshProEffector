using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Reflection;

namespace TextMeshProEffector {
    // TMP_Textへ直接文字列を設定してもタグの抽出ができるが、
    // その場合の動作が複雑(エディタ上とビルド時で処理の流れがかなり異なる)で、微小だがアロケーションも発生する
    // 加えて特定ケースで文字列の変更が検知できない問題もある
    [ExecuteAlways, RequireComponent(typeof(TMP_Text))]
    public class TMPE_FlexibleEffector : TMPE_EffectorBase {
        // テキスト成形+タグ抽出のモジュール
#if UNITY_EDITOR
        private TMPE_TextPreprocessorForEditor _textPreprocessorForEditor;
#endif

        // TMP_Textのinternalメンバにアクセスするためのリフレクション系オブジェクトのキャッシュ
        private FieldInfo _textBackingArrayInfo;
        private FieldInfo _textBackingArray_arrayInfo;
        private FieldInfo _textBackingArray_countInfo;
        private FieldInfo _inputSourceInfo;

        // テキストの変更を検知するためのキャッシュ
        private TMPE_TextBuffer _characterInfoOld = new TMPE_TextBuffer();

        // Unityイベント関数
        protected override void OnEnable() {
#if UNITY_EDITOR
            if(_textPreprocessorForEditor == null) _textPreprocessorForEditor = new TMPE_TextPreprocessorForEditor(this, _textPreprocessor);
            _textComponent.textPreprocessor = _textPreprocessorForEditor;
#endif
            base.OnEnable();
        }

        // Unityイベント関数
        protected override void OnDisable() {
            base.OnDisable();
#if UNITY_EDITOR
            if(_textComponent.textPreprocessor == _textPreprocessorForEditor) _textComponent.textPreprocessor = null;
#endif
        }

        protected override void OnTextChanged(Object obj) {
            if(obj != _textComponent) return;

            if(_skipOnTextChanged) {
                _skipOnTextChanged = false;
                return;
            }

            if(_setTextCalled) {
                _setTextCalled = false;
                UpdateCharacterInfoOld();
                _originalVertexData.StoreOriginalVertexData(_textInfo);
                UpdateGeometry();
                return;
            }

            TMP_CharacterInfo[] characterInfo = _textInfo.characterInfo;
            
            // テキストが変更されたかチェック
            _characterInfoOld ??= new TMPE_TextBuffer();
            if(_characterInfoOld.Capacity < _textInfo.characterCount) {
                _characterInfoOld.Initialize(Mathf.NextPowerOfTwo(_textInfo.characterCount));
            }

            bool different = _textInfo.characterCount != _characterInfoOld.Length;
            int checkIndex = 0;
            // 比較
            for(; checkIndex < _textInfo.characterCount; checkIndex++) {
                if(characterInfo[checkIndex].character != _characterInfoOld[checkIndex]) {
                    different = true;
                    break;
                }
            }
            // 次の比較用にコピー
            for(; checkIndex < _textInfo.characterCount; checkIndex++) {
                _characterInfoOld[checkIndex] = characterInfo[checkIndex].character;
            }
            _characterInfoOld.Length = _textInfo.characterCount;

            // テキスト変更時
            if(different) {
                // 設定された文字列からタグを除去してタグ情報のオブジェクトを作成
                if(_textBackingArrayInfo == null) {
                    _textBackingArrayInfo = typeof(TMP_Text).GetField("m_TextBackingArray", BindingFlags.NonPublic | BindingFlags.Instance);
                    _textBackingArray_arrayInfo = _textBackingArrayInfo.FieldType.GetField("m_Array", BindingFlags.NonPublic | BindingFlags.Instance);
                    _textBackingArray_countInfo = _textBackingArrayInfo.FieldType.GetField("m_Count", BindingFlags.NonPublic | BindingFlags.Instance);
                    _inputSourceInfo = typeof(TMP_Text).GetField("m_inputSource", BindingFlags.NonPublic | BindingFlags.Instance);
                }

                // リッチテキストタグなどが除去される前のテキストを取得
                object textBackingArray = _textBackingArrayInfo.GetValue(_textComponent);
                uint[] textBackingArray_array = (uint[]) _textBackingArray_arrayInfo.GetValue(textBackingArray);
                int textBackingArray_count = (int) _textBackingArray_countInfo.GetValue(textBackingArray);

#if UNITY_EDITOR
                // internal enum TextInputSources { TextInputBox = 0, SetText = 1, SetTextArray = 2, TextString = 3 };
                // 0,3の場合はTMP_Text内部でtextPreprocessorを使って文字列がパースされる
                int inputSource = (int) _inputSourceInfo.GetValue(_textComponent);
                // ITextPreprocessorを通さないケースの場合は文字列を流し直して無理やりITextPreprocessorを通す
                if(inputSource == 1 || inputSource == 2) {
                    _textComponent.SetText(_textComponent.text);
                }
#else
                // 加工+タグ情報オブジェクト生成
                _textPreprocessor.Source.Initialize(textBackingArray_array, textBackingArray_count);
                _textPreprocessor.ProcessText(this);
                _textComponent.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
#endif

                ResetTypinStatus();

                _skipOnTextChanged = true;
                _textComponent.ForceMeshUpdate(true);

                UpdateCharacterInfoOld();
                _originalVertexData.StoreOriginalVertexData(_textInfo);
                UpdateGeometry();
            }
            // テキスト以外のプロパティ変更時
            else {
                _originalVertexData.StoreOriginalVertexData(_textInfo);
                UpdateGeometry();
            }
        }

        private void UpdateCharacterInfoOld() {
            TMP_CharacterInfo[] characterInfo = _textInfo.characterInfo;
            for(int i = 0; i < _textInfo.characterCount; i++) {
                _characterInfoOld[i] = characterInfo[i].character;
            }
            _characterInfoOld.Length = _textInfo.characterCount;
        }
    }
}