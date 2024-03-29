using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    public class TMPE_TextPreprocessor {
        private TMPE_TextBuffer _source = new TMPE_TextBuffer();
        public TMPE_TextBuffer Source => _source;

        private TMPE_TextBuffer _destination = new TMPE_TextBuffer();
        public TMPE_TextBuffer Destination => _destination;

        private enum TagType {Basic, Typing, TypingEvent, TypeWriterControl}

        public void ProcessText(TMPE_EffectorBase effector) {
            TMPE_EffectContainer effectContainer = effector.EffectContainer;
            TMPE_TagContainer tagContainer = effector.TagContainer;
            List<TMPE_TypeWriterSetting> typeWriterSettings = effector.TypeWriterSettings;

            tagContainer.Clear();
            tagContainer.PrepareTagLists(effector.TypeWriterSettings.Count);
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
                                    int typeWriterIndex = _source[contentStartIndex + 2] - '0';
                                    if(0 <= typeWriterIndex && typeWriterIndex <= 9 && contentLength >= 4) {
                                        tag = tagContainer.FindLastUnclosedTypingTag(typeWriterIndex, _source.Array, contentStartIndex + 3, contentEndindex);
                                    }
                                    else {
                                        tag = tagContainer.FindLastUnclosedTypingTag(0, _source.Array, contentStartIndex + 2, contentEndindex);
                                    }
                                }
                                else if(contentLength >= 3 && _source[contentStartIndex + 1] == '!') {
                                    int typeWriterIndex = _source[contentStartIndex + 2] - '0';
                                    if(0 <= typeWriterIndex && typeWriterIndex <= 9 && contentLength >= 4) {
                                        tag = tagContainer.FindLastUnclosedTypingEventTag(typeWriterIndex, _source.Array, contentStartIndex + 3, contentEndindex);
                                    }
                                    else {
                                        tag = tagContainer.FindLastUnclosedTypingEventTag(0, _source.Array, contentStartIndex + 2, contentEndindex);
                                    }
                                }
                                else if(contentLength >= 3 && _source[contentStartIndex + 1] == '?') {
                                    int typeWriterIndex = _source[contentStartIndex + 2] - '0';
                                    if(0 <= typeWriterIndex && typeWriterIndex <= 9 && contentLength >= 4) {
                                        tag = tagContainer.FindLastUnclosedTypeWriterControlTag(typeWriterIndex, _source.Array, contentStartIndex + 3, contentEndindex);
                                    }
                                    else {
                                        tag = tagContainer.FindLastUnclosedTypeWriterControlTag(0, _source.Array, contentStartIndex + 2, contentEndindex);
                                    }
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
                            int typeWriterIndex = -1;
                            for(int i = contentStartIndex; i <= contentEndindex; i++) {
                                if(state == 0) {
                                    if(_source[i] == ' ') {
                                        if(i == startIndex || i == contentEndindex) break;

                                        string matchedTagName = GetMatchedTagId(typeWriterSettings, effectContainer, _source.Array, startIndex, i - 1, out tagType, out typeWriterIndex);

                                        if(matchedTagName == null) break;

                                        tag = TMPE_Tag.Get(matchedTagName);
                                        state = 2;
                                        startIndex = i + 1;
                                    }
                                    else if(_source[i] == '=') {
                                        if(i == startIndex || i == contentEndindex) break;

                                        string matchedTagName = GetMatchedTagId(typeWriterSettings, effectContainer, _source.Array, startIndex, i - 1, out tagType, out typeWriterIndex);

                                        if(matchedTagName == null) break;

                                        tag = TMPE_Tag.Get(matchedTagName);
                                        state = 1;
                                        startIndex = i + 1;
                                    }
                                    else if(i == contentEndindex) {
                                        string matchedTagName = GetMatchedTagId(typeWriterSettings, effectContainer, _source.Array, startIndex, i, out tagType, out typeWriterIndex);

                                        if(matchedTagName == null) break;

                                        tag = TMPE_Tag.Get(matchedTagName);
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
                                    else if(tagType == TagType.Typing) {
                                        if(typeWriterIndex != -1) {
                                            TMPE_TypeWriterBase typeWriter = typeWriterSettings[typeWriterIndex]?.TypeWriter;
                                            if(typeWriter != null && typeWriter.IsValidTypingTag(tag)) {
                                                tag.StartIndex = _destination.Length;
                                                tagContainer.TypingTags[typeWriterIndex].Add(tag);
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
                                    else if(tagType == TagType.TypingEvent) {
                                        if(typeWriterIndex != -1) {
                                            TMPE_TypeWriterBase typeWriter = typeWriterSettings[typeWriterIndex]?.TypeWriter;
                                            if(typeWriter != null && typeWriter.IsValidTypingEventTag(tag)) {
                                                tag.StartIndex = _destination.Length;
                                                tagContainer.TypingEventTags[typeWriterIndex].Add(tag);
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
                                    else if(tagType == TagType.TypeWriterControl) {
                                        if(typeWriterIndex != -1) {
                                            TMPE_TypeWriterBase typeWriter = typeWriterSettings[typeWriterIndex]?.TypeWriter;
                                            if(typeWriter != null && typeWriter.IsValidTypeWriterControlTag(tag)) {
                                                tag.StartIndex = _destination.Length;
                                                tagContainer.TypeWriterControlTags[typeWriterIndex].Add(tag);
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

        private string GetMatchedTagId(List<TMPE_TypeWriterSetting> typeWriterSettings, TMPE_EffectContainer effectContainer, char[] source, int startIndex, int endIndex, out TagType tagType, out int typeWriterIndex) {
            tagType = TagType.Basic;
            typeWriterIndex = -1;
            int length = endIndex - startIndex + 1;
            if(_source[startIndex] == '+') {
                tagType = TagType.Typing;
                if(length < 2) return null;

                int typeWriterIndexCheck = source[startIndex + 1] - '0';
                if(0 <= typeWriterIndexCheck && typeWriterIndexCheck < typeWriterSettings.Count && length >= 3) {
                    typeWriterIndex = typeWriterIndexCheck;
                    return FindMatchedTypingEffectTagName(typeWriterSettings[typeWriterIndex].TypeWriter, _source.Array, startIndex + 2, endIndex);
                }
                else if(typeWriterSettings.Count > 0){
                    typeWriterIndex = 0;
                    return FindMatchedTypingEffectTagName(typeWriterSettings[typeWriterIndex].TypeWriter, _source.Array, startIndex + 1, endIndex);
                }
                else {
                    return null;
                }
            }
            else if(_source[startIndex] == '!') {
                tagType = TagType.TypingEvent;
                if(length < 2) return null;

                int typeWriterIndexCheck = source[startIndex + 1] - '0';
                if(0 <= typeWriterIndex && typeWriterIndex < typeWriterSettings.Count && length >= 3) {
                    typeWriterIndex = typeWriterIndexCheck;
                    return FindMatchedTypingEventEffectTagName(typeWriterSettings[typeWriterIndex].TypeWriter, _source.Array, startIndex + 2, endIndex);
                }
                else if(typeWriterSettings.Count > 0){
                    typeWriterIndex = 0;
                    return FindMatchedTypingEventEffectTagName(typeWriterSettings[typeWriterIndex].TypeWriter, _source.Array, startIndex + 1, endIndex);
                }
                else {
                    return null;
                }
            }
            else if(_source[startIndex] == '?') {
                tagType = TagType.TypeWriterControl;
                if(length < 2) return null;

                int typeWriterIndexCheck = source[startIndex + 1] - '0';
                if(0 <= typeWriterIndex && typeWriterIndex < typeWriterSettings.Count && length >= 3) {
                    typeWriterIndex = typeWriterIndexCheck;
                    return FindMatchedTypeWriterControlTagName(typeWriterSettings[typeWriterIndex].TypeWriter, _source.Array, startIndex + 2, endIndex);
                }
                else if(typeWriterSettings.Count > 0){
                    typeWriterIndex = 0;
                    return FindMatchedTypeWriterControlTagName(typeWriterSettings[typeWriterIndex].TypeWriter, _source.Array, startIndex + 1, endIndex);
                }
                else {
                    return null;
                }
            }
            else {
                return FindMatchedBasicEffectTagName(effectContainer.BasicEffects, _source.Array, startIndex, endIndex);
            }
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

        private string FindMatchedTypingEffectTagName(TMPE_TypeWriterBase typeWriter, char[] array, int arrayStartIndex, int arrayEndIndex) {
            if(typeWriter == null) return null;

            for(int i = 0; i < typeWriter.TypingEffects.Count; i++) {
                TMPE_TypingEffect effect = typeWriter.TypingEffects[i];
                if(effect.TagName.EqualsPartialCharArray(array, arrayStartIndex, arrayEndIndex)) {
                    return effect.TagName;
                }
            }
            return null;
        }

        private string FindMatchedTypingEventEffectTagName(TMPE_TypeWriterBase typeWriter, char[] array, int arrayStartIndex, int arrayEndIndex) {
            if(typeWriter == null) return null;

            for(int i = 0; i < typeWriter.TypingEventEffects.Count; i++) {
                TMPE_TypingEventEffect effect = typeWriter.TypingEventEffects[i];
                if(effect.TagName.EqualsPartialCharArray(array, arrayStartIndex, arrayEndIndex)) {
                    return effect.TagName;
                }
            }
            return null;
        }

        private string FindMatchedTypeWriterControlTagName(TMPE_TypeWriterBase typeWriter, char[] array, int arrayStartIndex, int arrayEndIndex) {
            if(typeWriter == null) return null;

            for(int i = 0; i < typeWriter.AcceptableTagNames.Length; i++) {
                if(typeWriter.AcceptableTagNames[i].EqualsPartialCharArray(array, arrayStartIndex, arrayEndIndex)) {
                    return typeWriter.AcceptableTagNames[i];
                }
            }
            return null;
        }
    }
}
