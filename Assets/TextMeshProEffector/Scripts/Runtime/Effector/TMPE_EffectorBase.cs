using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TextMeshProEffector {
    [ExecuteAlways, RequireComponent(typeof(TMP_Text)), DisallowMultipleComponent]
    public abstract class TMPE_EffectorBase : MonoBehaviour {
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
                    _forceUpdateInEditing = EditorPrefs.GetBool(_forceUpdateInEditingSessionStateKey, false);
                    _forceUpdateInEditingIsUsed = true;
                }
                return _forceUpdateInEditing;
            }
            set {
                _forceUpdateInEditing = value;
                EditorPrefs.SetBool(_forceUpdateInEditingSessionStateKey, value);
            }
        }
#endif
        // テキスト設定時の文字のタイピング状態
        [SerializeField] protected CharacterTypingState _defaultTypingState = CharacterTypingState.Typed;

        // 演出データ類
        [SerializeField] protected List<TMPE_BasicEffectContainer> _basicEffectContainers = new List<TMPE_BasicEffectContainer>();
        public IReadOnlyList<TMPE_BasicEffectContainer> BasicEffectContainers => _basicEffectContainers;

        [SerializeField] protected List<TMPE_TypeWriter> _typeWriters = new List<TMPE_TypeWriter>();
        public List<TMPE_TypeWriter> TypeWriters => _typeWriters;
        
        // TMP
        protected TMP_Text _textComponent;
        public TMP_Text TextComponent => _textComponent;

        [NonSerialized] protected TMP_TextInfo _textInfo;
        public TMP_TextInfo TextInfo => _textInfo;
        
        // 元々の頂点情報のキャッシュ
        protected TMPE_VertexDataStorage _originalVertexData = new TMPE_VertexDataStorage();

        // Tag
        protected TMPE_TagContainer _basicTagContainer = new TMPE_TagContainer();
        public TMPE_TagContainer BasicTagContainer => _basicTagContainer;

        // タイピング関係の状態
        protected float _elapsedTimeFromTextChanged;
        public float ElapsedTimeFromTextChanged => _elapsedTimeFromTextChanged;

        protected TMPE_CharacterTypingInfo[] _typingInfo;
        public TMPE_CharacterTypingInfo[] TypingInfo => _typingInfo;

        protected float _typingPauseTimer;
        public float TypingPauseTimer {
            get => _typingPauseTimer;
            set => _typingPauseTimer = value;
        }

        // 処理フロー制御用のフラグ
        protected bool _skipOnTextChanged; // TMPro_EventManager.TEXT_CHANGED_EVENTの再帰呼び出しによる無限ループ回避用
        protected bool _setTextCalled; // 特定のフローで重めの処理が2重で行われてしまうのを回避

        // テキスト成形+タグ抽出のモジュール
        protected TMPE_TextPreprocessor _textPreprocessor = new TMPE_TextPreprocessor();

        // イベント
        [SerializeField] private protected OneShotUnityEvent _onTypingCompleted = new OneShotUnityEvent();
        /// <summary>タイピング終了時コールバック</summary>
        public UnityEvent OnTypingCompleted => _onTypingCompleted.UnityEvent;

        [NonSerialized] private bool _isInitialized;


        // Unityイベント関数
        protected virtual void Awake() {
            _textComponent = GetComponent<TMP_Text>();
        }

        // Unityイベント関数
        protected virtual void OnEnable() {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);

            // コンパイル直後にはAwakeが呼ばれないため、エディタ上での動作を維持するためにOnEnable内で初期化
            InitializeIfNotInitialized();
        }

        // Unityイベント関数
        protected virtual void OnDisable() {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        // Unityイベント関数
        protected virtual void Start() {
            InitializeIfNotInitialized();
        }

        // Unityイベント関数
        protected virtual void LateUpdate() {
            // this.SetTextを使用したフレームではOnTextChanged経由でUpdateGeometryが呼ばれるのでスキップ
            // 時間経過系の処理もスキップ(テキスト設定時のフレームは時間=0で処理の方針)
            if(_setTextCalled) return;

            // 時間経過
            _elapsedTimeFromTextChanged += Time.deltaTime;

            // 自動タイプ
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.StartAutoTypingIfNeed();
            }

            // タイピング状態の更新
            _typingPauseTimer = Mathf.Max(_typingPauseTimer - Time.deltaTime, 0);
            if(_typingPauseTimer == 0) {
                foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                    typeWriter?.UpdateTyping();
                }
            }

            UpdateGeometry();

            // タイピング完了イベントの発火
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.InvokeOnTypingCompletedIfNeed();
            }
            bool allTypeWriterFinished = true;
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null) continue;
                allTypeWriterFinished &= typeWriter.IsFinished();
            }
            if(_onTypingCompleted.IsInvoked == false) {
                if(allTypeWriterFinished) {
                    _onTypingCompleted.Invoke();
                }
            }
        }

        // Unityイベント関数
        protected virtual void OnRenderObject() {
#if UNITY_EDITOR
            if(Application.isPlaying == false) {
                if(IsEditing && ForceUpdateInEditing) {
                    EditorApplication.QueuePlayerLoopUpdate();
                    SceneView.RepaintAll();
                }
            }
#endif
        }

        // 初期化処理
        protected void InitializeIfNotInitialized() {
            if(_isInitialized) return;
            Initialize();
            _isInitialized = true;
        }

        protected virtual void Initialize() {
            _skipOnTextChanged = true;
            _textComponent.ForceMeshUpdate(true); // TMPro_EventManager.TEXT_CHANGED_EVENTが発火
            _textInfo = _textComponent.textInfo;
            _typingInfo = new TMPE_CharacterTypingInfo[Mathf.NextPowerOfTwo(_textInfo.characterCount)];
            ResetTypinStatus();
            _originalVertexData.StoreOriginalVertexData(_textInfo);
        }

        // TMPro_EventManager.TEXT_CHANGED_EVENTは元をたどるとCanvas.willRenderCanvasesイベントから呼び出される
        // (LateUpdateよりも遅いタイミング)
        // (※ForceMeshUpdate()を呼んだ場合はTMPro_EventManager.TEXT_CHANGED_EVENTが即時呼ばれる)
        // Canvas.willRenderCanvasesより後に実行されるライフサイクル関数が見つからないため、ジオメトリの加工はUpdateで行うが
        // テキスト変更時のフレームでは、ジオメトリ加工後にテキスト変更に起因するメッシュの再生成が行われるため
        // 1フレームだけ未加工のジオメトリが表示されてしまう
        // 対策として、テキスト変更時は再度ジオメトリ加工を行う
        protected virtual void OnTextChanged(Object obj) {
            if(obj != _textComponent) return;

            // TMP_Text.ForceMeshUpdate()を呼びたいが、TMPro_EventManager.TEXT_CHANGED_EVENTは呼びたくない場合
            if(_skipOnTextChanged) {
                _skipOnTextChanged = false;
                return;
            }

            if(_setTextCalled) {
                _setTextCalled = false;
                ResetTypinStatus();
                _originalVertexData.StoreOriginalVertexData(_textInfo);
                UpdateGeometry();
                return;
            }

            // テキスト以外のプロパティ変更時にもOnTextChangedが呼ばれる
            // メッシュが再生成され得るので頂点情報をキャッシュしなおす
            _originalVertexData.StoreOriginalVertexData(_textInfo);
            // メッシュが再生成されると加工が消えてしまうので、再度加工
            UpdateGeometry();
        }

        protected virtual void ResetTypinStatus() {
            _elapsedTimeFromTextChanged = 0;
            _typingPauseTimer = 0;
            if(_typingInfo.Length < _textInfo.characterCount) {
                Array.Resize(ref _typingInfo, Mathf.NextPowerOfTwo(_textInfo.characterCount));
            }
            for(int i = 0; i < _typingInfo.Length; i++) {
                _typingInfo[i].Reset(_defaultTypingState);
            }
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.ResetTypinRelatedValues();
            }

            _onTypingCompleted.Reset();
        }

        protected void UpdateGeometry() {
            _originalVertexData.RestoreOriginalVertexData(_textInfo);

            // 描画情報を加工
            foreach(TMPE_BasicEffectContainer effectContainer in _basicEffectContainers) {
                effectContainer?.UpdateVertex(this);
            }
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.UpdateVertex();
            }

            // 不可視に設定されている場合は不透明度を0に上書き
            for(int i = 0; i < _textInfo.characterCount; i++) {
                if(_typingInfo[i].IsTyped() == false) {
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
                _textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            }
        }

        /// <summary>テキストを設定(推奨)</summary>
        public void SetText(string text) {
            _textPreprocessor.ProcessText(this, text);
            _textComponent.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
            ResetTypinStatus();

            _setTextCalled = true;
        }

        /// <summary>テキストを設定(推奨)</summary>
        public void SetText(char[] text) {
            _textPreprocessor.ProcessText(this, text);
            _textComponent.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
            ResetTypinStatus();

            _setTextCalled = true;
        }

        public void PauseTyping() {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.PauseTyping();
            }
        }

        public void ResumeTyping() {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.ResumeTyping();
            }
        }

        public void IsPausedTypingAny() {
            bool paused = false;
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null) continue;
                paused |= typeWriter.IsPausedTyping;
            }
        }

        public void SetTypeWriterSpeed(float speed) {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null) continue;
                typeWriter.Speed = speed;
            }
        }

        public void SetTypingSpeed(float speed) {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null) continue;
                typeWriter.TypingSpeed = speed;
            }
        }

        public void SetTypingEffectSpeed(float speed) {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null) continue;
                typeWriter.TypingEffectSpeed = speed;
            }
        }

        public bool IsStartedTyping() {
            bool isStarted = false;
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null) continue;
                isStarted |= typeWriter.IsStartedTyping();
            }
            return isStarted;
        }

        public bool IsFinishedTyping() {
            bool isFinished = true;
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null) continue;
                isFinished &= typeWriter.IsFinishedTyping();
            }
            return isFinished;
        }

        public bool IsPlayingTypingEffect() {
            bool isPlaying = false;
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null || typeWriter.TypingBehaviour == null) continue;
                isPlaying |= typeWriter.IsPlayingTypingEffect;
            }
            return isPlaying;
        }
    }
}