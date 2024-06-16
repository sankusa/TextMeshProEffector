using System.Collections.Generic;

namespace TextMeshProEffector {
    public class TMPE_TextPreprocessor {
        private const int TYPEWRITER_INDEX_UNSETTED = -1;
        private const int BRACKET_START_INDEX_UNSETTED = -1;

        private TMPE_TextBuffer _source = new TMPE_TextBuffer();

        private TMPE_TextBuffer _destination = new TMPE_TextBuffer();
        public IReadOnlyTMPE_TextBuffer Destination => _destination;

        private enum TagType {Basic, Typing, TypingEvent, TypeWriterControl}

        public void ProcessText(TMPE_EffectorBase effector, string sourceText) {
            _source.Initialize(sourceText);
            ProcessTextInternal(effector);
        }

        public void ProcessText(TMPE_EffectorBase effector, char[] sourceText) {
            _source.Initialize(sourceText);
            ProcessTextInternal(effector);
        }

        private void ProcessTextInternal(TMPE_EffectorBase effector) {
            IReadOnlyList<TMPE_BasicEffectContainer> effectContainers = effector.BasicEffectContainers;
            TMPE_TagContainer basicTagContainer = effector.BasicTagContainer;
            List<TMPE_TypeWriter> typeWriters = effector.TypeWriters;

            basicTagContainer.Clear();
            for(int i = 0; i < effector.TypeWriters.Count; i++) {
                effector.TypeWriters[i].ClearTag();
            }
            _destination.Initialize(_source.Length);

            int bracketStartIndex = BRACKET_START_INDEX_UNSETTED;

            for(int readIndex = 0; readIndex < _source.Length; readIndex++) {
                if(_source[readIndex] == TMPE_TagSyntax.OPEN_BRACKET) {
                    if(bracketStartIndex != BRACKET_START_INDEX_UNSETTED) {
                        _destination.Append(_source.Array, bracketStartIndex, readIndex - 1);
                    }
                    bracketStartIndex = readIndex;
                }
                else if(bracketStartIndex != BRACKET_START_INDEX_UNSETTED && _source[readIndex] == TMPE_TagSyntax.CLOSE_BRACKET) {
                    int bracketEndIndex = readIndex;
                    int contentStartIndex = bracketStartIndex + 1;
                    int contentEndindex = bracketEndIndex - 1;
                    int contentLength = contentEndindex - contentStartIndex + 1;
                    if(contentLength == 0) {
                        _destination.Append(_source.Array, bracketStartIndex, bracketEndIndex);
                        bracketStartIndex = BRACKET_START_INDEX_UNSETTED;
                    }
                    else {
                        bool isValidTag = false;
                        // 終了タグ
                        if(_source[contentStartIndex] == TMPE_TagSyntax.TAG_PREFIX_CLOSE) {
                            if(contentLength >= 2) {
                                TMPE_Tag tag = null;
                                if(contentLength >= 3 && _source[contentStartIndex + 1] == TMPE_TagSyntax.TAG_PREFIX_TYPING) {
                                    int typeWriterIndex = _source[contentStartIndex + 2] - '0';
                                    if(0 <= typeWriterIndex && typeWriterIndex <= 9 && contentLength >= 4) {
                                        tag = typeWriters[typeWriterIndex].TypingTagContainer.FindLastUnclosedTag(_source.Array, contentStartIndex + 3, contentEndindex);
                                    }
                                    else {
                                        tag = typeWriters[0].TypingTagContainer.FindLastUnclosedTag(_source.Array, contentStartIndex + 2, contentEndindex);
                                    }
                                }
                                else if(contentLength >= 3 && _source[contentStartIndex + 1] == TMPE_TagSyntax.TAG_PREFIX_TYPING_EVENT) {
                                    int typeWriterIndex = _source[contentStartIndex + 2] - '0';
                                    if(0 <= typeWriterIndex && typeWriterIndex <= 9 && contentLength >= 4) {
                                        tag = typeWriters[typeWriterIndex].TypingEventTagContainer.FindLastUnclosedTag(_source.Array, contentStartIndex + 3, contentEndindex);
                                    }
                                    else {
                                        tag = typeWriters[0].TypingEventTagContainer.FindLastUnclosedTag(_source.Array, contentStartIndex + 2, contentEndindex);
                                    }
                                }
                                else if(contentLength >= 3 && _source[contentStartIndex + 1] == TMPE_TagSyntax.TAG_PREFIX_TYPEWRITER_CONTROL) {
                                    int typeWriterIndex = _source[contentStartIndex + 2] - '0';
                                    if(0 <= typeWriterIndex && typeWriterIndex <= 9 && contentLength >= 4) {
                                        tag = typeWriters[typeWriterIndex].TypeWriterControlTagContainer.FindLastUnclosedTag(_source.Array, contentStartIndex + 3, contentEndindex);
                                    }
                                    else {
                                        tag = typeWriters[0].TypeWriterControlTagContainer.FindLastUnclosedTag(_source.Array, contentStartIndex + 2, contentEndindex);
                                    }
                                }
                                else {
                                    tag = basicTagContainer.FindLastUnclosedTag(_source.Array, contentStartIndex + 1, contentEndindex);
                                }

                                if(tag != null) {
                                    tag.Close(_destination.Length - 1);
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
                            int typeWriterIndex = TYPEWRITER_INDEX_UNSETTED;
                            for(int i = contentStartIndex; i <= contentEndindex; i++) {
                                if(state == 0) {
                                    if(_source[i] == TMPE_TagSyntax.TAG_ATTRIBUTE_SEPARATOR) {
                                        if(i == startIndex || i == contentEndindex) break;

                                        string matchedTagName = GetMatchedTagId(typeWriters, effectContainers, _source.Array, startIndex, i - 1, out tagType, out typeWriterIndex);

                                        if(matchedTagName == null) break;

                                        tag = TMPE_Tag.GetInstance(matchedTagName);
                                        state = 2;
                                        startIndex = i + 1;
                                    }
                                    else if(_source[i] == TMPE_TagSyntax.ASSIGN_OPERATOR) {
                                        if(i == startIndex || i == contentEndindex) break;

                                        string matchedTagName = GetMatchedTagId(typeWriters, effectContainers, _source.Array, startIndex, i - 1, out tagType, out typeWriterIndex);

                                        if(matchedTagName == null) break;

                                        tag = TMPE_Tag.GetInstance(matchedTagName);
                                        state = 1;
                                        startIndex = i + 1;
                                    }
                                    else if(i == contentEndindex) {
                                        string matchedTagName = GetMatchedTagId(typeWriters, effectContainers, _source.Array, startIndex, i, out tagType, out typeWriterIndex);

                                        if(matchedTagName == null) break;

                                        tag = TMPE_Tag.GetInstance(matchedTagName);
                                    }
                                }
                                else if(state == 1) {
                                    if(_source[i] == TMPE_TagSyntax.TAG_ATTRIBUTE_SEPARATOR) {
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
                                    else if(_source[i] == TMPE_TagSyntax.TAG_ATTRIBUTE_SEPARATOR) {
                                        correctFormat = false;
                                        break;
                                    }
                                    else if(_source[i] == TMPE_TagSyntax.ASSIGN_OPERATOR) {
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
                                    if(_source[i] == TMPE_TagSyntax.TAG_ATTRIBUTE_SEPARATOR) {
                                        if(i == startIndex || i == contentEndindex) {
                                            correctFormat = false;
                                            break;
                                        }

                                        string value = new string(_source.Array, startIndex, i - startIndex);
                                        tag.AddAttribute(attributeName, value);
                                        state = 2;
                                        startIndex = i + 1;
                                    }
                                    else if(i == contentEndindex) {
                                        string value = new string(_source.Array, startIndex, i - startIndex + 1);
                                        tag.AddAttribute(attributeName, value);
                                    }
                                }
                            }

                            if(tag != null) {
                                if(correctFormat) {
                                    // Tagチェック
                                    if(tagType == TagType.Basic && effector.BasicEffectContainers.ValidateTag(tag)) {
                                        tag.Open(_destination.Length);
                                        basicTagContainer.AddTag(tag);
                                        isValidTag = true;
                                    }
                                    else if(tagType == TagType.Typing) {
                                        if(typeWriterIndex != TYPEWRITER_INDEX_UNSETTED) {
                                            TMPE_TypeWriter typeWriter = typeWriters[typeWriterIndex];
                                            if(typeWriter != null && typeWriter.TypingEffectContainers.ValidateTag(tag)) {
                                                tag.Open(_destination.Length);
                                                effector.TypeWriters[typeWriterIndex].TypingTagContainer.AddTag(tag);
                                                isValidTag = true;
                                            }
                                        }
                                    }
                                    else if(tagType == TagType.TypingEvent) {
                                        if(typeWriterIndex != TYPEWRITER_INDEX_UNSETTED) {
                                            TMPE_TypeWriter typeWriter = typeWriters[typeWriterIndex];
                                            if(typeWriter != null && typeWriter.TypingEventEffectContainers.ValidateTag(tag)) {
                                                tag.Open(_destination.Length);
                                                effector.TypeWriters[typeWriterIndex].TypingEventTagContainer.AddTag(tag);
                                                isValidTag = true;
                                            }
                                        }
                                    }
                                    else if(tagType == TagType.TypeWriterControl) {
                                        if(typeWriterIndex != TYPEWRITER_INDEX_UNSETTED) {
                                            TMPE_TypingBehaviourBase typingBehaviour = typeWriters[typeWriterIndex]?.TypingBehaviour;
                                            if(typingBehaviour != null && typingBehaviour.IsValidTypeWriterControlTag(tag)) {
                                                tag.Open(_destination.Length);
                                                effector.TypeWriters[typeWriterIndex].TypeWriterControlTagContainer.AddTag(tag);
                                                isValidTag = true;
                                            }
                                        }
                                    }
                                }

                                if(isValidTag == false) {
                                    TMPE_Tag.ReleaseInstance(tag);
                                }
                            }
                        }

                        // 有効なタグでなければ、読み飛ばしていたタグ部分の文字を結果に追加
                        if(isValidTag == false) {
                            _destination.Append(_source.Array, bracketStartIndex, bracketEndIndex);
                        }

                        bracketStartIndex = BRACKET_START_INDEX_UNSETTED;
                    }
                }
                // タグの可能性が無ければ即時結果に追加
                else if(bracketStartIndex == BRACKET_START_INDEX_UNSETTED) {
                    _destination.Append(_source[readIndex]);
                }
            }

            // タグが開きっぱなしなら読み飛ばしていた分を追加
            if(bracketStartIndex != BRACKET_START_INDEX_UNSETTED) {
                _destination.Append(_source.Array, bracketStartIndex, _source.Length);
            }

            // 閉じていないタグを閉じる
            basicTagContainer.CloseUnclosedTags(_destination.Length - 1);
            for(int i = 0; i < effector.TypeWriters.Count; i++) {
                effector.TypeWriters[i].TypingTagContainer.CloseUnclosedTags(_destination.Length - 1);
            }
        }

        private string GetMatchedTagId(List<TMPE_TypeWriter> typeWriters, IReadOnlyList<TMPE_BasicEffectContainer> effectContainers, char[] source, int tagNameStartIndex, int tagNameEndIndex, out TagType tagType, out int typewriterIndex) {
            tagType = TagType.Basic;
            typewriterIndex = TYPEWRITER_INDEX_UNSETTED;

            if(_source[tagNameStartIndex] == TMPE_TagSyntax.TAG_PREFIX_TYPING) {
                tagType = TagType.Typing;
                typewriterIndex = GetTypewriterIndexAndTagNamePrefixLength(typeWriters, source, tagNameStartIndex, tagNameEndIndex, out int tagNamePrefixLength);

                if(typewriterIndex != TYPEWRITER_INDEX_UNSETTED) {
                    return FindMatchedEffectTagName(typeWriters[typewriterIndex].TypingEffectContainers, _source.Array, tagNameStartIndex + tagNamePrefixLength, tagNameEndIndex);
                }

                return null;
            }
            else if(_source[tagNameStartIndex] == TMPE_TagSyntax.TAG_PREFIX_TYPING_EVENT) {
                tagType = TagType.TypingEvent;
                typewriterIndex = GetTypewriterIndexAndTagNamePrefixLength(typeWriters, source, tagNameStartIndex, tagNameEndIndex, out int tagNamePrefixLength);

                if(typewriterIndex != TYPEWRITER_INDEX_UNSETTED) {
                    return FindMatchedEffectTagName(typeWriters[typewriterIndex].TypingEventEffectContainers, _source.Array, tagNameStartIndex + tagNamePrefixLength, tagNameEndIndex);
                }

                return null;
            }
            else if(_source[tagNameStartIndex] == TMPE_TagSyntax.TAG_PREFIX_TYPEWRITER_CONTROL) {
                tagType = TagType.TypeWriterControl;
                typewriterIndex = GetTypewriterIndexAndTagNamePrefixLength(typeWriters, source, tagNameStartIndex, tagNameEndIndex, out int tagNamePrefixLength);

                if(typewriterIndex != TYPEWRITER_INDEX_UNSETTED) {
                    return typeWriters[typewriterIndex].TypingBehaviour?.FindMatchedTypeWriterControlTagName(_source.Array, tagNameStartIndex + tagNamePrefixLength, tagNameEndIndex);
                }

                return null;
            }
            else {
                return FindMatchedEffectTagName(effectContainers, _source.Array, tagNameStartIndex, tagNameEndIndex);
            }
        }

        int GetTypewriterIndexAndTagNamePrefixLength(List<TMPE_TypeWriter> typeWriters, char[] source, int tagNameStartIndex, int tagNameEndIndex, out int tagNamePrefixLength) {
            int tagNameLength = tagNameEndIndex - tagNameStartIndex + 1;
            int typewriterIndex = TYPEWRITER_INDEX_UNSETTED;
            tagNamePrefixLength = 1;
            if(tagNameLength - tagNamePrefixLength <= 0) return typewriterIndex;

            int typewriterIndexCheck = source[tagNameStartIndex + tagNamePrefixLength] - '0';
            if(0 <= typewriterIndexCheck && typewriterIndexCheck < typeWriters.Count && tagNameLength >= 3) {
                typewriterIndex = typewriterIndexCheck;
                tagNamePrefixLength++;
            }
            else if(typeWriters.Count > 0){
                typewriterIndex = 0;
            }
            return typewriterIndex;
        }

        private string FindMatchedEffectTagName<T>(IReadOnlyList<T> effectContainers, char[] charArrayContainsTag, int tagNameStartIndex, int tagNameEndIndex) where T : TMPE_EffectContainerBase {
            for(int i = 0; i < effectContainers.Count; i++) {
                T effectContainer = effectContainers[i];
                if(effectContainer == null) continue;
                string tagName = effectContainer.FindTag(charArrayContainsTag, tagNameStartIndex, tagNameEndIndex);
                if(string.IsNullOrEmpty(tagName) == false) return tagName;
            }
            return null;
        }
    }
}
