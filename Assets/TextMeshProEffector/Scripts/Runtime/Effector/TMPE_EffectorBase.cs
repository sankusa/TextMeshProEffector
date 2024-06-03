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
        //タイピング開始前から描画を行うか
        [SerializeField] protected CharacterVisiblity _defaultVisiblity = CharacterVisiblity.Visible;

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
        protected List<Color32[]> _originalColors32 = new List<Color32[]>();
        public IReadOnlyList<Color32[]> OriginalColors32 => _originalColors32;
        protected List<Vector3[]> _originalVertices = new List<Vector3[]>();
        public IReadOnlyList<Vector3[]> OriginalVertices => _originalVertices;

        // Tag
        protected TMPE_TagContainer _tagContainer = new TMPE_TagContainer();
        public TMPE_TagContainer TagContainer => _tagContainer;

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

        // 処理フロー操作用のフラグ
        protected bool _skipOnTextChanged; // TMPro_EventManager.TEXT_CHANGED_EVENTの再帰呼び出しによる無限ループ回避用
        protected bool _setTextCalled; // 特定のフローで重めの処理が2重で行われてしまうのを回避

        // テキスト成形+タグ抽出のモジュール
        protected TMPE_TextPreprocessor _textPreprocessor = new TMPE_TextPreprocessor();

        // イベント
        [SerializeField] private protected OneShotUnityEvent _onTypingCompleted = new OneShotUnityEvent();
        /// <summary>タイピング終了時コールバック</summary>
        public UnityEvent OnTypingCompleted => _onTypingCompleted.UnityEvent;

        // Unityイベント関数
        protected virtual void Awake() {
            _textComponent = GetComponent<TMP_Text>();
        }

        // Unityイベント関数
        protected virtual void OnEnable() {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
#if UNITY_EDITOR
            // コンパイル直後にはAwakeが呼ばれないため、エディタ上での動作を維持するためにOnEnable内で初期化
            if(EditorApplication.isPlaying == false) {
                Initialize();
            }
#endif
        }

        // Unityイベント関数
        protected virtual void OnDisable() {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        // Unityイベント関数
        protected virtual void Start() {
#if UNITY_EDITOR
            if(EditorApplication.isPlaying) {
                Initialize();
            }
#else
            Initialize();
#endif
        }

        // Unityイベント関数
        protected virtual void LateUpdate() {
            // this.SetTextを使用した場合はLateUpdate時点でレンダリング時のOnTextChanged呼び出しが確定しているので、OnTextChangedで呼ばれるUpdateGeometryをここで呼ばなくて済む
            // 時間経過系の処理もスキップ(テキスト設定時のフレームは時間=0で処理の方針)
            if(_setTextCalled) return;

            // 前処理
            // タグコンテナの要素数をタイプライターの数と合わせる
            // _tagContainer.PrepareTagLists(_typeWriters.Count);

            // 自動タイプ
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null) continue;
                if(typeWriter.TypingBehaviour != null) {
                    if(typeWriter.TypingBehaviour.IsStartedTyping(typeWriter) == false) {
                        if(typeWriter.StartTypingAuto == AutoStartTypingType.Auto) {
                            typeWriter.TypingBehaviour.StartTyping(typeWriter);
                        }
                        else if(typeWriter.StartTypingAuto == AutoStartTypingType.OnOtherTypeWriterStartedTyping) {
                            TMPE_TypeWriter.OtherTypewriterWaitSetting otherTypewriterWaitSetting = typeWriter.OtherTypewriterSetting;
                            TMPE_TypingBehaviourBase targetTypeWriter = _typeWriters[otherTypewriterWaitSetting.TargetTypeWriterIndex].TypingBehaviour;
                            if(targetTypeWriter != null) {
                                if(targetTypeWriter.GetElapsedTimeForTyping(typeWriter) > otherTypewriterWaitSetting.Delay) {
                                    typeWriter.TypingBehaviour.StartTyping(typeWriter);
                                }
                            }
                        }
                    }
                    else {
                        if(typeWriter.Repeat && typeWriter.RepeatInterval > 0 && typeWriter.TypingBehaviour.GetElapsedTimeForTyping(typeWriter) > typeWriter.RepeatInterval) {
                            typeWriter.TypingBehaviour.OnTextChanged(typeWriter);
                            typeWriter.TypingBehaviour.StartTyping(typeWriter);
                        }
                    }
                }
            }

            // 時間経過
            _elapsedTimeFromTextChanged += Time.deltaTime;

            // タイピング状態の更新
            _typingPauseTimer = Mathf.Max(_typingPauseTimer - Time.deltaTime, 0);
            if(_typingPauseTimer == 0) {
                foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                    typeWriter?.TypingBehaviour?.UpdateTyping(typeWriter);
                }
            }

            UpdateGeometry();
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

        // Unityイベント関数
        protected virtual void OnDestroy() {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.TypingBehaviour?.OnDetach(typeWriter);
            }
        }

        // 初期化処理
        protected virtual void Initialize() {
            _skipOnTextChanged = true;
            _textComponent.ForceMeshUpdate(true); // TMPro_EventManager.TEXT_CHANGED_EVENTが発火
            _textInfo = _textComponent.textInfo;
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null) continue;
                typeWriter.TypingBehaviour?.OnAttach(typeWriter);
            }
            ResetTypinRelatedValues();
            CacheVertexData();
        }

        internal bool IsValidBasicTag(TMPE_Tag tag) {
            bool isValidTypingTag = false;
            foreach(TMPE_BasicEffectContainer basicEffectContainer in _basicEffectContainers) {
                if(basicEffectContainer == null) continue;
                isValidTypingTag |= basicEffectContainer.ValidateTag(tag);
            }
            return isValidTypingTag;
        }

        // TMPro_EventManager.TEXT_CHANGED_EVENTは元をたどるとCanvas.willRenderCanvasesイベントから呼び出される
        // (LateUpdateよりも遅いタイミング)
        // (※ForceMeshUpdate()を呼んだ場合はTMPro_EventManager.TEXT_CHANGED_EVENTが即時呼ばれる)
        // Canvas.willRenderCanvasesより後に実行されるライフサイクル関数が見つからないため、ジオメトリの加工はUpdateで行うが
        // テキスト変更時のフレームでは、ジオメトリ加工後にテキスト変更に起因するメッシュの再生成が行われるため
        // 1フレームだけ未加工のジオメトリが表示されてしまう
        // 対策として、テキスト変更時は再度ジオメトリ加工を行う
        protected virtual void OnTextChanged(Object obj) {
            if(obj == _textComponent) {
                if(_skipOnTextChanged) {
                    _skipOnTextChanged = false;
                    return;
                }

                if(_setTextCalled) {
                    _setTextCalled = false;
                    ResetTypinRelatedValues();
                    CacheVertexData();
                    UpdateGeometry();
                    return;
                }

                // テキスト以外のプロパティ変更時
                ResetTypinRelatedValues();
                CacheVertexData();
                UpdateGeometry();
            }
        }

        protected virtual void ResetTypinRelatedValues() {
            // _tagContainer.PrepareTagLists(_typeWriters.Count);
            _elapsedTimeFromTextChanged = 0;
            _typingPauseTimer = 0;
            if(_typingInfo == null) {
                _typingInfo = new TMPE_CharacterTypingInfo[Mathf.NextPowerOfTwo(_textInfo.characterCount)];
            }
            else if(_typingInfo.Length < _textInfo.characterCount) {
                Array.Resize(ref _typingInfo, Mathf.NextPowerOfTwo(_textInfo.characterCount));
            }
            for(int i = 0; i < _typingInfo.Length; i++) {
                _typingInfo[i].Reset(_defaultVisiblity);
            }
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null) continue;
                typeWriter.ResetTypinRelatedValues();
                typeWriter.TypingBehaviour?.OnTextChanged(typeWriter);
            }

            _onTypingCompleted.Reset();
        }
        
        /// <summary>
        /// TMP_TextInfoの内容をキャッシュ
        /// </summary>
        protected void CacheVertexData() {
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

        protected void UpdateGeometry() {
            // キャッシュしておいた元々の値を設定
            for(int i = 0; i < _textInfo.materialCount; i++) {
                Array.Copy(_originalColors32[i], _textInfo.meshInfo[i].colors32, _textInfo.meshInfo[i].colors32.Length);
                Array.Copy(_originalVertices[i], _textInfo.meshInfo[i].vertices, _textInfo.meshInfo[i].vertices.Length);
            }

            // 描画情報を加工
            foreach(TMPE_BasicEffectContainer effectContainer in _basicEffectContainers) {
                effectContainer?.UpdateVertex(this);
            }

            bool anyTypeWriterPlaying = false;
            bool allTypeWriterFinishedTyping = true;
            for(int i = 0; i < _typeWriters.Count; i++) {
                TMPE_TypeWriter typeWriter = _typeWriters[i];
                if(typeWriter == null || typeWriter.TypingBehaviour == null) continue;
                bool currentTypeWriterPlaying = typeWriter.UpdateVertex();
                bool currentTypeWriterFinishedTyping = typeWriter.TypingBehaviour.IsFinishedTyping(typeWriter);
                if(typeWriter.OnTypingCompletedInternal.IsInvoked == false) {
                    if(currentTypeWriterPlaying == false && currentTypeWriterFinishedTyping) {
                        typeWriter.OnTypingCompletedInternal.Invoke();
                    }
                }
                anyTypeWriterPlaying |= currentTypeWriterPlaying;
                allTypeWriterFinishedTyping &= currentTypeWriterFinishedTyping;
            }

            // タイピング終了判定
            if(_onTypingCompleted.IsInvoked == false) {
                if(anyTypeWriterPlaying == false && allTypeWriterFinishedTyping) {
                    _onTypingCompleted.Invoke();
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
                _textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            }
        }

        /// <summary>テキストを設定(推奨)</summary>
        public void SetText(string text) {
            _textPreprocessor.ProcessText(this, text);
            _textComponent.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
            ResetTypinRelatedValues();

            _setTextCalled = true;
        }

        /// <summary>テキストを設定(推奨)</summary>
        public void SetText(char[] text) {
            _textPreprocessor.ProcessText(this, text);
            _textComponent.SetCharArray(_textPreprocessor.Destination.Array, 0, _textPreprocessor.Destination.Length);
            ResetTypinRelatedValues();

            _setTextCalled = true;
        }

        public void StartTyping() {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.TypingBehaviour?.StartTyping(typeWriter);
            }
        }
        public void StartTyping(int typeWriterIndex) {
            _typeWriters[typeWriterIndex].TypingBehaviour.StartTyping(_typeWriters[typeWriterIndex]);
        }

        public void PauseTyping() {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.TypingBehaviour?.PauseTyping(typeWriter);
            }
        }
        public void PauseTyping(int typeWriterIndex) {
            _typeWriters[typeWriterIndex].TypingBehaviour.PauseTyping(_typeWriters[typeWriterIndex]);
        }

        public void DelayTyping(float seconds) {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.TypingBehaviour?.DelayTyping(typeWriter, seconds);
            }
        }
        public void DelayTyping(float seconds, int typeWriterIndex) {
            _typeWriters[typeWriterIndex].TypingBehaviour.DelayTyping(_typeWriters[typeWriterIndex], seconds);
        }

        public void ResumeTyping() {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.TypingBehaviour?.ResumeTyping(typeWriter);
            }
        }
        public void ResumeTyping(int typeWriterIndex) {
            _typeWriters[typeWriterIndex].TypingBehaviour.ResumeTyping(_typeWriters[typeWriterIndex]);
        }

        public bool IsPausedTyping(int typeWriterIndex) {
            TMPE_TypeWriter typeWriter = _typeWriters[typeWriterIndex];
            if(typeWriter == null || typeWriter.TypingBehaviour == null) return false;
            return typeWriter.TypingBehaviour.IsPausedTyping(typeWriter);
        }
        public void IsPausedTypingAny() {
            bool paused = false;
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null || typeWriter.TypingBehaviour == null) continue;
                paused |= typeWriter.TypingBehaviour.IsPausedTyping(typeWriter);
            }
        }

        public void SetTypeWriterSpeed(float speed) {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.TypingBehaviour?.SetTypeWriterSpeed(typeWriter, speed);
            }
        }
        public void SetTypeWriterSpeed(int typeWriterIndex, int speed) {
            _typeWriters[typeWriterIndex].TypingBehaviour.SetTypeWriterSpeed(_typeWriters[typeWriterIndex], speed);
        }

        public void SetTypingSpeed(float speed) {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.TypingBehaviour?.SetTypingSpeed(typeWriter, speed);
            }
        }
        public void SetTypingSpeed(int typeWriterIndex, int speed) {
            _typeWriters[typeWriterIndex].TypingBehaviour.SetTypingSpeed(_typeWriters[typeWriterIndex], speed);
        }

        public void SetTypingEffectSpeed(float speed) {
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                typeWriter?.TypingBehaviour?.SetTypingEffectSpeed(typeWriter, speed);
            }
        }
        public void SetTypingEffectSpeed(int typeWriterIndex, int speed) {
            _typeWriters[typeWriterIndex].TypingBehaviour.SetTypingEffectSpeed(_typeWriters[typeWriterIndex], speed);
        }

        public bool IsStartedTyping(int typeWriterIndex) {
            TMPE_TypeWriter typeWriter = _typeWriters[typeWriterIndex];
            if(typeWriter == null || typeWriter.TypingBehaviour == null) return true;
            return typeWriter.TypingBehaviour.IsStartedTyping(typeWriter);
        }

        public bool IsFinishedTyping() {
            bool isFinished = true;
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null || typeWriter.TypingBehaviour == null) continue; 
                isFinished &= typeWriter.TypingBehaviour.IsFinishedTyping(typeWriter);
            }
            return isFinished;
        }
        public bool IsFinishedTyping(int typeWriterIndex) {
            TMPE_TypeWriter typeWriter = _typeWriters[typeWriterIndex];
            if(typeWriter.TypingBehaviour == null) return true;
            return typeWriter.TypingBehaviour.IsFinishedTyping(typeWriter);
        }

        public bool IsPlayingTypingEffect() {
            bool isPlaying = false;
            foreach(TMPE_TypeWriter typeWriter in _typeWriters) {
                if(typeWriter == null || typeWriter.TypingBehaviour == null) continue;
                isPlaying |= typeWriter.IsPlaying;
            }
            return isPlaying;
        }
        public bool IsPlayingTypingEffect(int typeWriterIndex) {
            TMPE_TypeWriter typeWriter = _typeWriters[typeWriterIndex];
            if(typeWriter.TypingBehaviour == null) return false;
            return typeWriter.IsPlaying;
        }
    }
}