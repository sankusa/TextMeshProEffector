using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TextMeshProEffector {
    [ExecuteAlways, RequireComponent(typeof(TMP_Text))]
    public class TMPE_Effector : MonoBehaviour, IEffector {
#if UNITY_EDITOR
        /// <summary>
        /// インスペクタが表示中か
        /// </summary>
        public static bool IsEditing;

        /// <summary>
        /// インスペクタ表示中にUpdateを呼ぶか
        /// </summary>
        private static bool _forceUpdateInEditing;
        private static bool _forceUpdateInEditingIsUsed;
        private const string _forceUpdateInEditingSessionStateKey = nameof(TMPE_Effector) + "_" + nameof(ForceUpdateInEditing);
        public static bool ForceUpdateInEditing {
            get {
                if(_forceUpdateInEditingIsUsed == false) {
                    _forceUpdateInEditing = SessionState.GetBool(_forceUpdateInEditingSessionStateKey, false);
                    _forceUpdateInEditingIsUsed = true;
                }
                return _forceUpdateInEditing;
            }
            set {
                _forceUpdateInEditing = value;
                SessionState.SetBool(_forceUpdateInEditingSessionStateKey, value);
            }
        }
#endif

        [SerializeField] private TMPE_EffectContainer _effectContainer;
        public TMPE_EffectContainer EffectContainer => _effectContainer;
        
        private TMP_Text _tmpText;
        public TMP_Text TextComponent => _tmpText;
        [NonSerialized] private TMP_TextInfo _textInfo;
        private List<Color32[]> _originalColors32 = new List<Color32[]>();
        private List<Vector3[]> _originalVertices = new List<Vector3[]>();

        private TMPE_TagContainer _tagContainer = new TMPE_TagContainer();
        internal TMPE_TagContainer TagContainer => _tagContainer;

        private float _elapsedTimeFromTextChanged;
        public float ElapsedTimeFromTextChanged => _elapsedTimeFromTextChanged;

        private float _elapsedTimeFromTextChangedForTyping;
        public float ElapsedTimeFromTextChangedForTyping => _elapsedTimeFromTextChangedForTyping;

        private float[] _elapsedTimesFromTyped;
        public float[] ElapsedTimesFromTyped => _elapsedTimesFromTyped;

        private float _typingPauseTimer;
        public float TypingPauseTimer {
            get => _typingPauseTimer;
            set => _typingPauseTimer = value;
        }

        private int _nextTypingEventIndex;

        [SerializeField, Min(0)] private float _intervalPerChar;

        [NonSerialized] private string _textOld;

        private bool _skipOnTextChanged;
        private bool _setTextCalled;

        private TMPE_TextPreprocessor _textPreprocessor = new TMPE_TextPreprocessor();
#if UNITY_EDITOR
        private TMPE_TextPreprocessorForEditor _textPreprocessorForEditor;
#endif

        private FieldInfo _textBackingArrayInfo;
        private FieldInfo _textBackingArray_arrayInfo;
        private FieldInfo _textBackingArray_countInfo;
        private FieldInfo _inputSourceInfo;

        private TMPE_TextBuffer _characterInfoOld = new TMPE_TextBuffer();

        void Awake() {
            _tmpText = GetComponent<TMP_Text>();
        }

        void OnEnable() {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
#if UNITY_EDITOR
            if(_textPreprocessorForEditor == null) _textPreprocessorForEditor = new TMPE_TextPreprocessorForEditor(this, _textPreprocessor);
            _tmpText.textPreprocessor = _textPreprocessorForEditor;
#endif
            _skipOnTextChanged = true;
            _tmpText.ForceMeshUpdate(true); // TMPro_EventManager.TEXT_CHANGED_EVENTが発火
            _textInfo = _tmpText.textInfo;
            OnNewTextSetted();
            CacheVertexData();
        }

        void OnDisable() {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
#if UNITY_EDITOR
            if(_tmpText.textPreprocessor == _textPreprocessorForEditor) _tmpText.textPreprocessor = null;
#endif
        }

        void LateUpdate() {
            if(_setTextCalled) return;

            _elapsedTimeFromTextChanged += Time.deltaTime;

            _typingPauseTimer = Mathf.Max(_typingPauseTimer - Time.deltaTime, 0);
            if(_typingPauseTimer == 0) {
                _elapsedTimeFromTextChangedForTyping += Time.deltaTime;
            }

            // 表示文字数計算
            bool tmpTextPropertyChanged = false;
            int oldMaxVisibleCharacters = _tmpText.maxVisibleCharacters;
            int newMaxVisibleCharacters;
            if (_intervalPerChar == 0) {
                newMaxVisibleCharacters = _textInfo.characterCount;
            }
            else {
                newMaxVisibleCharacters = Mathf.Min(Mathf.CeilToInt(_elapsedTimeFromTextChangedForTyping / _intervalPerChar), _textInfo.characterCount);
            }

            bool maxVisibleCharactersChanged = newMaxVisibleCharacters != oldMaxVisibleCharacters;
            if(maxVisibleCharactersChanged) {
                int typeStartIndex = oldMaxVisibleCharacters;
                int typeEndIndex = newMaxVisibleCharacters - 1;
                int typeCount = 0;
                for(int i = typeStartIndex; i <= typeEndIndex; i++) {
                    if(i == _nextTypingEventIndex) {
                        _effectContainer?.ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.BeforeTyping, this, _textInfo, _tagContainer.TypingEventTags, i);
                        _nextTypingEventIndex++;
                    }
                    if(_typingPauseTimer > 0) break;
                    typeCount++;
                    _effectContainer?.ProcessTypingEvents(TMPE_TypingEventEffect.TriggerTiming.AfterTyping, this, _textInfo, _tagContainer.TypingEventTags, i);
                    if(_typingPauseTimer > 0) break;
                }
                if(typeCount > 0) {
                    _tmpText.maxVisibleCharacters = oldMaxVisibleCharacters + typeCount;
                    tmpTextPropertyChanged = true;
                }
            }

            for(int i = 0; i < Mathf.Min(_tmpText.maxVisibleCharacters, _elapsedTimesFromTyped.Length); i++) {
                _elapsedTimesFromTyped[i] += Time.deltaTime;
            }
            
            // レンダリング時にメッシュが更新されてOnTextChanged内でUpdateGeometry()が呼ばれるため
            // ここでは呼ばなくてよい
            if(tmpTextPropertyChanged) return;
            UpdateGeometry();
        }

        void OnRenderObject() {
#if UNITY_EDITOR
            if(Application.isPlaying == false) {
                if(IsEditing && ForceUpdateInEditing) {
                    EditorApplication.QueuePlayerLoopUpdate();
                    SceneView.RepaintAll();
                }
            }
#endif
        }

        // TMPro_EventManager.TEXT_CHANGED_EVENTは元をたどるとCanvas.willRenderCanvasesイベントから呼び出される
        // (LateUpdateよりも遅いタイミング)
        // (※ForceMeshUpdate()を呼んだ場合はTMPro_EventManager.TEXT_CHANGED_EVENTが即時呼ばれる)
        // Canvas.willRenderCanvasesより後に実行されるライフサイクル関数が見つからないため、ジオメトリの加工はUpdateで行うが
        // テキスト変更時のフレームでは、ジオメトリ加工後にテキスト変更に起因するメッシュの再生成が行われるため
        // 1フレームだけ未加工のジオメトリが表示されてしまう
        // 対策として、テキスト変更時は再度ジオメトリ加工を行う
        private void OnTextChanged(Object obj) {
            if(obj == _tmpText) {
                if(_skipOnTextChanged) {
                    _skipOnTextChanged = false;
                    return;
                }

                if(_setTextCalled) {
                    _setTextCalled = false;
                    UpdateCharacterInfoOld();
                    CacheVertexData();
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
                    if(_effectContainer == null) {
                        _tagContainer.Clear();
                    }
                    else {
                        if(_textBackingArrayInfo == null) {
                            _textBackingArrayInfo = typeof(TMP_Text).GetField("m_TextBackingArray", BindingFlags.NonPublic | BindingFlags.Instance);
                            _textBackingArray_arrayInfo = _textBackingArrayInfo.FieldType.GetField("m_Array", BindingFlags.NonPublic | BindingFlags.Instance);
                            _textBackingArray_countInfo = _textBackingArrayInfo.FieldType.GetField("m_Count", BindingFlags.NonPublic | BindingFlags.Instance);
                            _inputSourceInfo = typeof(TMP_Text).GetField("m_inputSource", BindingFlags.NonPublic | BindingFlags.Instance);
                        }

                        // リッチテキストタグなどが除去される前のテキストを取得
                        object textBackingArray = _textBackingArrayInfo.GetValue(_tmpText);
                        uint[] textBackingArray_array = (uint[]) _textBackingArray_arrayInfo.GetValue(textBackingArray);
                        int textBackingArray_count = (int) _textBackingArray_countInfo.GetValue(textBackingArray);

#if UNITY_EDITOR
                        // internal enum TextInputSources { TextInputBox = 0, SetText = 1, SetTextArray = 2, TextString = 3 };
                        // 0,3の場合はTMP_Text内部でtextPreprocessorを使って文字列がパースされる
                        int inputSource = (int) _inputSourceInfo.GetValue(_tmpText);
                        // ITextPreprocessorを通さないケースの場合は文字列を流し直して無理やりITextPreprocessorを通す
                        if(inputSource == 1 || inputSource == 2) {
                            _tmpText.SetText(_tmpText.text);
                        }
#else
                        // 加工+タグ情報オブジェクト生成
                        _textPreprocessor.Source.Initialize(textBackingArray_array, textBackingArray_count);
                        _textPreprocessor.ProcessText(_effectContainer, _basicTags);
                        _tmpText.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
#endif
                    }
                    // テキスト変更時共通処理
                    OnNewTextSetted();

                    _skipOnTextChanged = true;
                    _tmpText.ForceMeshUpdate(true);

                    UpdateCharacterInfoOld();
                    CacheVertexData();
                    UpdateGeometry();
                }
                else {
                    // テキスト以外のプロパティ変更時
                    CacheVertexData();
                    UpdateGeometry();
                }
                // // テキスト変更時(Canvas.willRenderCanvasesのタイミングで呼ばれる)
                // if(_tmpText.text != _textOld) {
                //     _textOld = _tmpText.text;

                //     _elapsedTimeFromTextChanged = 0;
                //     //_tmpText.maxVisibleCharacters = 0;

                //     // ForceMeshUpdate内でOnTextChangedが呼ばれてしまうので
                //     _skipOnTextChanged = true;
                //     // maxVisibleCharactersの変更を検知してメッシュが更新されるのは次フレームなので
                //     // 1フレームだけ新しい文字列が元のmaxVisibleCharactersの値のまま表示され、チラついて見える
                //     // 対策として、フレーム内2度目になってしまうがメッシュ更新処理を呼ぶ
                //     _tmpText.ForceMeshUpdate(true);
                //     CacheTextInfo();
                //     UpdateGeometry();
                // }
            }
        }

        private void OnNewTextSetted() {Debug.Log("OnNewTextSetted");
            _elapsedTimeFromTextChanged = 0;
            _elapsedTimeFromTextChangedForTyping = 0;
            _typingPauseTimer = 0;
            if(_elapsedTimesFromTyped == null || _elapsedTimesFromTyped.Length < _textInfo.characterCount) {
                _elapsedTimesFromTyped = new float[Mathf.NextPowerOfTwo(_textInfo.characterCount)];
            }
            else {
                Array.Clear(_elapsedTimesFromTyped, 0, _elapsedTimesFromTyped.Length);
            }
            _nextTypingEventIndex = 0;

            if(_intervalPerChar == 0) {
                _tmpText.maxVisibleCharacters = _textInfo.characterCount;
            }
            else {
                _tmpText.maxVisibleCharacters = 0;
            }
        }
        
        /// <summary>
        /// TMP_TextInfoの内容をキャッシュ
        /// </summary>
        private void CacheVertexData() {
            for(int i = 0; i < _textInfo.materialCount; i++) {
                TMP_MeshInfo meshInfo = _textInfo.meshInfo[i];
                if(i == _originalColors32.Count) _originalColors32.Add(new Color32[Mathf.NextPowerOfTwo(meshInfo.colors32.Length)]);
                if(_originalColors32[i].Length < meshInfo.colors32.Length) {
                    _originalColors32[i] = new Color32[Mathf.NextPowerOfTwo(meshInfo.colors32.Length)];
                }
                Color32[] currentColors32 = _originalColors32[i];
                for(int j = 0; j < meshInfo.colors32.Length; j++) {
                    currentColors32[j] = meshInfo.colors32[j];
                }
            }

            for(int i = 0; i < _textInfo.materialCount; i++) {
                TMP_MeshInfo meshInfo = _textInfo.meshInfo[i];
                if(i == _originalVertices.Count) _originalVertices.Add(new Vector3[Mathf.NextPowerOfTwo(meshInfo.vertices.Length)]);
                if(_originalVertices[i].Length < meshInfo.colors32.Length) {
                    _originalVertices[i] = new Vector3[Mathf.NextPowerOfTwo(meshInfo.vertices.Length)];
                }
                Vector3[] currentVertices = _originalVertices[i];
                for(int j = 0; j < meshInfo.vertices.Length; j++) {
                    currentVertices[j] = meshInfo.vertices[j];
                }
            }
        }

        private void UpdateGeometry() {
            // キャッシュしておいた元々の値を設定
            for(int i = 0; i < _textInfo.materialCount; i++) {
                // Array.Copy(_originalColors32[i], _textInfo.meshInfo[i].colors32, _originalColors32.Length);
                // Array.Copy(_originalVertices[i], _textInfo.meshInfo[i].vertices, _originalVertices.Length);
                Array.Copy(_originalColors32[i], _textInfo.meshInfo[i].colors32, _textInfo.meshInfo[i].colors32.Length);
                Array.Copy(_originalVertices[i], _textInfo.meshInfo[i].vertices, _textInfo.meshInfo[i].vertices.Length);
                // _originalColors32[i].CopyTo(_textInfo.meshInfo[i].colors32, 0);
                // _originalVertices[i].CopyTo(0, _textInfo.meshInfo[i].vertices, 0, _textInfo.meshInfo[i].vertices.Length);
            }

            // 描画情報を加工
            if(_effectContainer != null) {
                _effectContainer.UpdateTextInfo_BasicEffect(this, _textInfo, _tagContainer.BasicTags);
                bool isPlaying = _effectContainer.UpdateTextInfo_TypingEffect(this, _textInfo, _tagContainer.TypingTags);
            }

            // for(int i = 0; i < _textInfo.materialCount; i++) {
            //     if(_textInfo.meshInfo[i].mesh == null) continue;

            //     _textInfo.meshInfo[i].mesh.colors32 = _textInfo.meshInfo[i].colors32;
            //     _textInfo.meshInfo[i].mesh.vertices = _textInfo.meshInfo[i].vertices;
            //     _tmpText.UpdateGeometry(_textInfo.meshInfo[i].mesh, i);
            // }

            // TextMeshProUGUIインスペクタ上でのテキスト編集時、文字列長が0になった場合のみ
            // UpdateVertexDataが呼ばれるとメッシュが更新されない(詳細は未調査)
            if(_textInfo.characterCount > 0) {
                _tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            }
        }

        public void SetText(string text) {
            _textPreprocessor.Source.Initialize(text);
            _textPreprocessor.ProcessText(_effectContainer, _tagContainer);
            _tmpText.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
            OnNewTextSetted();
            // _skipOnTextChanged = true;
            // _tmpText.ForceMeshUpdate(true);
            // UpdateCharacterInfoOld();

            // CacheVertexData();
            // UpdateGeometry();
            _setTextCalled = true;
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