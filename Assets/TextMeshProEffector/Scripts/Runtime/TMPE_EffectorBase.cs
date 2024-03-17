using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Reflection;
using System.Linq;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TextMeshProEffector {
    [ExecuteAlways, RequireComponent(typeof(TMP_Text))]
    public abstract class TMPE_EffectorBase : MonoBehaviour, IEffector {
#if UNITY_EDITOR
        /// <summary>
        /// インスペクタが表示中か
        /// </summary>
        public static bool IsEditing;

        /// <summary>
        /// インスペクタ表示中にUpdateを呼ぶか
        /// </summary>
        protected static bool _forceUpdateInEditing;
        protected static bool _forceUpdateInEditingIsUsed;
        protected const string _forceUpdateInEditingSessionStateKey = nameof(TMPE_Effector) + "_" + nameof(ForceUpdateInEditing);
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
        //タイピング開始前から描画を行うか
        [SerializeField] protected CharacterVisiblity _defaultCharacterVisiblity = CharacterVisiblity.Invisible;

        // 演出データ類
        [SerializeField] protected TMPE_EffectContainer _effectContainer;
        public TMPE_EffectContainer EffectContainer => _effectContainer;

        [SerializeField] protected List<TMPE_TypeWriterSetting> _typeWriterSettings;
        public List<TMPE_TypeWriterSetting> TypeWriterSettings => _typeWriterSettings;
        
        // TMP
        protected TMP_Text _tmpText;
        public TMP_Text TextComponent => _tmpText;

        [NonSerialized] protected TMP_TextInfo _textInfo;
        public TMP_TextInfo TextInfo => _textInfo; 
        
        // 元々の頂点情報のキャッシュ
        protected List<Color32[]> _originalColors32 = new List<Color32[]>();
        protected List<Vector3[]> _originalVertices = new List<Vector3[]>();

        // Tag
        protected TMPE_TagContainer _tagContainer = new TMPE_TagContainer();
        public TMPE_TagContainer TagContainer => _tagContainer;

        // タイピング関係の状態
        private float _elapsedTimeFromTextChanged;
        public float ElapsedTimeFromTextChanged => _elapsedTimeFromTextChanged;

        protected TMPE_TypingInfo[] _typingInfo;
        public TMPE_TypingInfo[] TypingInfo => _typingInfo;

        protected float _typingPauseTimer;
        public float TypingPauseTimer {
            get => _typingPauseTimer;
            set => _typingPauseTimer = value;
        }

        // 処理フロー操作用のフラグ
        protected bool _skipOnTextChanged; // TMPro_EventManager.TEXT_CHANGED_EVENTの再帰呼び出しによる無限ループ回避用
        protected bool _setTextCalled; // 特定のフローで重めの処理が2重で行われてしまうのを回避

        // テキスト成形+タグ抽出のモジュール
        protected TMPE_TextPreprocessor _textPreprocessor = new TMPE_TextPreprocessor();
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

        // イベント
        [SerializeField] private OneShotUnityEvent _onTypingCompleted;
        /// <summary>タイピング終了時コールバック</summary>
        public UnityEvent OnTypingCompleted => _onTypingCompleted.UnityEvent;

        // Unityイベント関数
        void Awake() {
            _tmpText = GetComponent<TMP_Text>();
        }

        // Unityイベント関数
        void OnEnable() {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
#if UNITY_EDITOR
            if(_textPreprocessorForEditor == null) _textPreprocessorForEditor = new TMPE_TextPreprocessorForEditor(this, _textPreprocessor);
            _tmpText.textPreprocessor = _textPreprocessorForEditor;

            // コンパイル直後にはAwakeが呼ばれないため、エディタ上での動作を維持するためにOnEnable内で初期化
            if(EditorApplication.isPlaying == false) {
                Initialize();
            }
#endif
        }

        // Unityイベント関数
        void OnDisable() {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
#if UNITY_EDITOR
            if(_tmpText.textPreprocessor == _textPreprocessorForEditor) _tmpText.textPreprocessor = null;
#endif
        }

        // Unityイベント関数
        void Start() {
#if UNITY_EDITOR
            if(EditorApplication.isPlaying) {
                Initialize();
            }
#else
            Initialize();
#endif
        }

        // Unityイベント関数
        void LateUpdate() {
            // this.SetTextを使用した場合はLateUpdate時点でレンダリング時のOnTextChanged呼び出しが確定しているので、OnTextChangedで呼ばれるUpdateGeometryをここで呼ばなくて済む
            // 時間経過系の処理もスキップ(テキスト設定時のフレームは時間=0で処理の方針)
            if(_setTextCalled) return;

            // 前処理
            // タグコンテナの要素数をタイプライターの数と合わせる
            _tagContainer.PrepareTagLists(_typeWriterSettings.Count);

            // 自動タイプ
            foreach(TMPE_TypeWriterSetting typeWriterSetting in _typeWriterSettings) {
                if(typeWriterSetting.TypeWriter != null) {
                    if(typeWriterSetting.TypeWriter.IsStartedTyping(this) == false) {
                        if(typeWriterSetting.StartTypingAuto == AutoStartTypingType.Auto) {
                            typeWriterSetting.TypeWriter.StartTyping(this);
                        }
                        else if(typeWriterSetting.StartTypingAuto == AutoStartTypingType.OnOtherTypeWriterStartedTyping) {
                            TMPE_TypeWriterBase targetTypeWriter = _typeWriterSettings[typeWriterSetting.TargetTypeWriterIndex].TypeWriter;
                            if(targetTypeWriter != null) {
                                if(targetTypeWriter.GetElapsedTime(this) > typeWriterSetting.DelayFromTargetTypeWriterStartedTyping) {
                                    typeWriterSetting.TypeWriter.StartTyping(this);
                                }
                            }
                        }
                    }
                    else {
                        if(typeWriterSetting.Repeat && typeWriterSetting.RepeatInterval > 0 && typeWriterSetting.TypeWriter.GetElapsedTime(this) > typeWriterSetting.RepeatInterval) {
                            typeWriterSetting.TypeWriter.OnTextChanged(this);
                            typeWriterSetting.TypeWriter.StartTyping(this);
                        }
                    }
                }
            }

            // 時間経過
            _elapsedTimeFromTextChanged += Time.deltaTime;

            // タイピング状態の更新
            _typingPauseTimer = Mathf.Max(_typingPauseTimer - Time.deltaTime, 0);
            if(_typingPauseTimer == 0) {
                foreach(TMPE_TypeWriterSetting typeWriterSetting in _typeWriterSettings) {
                    typeWriterSetting.TypeWriter?.UpdateTyping(this);
                }
            }

            UpdateGeometry();
        }

        // Unityイベント関数
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

        // Unityイベント関数
        void OnDestroy() {
            foreach(TMPE_TypeWriterSetting typeWriterSetting in _typeWriterSettings) {
                typeWriterSetting.TypeWriter?.OnDetach(this);
            }
        }

        // 初期化処理
        private void Initialize() {
            _skipOnTextChanged = true;
            _tmpText.ForceMeshUpdate(true); // TMPro_EventManager.TEXT_CHANGED_EVENTが発火
            _textInfo = _tmpText.textInfo;
            foreach(TMPE_TypeWriterSetting typeWriterSetting in _typeWriterSettings) {
                typeWriterSetting.TypeWriter?.OnAttach(this);
            }
            ResetTypinRelatedValues();
            CacheVertexData();
        }

        /// <summary>タイピング開始</summary>
        public void StartTyping() {
            foreach(TMPE_TypeWriterSetting typeWriterSetting in _typeWriterSettings) {
                typeWriterSetting.TypeWriter?.StartTyping(this);
            }
        }

        /// <summary>タイプライターのインデックスを取得</summary>
        public int GetTypeWriterIndex(TMPE_TypeWriterBase typeWriter) {
            for(int i = 0; i < _typeWriterSettings.Count; i++) {
                if(_typeWriterSettings[i].TypeWriter == typeWriter) return i;
            }
            Debug.LogError($"TypeWriter : {typeWriter} not found");
            return -1;
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
                        _textPreprocessor.ProcessText(_effectContainer, _tagContainer);
                        _tmpText.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
#endif
                    }
                    // テキスト変更時共通処理
                    ResetTypinRelatedValues();

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
            }
        }

        private void ResetTypinRelatedValues() {
            _elapsedTimeFromTextChanged = 0;
            _typingPauseTimer = 0;
            if(_typingInfo == null || _typingInfo.Length < _textInfo.characterCount) {
                _typingInfo = new TMPE_TypingInfo[Mathf.NextPowerOfTwo(_textInfo.characterCount)];
            }
            for(int i = 0; i < _typingInfo.Length; i++) {
                _typingInfo[i].Reset(_defaultCharacterVisiblity);
            }
            foreach(TMPE_TypeWriterSetting typeWriterSetting in _typeWriterSettings) {
                typeWriterSetting.TypeWriter?.OnTextChanged(this);
            }

            _onTypingCompleted.Reset();
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
                Array.Copy(_originalColors32[i], _textInfo.meshInfo[i].colors32, _textInfo.meshInfo[i].colors32.Length);
                Array.Copy(_originalVertices[i], _textInfo.meshInfo[i].vertices, _textInfo.meshInfo[i].vertices.Length);
            }

            // 描画情報を加工
            _effectContainer?.UpdateTextInfo_BasicEffect(this);
            bool typeWriterPlaying = false;
            for(int i = 0; i < _typeWriterSettings.Count; i++) {
                TMPE_TypeWriterSetting typeWriterSetting = _typeWriterSettings[i];
                if(typeWriterSetting.TypeWriter == null) continue;
                typeWriterPlaying |= typeWriterSetting.TypeWriter.UpdateVertex(this, i);
            }

            // タイピング終了判定
            if(_onTypingCompleted.IsInvoked == false) {
                if(typeWriterPlaying == false) {
                    int unfinishedTypeWriterCount = 0;
                    for(int i = 0; i < _typeWriterSettings.Count; i++) {
                        TMPE_TypeWriterSetting typeWriterSetting = _typeWriterSettings[i];
                        if(typeWriterSetting.TypeWriter != null && typeWriterSetting.TypeWriter.IsFinishedTyping(this) == false) {
                            unfinishedTypeWriterCount++;
                        }
                    }
                    if(unfinishedTypeWriterCount == 0) _onTypingCompleted.Invoke();
                }
            }

            // 不可視に設定されている場合は不透明度を0に上書き
            for(int i = 0; i < _textInfo.characterCount; i++) {
                if(_typingInfo[i].Visiblity == CharacterVisiblity.Invisible) {
                    TMP_CharacterInfo characterInfo = _textInfo.characterInfo[i];
                    // 改行コードなどの特殊文字はvertexIndex = 0となっているのでスキップ
                    if(characterInfo.isVisible == false) continue;

                    int materialIndex = characterInfo.materialReferenceIndex;
                    Color32[] colors32 = _textInfo.meshInfo[materialIndex].colors32;
                    colors32[characterInfo.vertexIndex].a = 0;
                    colors32[characterInfo.vertexIndex + 1].a = 0;
                    colors32[characterInfo.vertexIndex + 2].a = 0;
                    colors32[characterInfo.vertexIndex + 3].a = 0;
                }
            }

            // TextMeshProUGUIインスペクタ上でのテキスト編集時、文字列長が0になった場合のみ
            // UpdateVertexDataが呼ばれるとメッシュが更新されない(詳細は未調査)
            if(_textInfo.characterCount > 0) {
                _tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            }
        }

        /// <summary>テキストを設定(推奨)</summary>
        public void SetText(string text) {
            _textPreprocessor.Source.Initialize(text);
            _textPreprocessor.ProcessText(this);
            _tmpText.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
            ResetTypinRelatedValues();

            _setTextCalled = true;
        }

        /// <summary>テキストを設定(推奨)</summary>
        public void SetText(char[] text) {
            _textPreprocessor.Source.Initialize(text);
            _textPreprocessor.ProcessText(this);
            _tmpText.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
            ResetTypinRelatedValues();

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