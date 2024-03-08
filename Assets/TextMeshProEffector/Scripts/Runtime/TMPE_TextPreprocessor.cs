using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    internal class TMPE_TextPreprocessor {
        private TMPE_TextBuffer _source = new TMPE_TextBuffer();
        public TMPE_TextBuffer Source => _source;

        private TMPE_TextBuffer _destination = new TMPE_TextBuffer();
        public TMPE_TextBuffer Destination => _destination;

        private enum TagType {Basic, Typing, TypingEvent}

        public void ProcessText(TMPE_EffectContainer effectContainer, TMPE_TagContainer tagContainer) {
            List<TMPE_BasicEffect> basicEffects = effectContainer.BasicEffects;
            List<TMPE_TypingEffect> typingEffects = effectContainer.TypingEffects;
            List<TMPE_TypingEventEffect> typingEventEffects = effectContainer.TypingEventEffects;

            tagContainer.Clear();
            _destination.Initialize(_source.Length);

            bool bracketStarted = false;
            int bracketStartIndex = -1;

            for(int readIndex = 0; readIndex < _source.Length; readIndex++) {
                if(_source[readIndex] == '<') {
                    if(bracketStarted) {
                        _destination.Append(_source.Array, bracketStartIndex, readIndex - 1);
                    }
                    else {
                        bracketStarted = true;
                    }
                    bracketStartIndex = readIndex;
                }
                else if(bracketStarted && _source[readIndex] == '>') {
                    int bracketEndIndex = readIndex;
                    int contentStartIndex = bracketStartIndex + 1;
                    int contentEndindex = bracketEndIndex - 1;
                    int contentLength = contentEndindex - contentStartIndex + 1;
                    if(contentLength == 0) {
                        _destination.Append(_source.Array, bracketStartIndex, bracketEndIndex);
                        bracketStarted = false;
                        bracketStartIndex = -1;
                    }
                    else {
                        bool isValidTag = false;
                        // 終了タグ
                        if(_source[contentStartIndex] == '/') {
                            if(contentLength >= 2) {
                                TMPE_Tag tag = null;
                                if(contentLength >= 3 && _source[contentStartIndex + 1] == '+') {
                                    tag = tagContainer.FindLastUnclosedTypingTag(_source.Array, contentStartIndex + 2, contentEndindex);
                                }
                                else if(contentLength >= 3 && _source[contentStartIndex + 1] == '!') {
                                    tag = tagContainer.FindLastUnclosedTypingEventTag(_source.Array, contentStartIndex + 2, contentEndindex);
                                }
                                else {
                                    tag = tagContainer.FindLastUnclosedBasicTag(_source.Array, contentStartIndex + 1, contentEndindex);
                                }

                                if(tag != null) {
                                    tag.EndIndex = _destination.Length - 1;
                                    isValidTag = true;
                                }
                            }
                        }
                        // 開始タグ
                        else {
                            int state = 0; // 0:Name, 1:MainParamValue, 2:MultiParamName, 3:MultiParamValue
                            int startIndex = contentStartIndex;
                            TMPE_Tag tag = null;
                            string attributeName = null;
                            bool correctFormat = true;
                            TagType tagType = TagType.Basic;
                            for(int i = contentStartIndex; i <= contentEndindex; i++) {
                                if(state == 0) {
                                    if(_source[i] == ' ') {
                                        if(i == startIndex || i == contentEndindex) break;

                                        string tagName;
                                        if (_source[contentStartIndex] == '+') {
                                            if(contentLength < 2) break;
                                            tagName = FindMatchedTypingEffectTagName(typingEffects, _source.Array, startIndex + 1, i - 1);
                                            tagType = TagType.Typing;
                                        }
                                        else if (_source[contentStartIndex] == '!') {
                                            if(contentLength < 2) break;
                                            tagName = FindMatchedTypingEventEffectTagName(typingEventEffects, _source.Array, startIndex + 1, i - 1);
                                            tagType = TagType.TypingEvent;
                                        }
                                        else {
                                            tagName = FindMatchedBasicEffectTagName(basicEffects, _source.Array, startIndex, i - 1);
                                        }

                                        if(tagName == null) break;
                                        
                                        tag = TMPE_Tag.Get(tagName);
                                        
                                        state = 2;
                                        startIndex = i + 1;
                                    }
                                    else if(_source[i] == '=') {
                                        if(i == startIndex || i == contentEndindex) break;

                                        string tagName = null;
                                        if(_source[contentStartIndex] == '+') {
                                            if(contentLength < 2) break;
                                            tagName = FindMatchedTypingEffectTagName(typingEffects, _source.Array, startIndex + 1, i - 1);
                                            tagType = TagType.Typing;
                                        }
                                        else if(_source[contentStartIndex] == '!') {
                                            if(contentLength < 2) break;
                                            tagName = FindMatchedTypingEventEffectTagName(typingEventEffects, _source.Array, startIndex + 1, i - 1);
                                            tagType = TagType.TypingEvent;
                                        }
                                        else {
                                            tagName = FindMatchedBasicEffectTagName(basicEffects, _source.Array, startIndex, i - 1);
                                        }

                                        if(tagName == null) break;

                                        tag = TMPE_Tag.Get(tagName);
                                        state = 1;
                                        startIndex = i + 1;
                                    }
                                    else if(i == contentEndindex) {
                                        string tagName = null;
                                        if(_source[contentStartIndex] == '+') {
                                            if(contentLength < 2) break;
                                            tagName = FindMatchedTypingEffectTagName(typingEffects, _source.Array, startIndex + 1, i);
                                            tagType = TagType.Typing;
                                        }
                                        else if(_source[contentStartIndex] == '!') {
                                            if(contentLength < 2) break;
                                            tagName = FindMatchedTypingEventEffectTagName(typingEventEffects, _source.Array, startIndex + 1, i);
                                            tagType = TagType.TypingEvent;
                                        }
                                        else {
                                            tagName = FindMatchedBasicEffectTagName(basicEffects, _source.Array, startIndex, i);
                                        }

                                        if(tagName == null) break;

                                        tag = TMPE_Tag.Get(tagName);
                                    }
                                }
                                else if(state == 1) {
                                    if(_source[i] == ' ') {
                                        if(i == startIndex || i == contentEndindex) {
                                            correctFormat = false;
                                            break;
                                        }

                                        string value = new string(_source.Array, startIndex, i - startIndex);
                                        tag.Value = value;
                                        state = 2;
                                        startIndex = i + 1;
                                    }
                                    else if(i == contentEndindex) {
                                        string value = new string(_source.Array, startIndex, i - startIndex + 1);
                                        tag.Value = value;
                                    }
                                }
                                else if(state == 2) {
                                    if(i == contentEndindex) {
                                        correctFormat = false;
                                        break;
                                    }
                                    else if(_source[i] == ' ') {
                                        correctFormat = false;
                                        break;
                                    }
                                    else if(_source[i] == '=') {
                                        if(i == startIndex) {
                                            correctFormat = false;
                                            break;
                                        }

                                        attributeName = new string(_source.Array, startIndex, i - startIndex);
                                        state = 3;
                                        startIndex = i + 1;
                                    }
                                }
                                else if(state == 3) {
                                    if(_source[i] == ' ') {
                                        if(i == startIndex || i == contentEndindex) {
                                            correctFormat = false;
                                            break;
                                        }

                                        string value = new string(_source.Array, startIndex, i - startIndex);
                                        tag.Attributes.Add(TMPE_TagAttribute.Get(attributeName, value));
                                        state = 2;
                                        startIndex = i + 1;
                                    }
                                    else if(i == contentEndindex) {
                                        string value = new string(_source.Array, startIndex, i - startIndex + 1);
                                        tag.Attributes.Add(TMPE_TagAttribute.Get(attributeName, value));
                                    }
                                }
                            }

                            if(tag != null) {
                                if(correctFormat) {
                                    // Tagチェック
                                    if(tagType == TagType.Basic && effectContainer.IsValidBasicTag(tag)) {
                                        tag.StartIndex = _destination.Length;
                                        tagContainer.BasicTags.Add(tag);
                                        isValidTag = true;
                                    }
                                    else if(tagType == TagType.Typing && effectContainer.IsValidTypingTag(tag)) {
                                        tag.StartIndex = _destination.Length;
                                        tagContainer.TypingTags.Add(tag);
                                        isValidTag = true;
                                    }
                                    else if(tagType == TagType.TypingEvent && effectContainer.IsValidTypingEventTag(tag)) {
                                        tag.StartIndex = _destination.Length;
                                        tagContainer.TypingEventTags.Add(tag);
                                        isValidTag = true;
                                    }
                                    else {
                                        TMPE_Tag.Release(tag);
                                    }
                                }
                                else {
                                    TMPE_Tag.Release(tag);
                                }
                            }
                        }

                        // 有効なタグでなければ、読み飛ばしていたタグ部分の文字を結果に追加
                        if(isValidTag == false) {
                            _destination.Append(_source.Array, bracketStartIndex, bracketEndIndex);
                        }

                        bracketStarted = false;
                        bracketStartIndex = -1;
                    }
                }
                // タグの可能性が無ければ即時結果に追加
                else if(bracketStarted == false) {
                    _destination.Append(_source[readIndex]);
                }
            }

            // タグが開きっぱなしなら読み飛ばしていた分を追加
            if(bracketStarted) {
                _destination.Append(_source.Array, bracketStartIndex, _source.Length);
            }

            tagContainer.CloseUnclosedTags(_destination.Length - 1);
        }

        private string FindMatchedBasicEffectTagName(List<TMPE_BasicEffect> basicEffects, char[] array, int arrayStartIndex, int arrayEndIndex) {
            for(int i = 0; i < basicEffects.Count; i++) {
                TMPE_BasicEffect effect = basicEffects[i];
                if(effect.TagName.EqualsPartialCharArray(array, arrayStartIndex, arrayEndIndex)) {
                    return effect.TagName;
                }
            }
            return null;
        }

        private string FindMatchedTypingEffectTagName(List<TMPE_TypingEffect> typingEffects, char[] array, int arrayStartIndex, int arrayEndIndex) {
            for(int i = 0; i < typingEffects.Count; i++) {
                TMPE_TypingEffect effect = typingEffects[i];
                if(effect.TagName.EqualsPartialCharArray(array, arrayStartIndex, arrayEndIndex)) {
                    return effect.TagName;
                }
            }
            return null;
        }

        private string FindMatchedTypingEventEffectTagName(List<TMPE_TypingEventEffect> typingEventEffects, char[] array, int arrayStartIndex, int arrayEndIndex) {
            for(int i = 0; i < typingEventEffects.Count; i++) {
                TMPE_TypingEventEffect effect = typingEventEffects[i];
                if(effect.TagName.EqualsPartialCharArray(array, arrayStartIndex, arrayEndIndex)) {
                    return effect.TagName;
                }
            }
            return null;
        }
    }
}
