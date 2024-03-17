using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
namespace TextMeshProEffector {
    // エディタ上でのみ使用するTMPE_TextPreprocessorのラッパー
    // TMP_Textはテキストボックスと同期をとるためにエディタ専用の処理をしているので
    // 仕様を壊さないためにビルド時とは異なる流れでタグの抽出をする
    internal class TMPE_TextPreprocessorForEditor : ITextPreprocessor {
        private string _sourceTextCache;
        private string _processedTextCache;
        private int _effectContainerInstanceIdOld;

        private readonly TMPE_Effector _effector;
        private readonly TMPE_TextPreprocessor _textPreprocessor;

        public TMPE_TextPreprocessorForEditor(TMPE_Effector effector, TMPE_TextPreprocessor textPreprocessor) {
            _effector = effector;
            _textPreprocessor = textPreprocessor;
        }

        // TMP_Textに入力された文字列からTMP_TextInfoが作成される前に文字列を加工できる
        // 文字列の加工でゴミが出るので、呼び出し回数を減らすために結果のキャッシュを行う
        // ※char[]を引数に取る系や数値を展開する系の関数でテキストを設定した場合は呼ばれないので注意
        string ITextPreprocessor.PreprocessText(string sourcetext) {
            // Debug.Log("ProcessText");
            TMPE_EffectContainer effectContainer = _effector.EffectContainer;

            int effectContainerInstanceId = effectContainer == null ? 0 : effectContainer.GetInstanceID();
            if(sourcetext == _sourceTextCache && effectContainerInstanceId == _effectContainerInstanceIdOld) {
                return _processedTextCache;
            }

            string processedText = null;
            if(effectContainer == null) {
                processedText = sourcetext;
                _effector.TagContainer.Clear();
            }
            else {
                // 有効なタグを文字列から切り取り、タグ情報オブジェクトを作成
                _textPreprocessor.Source.Initialize(sourcetext);
                _textPreprocessor.ProcessText(_effector);
                processedText = _textPreprocessor.Destination.ToString();
            }
            // キャッシュ
            _sourceTextCache = sourcetext;
            _processedTextCache = processedText;
            _effectContainerInstanceIdOld = effectContainerInstanceId;
            return processedText;
        }
    }
}
#endif