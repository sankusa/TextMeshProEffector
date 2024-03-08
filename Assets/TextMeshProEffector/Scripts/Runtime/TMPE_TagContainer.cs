using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    public class TMPE_TagContainer {
        private List<TMPE_Tag> _basicTags = new List<TMPE_Tag>();
        internal List<TMPE_Tag> BasicTags => _basicTags;

        private List<TMPE_Tag> _typingTags = new List<TMPE_Tag>();
        internal List<TMPE_Tag> TypingTags => _typingTags;

        private List<TMPE_Tag> _typingEventTags = new List<TMPE_Tag>();
        internal List<TMPE_Tag> TypingEventTags => _typingEventTags;

        public void Clear() {
            // タグをオブジェクトプールに戻す
            foreach(TMPE_Tag tag in _basicTags) {
                TMPE_Tag.Release(tag);
            }
            _basicTags.Clear();

            foreach(TMPE_Tag tag in _typingTags) {
                TMPE_Tag.Release(tag);
            }
            _typingTags.Clear();

            foreach(TMPE_Tag tag in _typingEventTags) {
                TMPE_Tag.Release(tag);
            }
            _typingEventTags.Clear();
        }

        public TMPE_Tag FindLastUnclosedBasicTag(char[] tagName, int nameStartIndex, int nameEndIndex) {
            for(int i = _basicTags.Count - 1; i >= 0; i--) {
                TMPE_Tag currentTag = _basicTags[i];
                if(
                    currentTag.EndIndex == -1
                    && currentTag.Name.EqualsPartialCharArray(tagName, nameStartIndex, nameEndIndex)
                ) {
                    return currentTag;
                }
            }
            return null;
        }

        public TMPE_Tag FindLastUnclosedTypingTag(char[] tagName, int nameStartIndex, int nameEndIndex) {
            for(int i = _typingTags.Count - 1; i >= 0; i--) {
                TMPE_Tag currentTag = _typingTags[i];
                if(
                    currentTag.EndIndex == -1
                    && currentTag.Name.EqualsPartialCharArray(tagName, nameStartIndex, nameEndIndex)
                ) {
                    return currentTag;
                }
            }
            return null;
        }

        public TMPE_Tag FindLastUnclosedTypingEventTag(char[] tagName, int nameStartIndex, int nameEndIndex) {
            for(int i = _typingEventTags.Count - 1; i >= 0; i--) {
                TMPE_Tag currentTag = _typingEventTags[i];
                if(
                    currentTag.EndIndex == -1
                    && currentTag.Name.EqualsPartialCharArray(tagName, nameStartIndex, nameEndIndex)
                ) {
                    return currentTag;
                }
            }
            return null;
        }

        public void CloseUnclosedTags(int endIndex) {
            foreach(TMPE_Tag tag in _basicTags) {
                if(tag.EndIndex == -1) tag.EndIndex = endIndex;
            }
            foreach(TMPE_Tag tag in _typingTags) {
                if(tag.EndIndex == -1) tag.EndIndex = endIndex;
            }
        }
    }
}