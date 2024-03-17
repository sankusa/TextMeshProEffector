using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    public class TMPE_TagContainer {
        private List<TMPE_Tag> _basicTags = new List<TMPE_Tag>();
        internal List<TMPE_Tag> BasicTags => _basicTags;

        private List<List<TMPE_Tag>> _typingTags = new List<List<TMPE_Tag>>();
        internal List<List<TMPE_Tag>> TypingTags => _typingTags;

        private List<List<TMPE_Tag>> _typingEventTags = new List<List<TMPE_Tag>>();
        internal List<List<TMPE_Tag>> TypingEventTags => _typingEventTags;

        private List<List<TMPE_Tag>> _typeWriterControlTags = new List<List<TMPE_Tag>>();
        internal List<List<TMPE_Tag>> TypeWriterControlTags => _typeWriterControlTags;

        public void PrepareTagLists(int typeWriterCount) {
            int typingTagListCreateNum = typeWriterCount - _typingTags.Count;
            if(typingTagListCreateNum > 0) {
                for(int i = 0; i < typingTagListCreateNum; i++) {
                    _typingTags.Add(new List<TMPE_Tag>());
                }
            }

            int typingEventTagListCreateNum = typeWriterCount - _typingEventTags.Count;
            if(typingEventTagListCreateNum > 0) {
                for(int i = 0; i < typingEventTagListCreateNum; i++) {
                    _typingEventTags.Add(new List<TMPE_Tag>());
                }
            }

            int typeWriterControlTagListCreateNum = typeWriterCount - _typeWriterControlTags.Count;
            if(typeWriterControlTagListCreateNum > 0) {
                for(int i = 0; i < typeWriterControlTagListCreateNum; i++) {
                    _typeWriterControlTags.Add(new List<TMPE_Tag>());
                }
            }
        }

        public void Clear() {
            // タグをオブジェクトプールに戻す
            foreach(TMPE_Tag tag in _basicTags) {
                TMPE_Tag.Release(tag);
            }
            _basicTags.Clear();

            foreach(List<TMPE_Tag> tags in _typingTags) {
                foreach(TMPE_Tag tag in tags) {
                    TMPE_Tag.Release(tag);
                }
                tags.Clear();
            }
            // _typingTags.Clear();

            foreach(List<TMPE_Tag> tags in _typingEventTags) {
                foreach(TMPE_Tag tag in tags) {
                    TMPE_Tag.Release(tag);
                }
                tags.Clear();
            }
            // _typingEventTags.Clear();

            foreach(List<TMPE_Tag> tags in _typeWriterControlTags) {
                foreach(TMPE_Tag tag in tags) {
                    TMPE_Tag.Release(tag);
                }
                tags.Clear();
            }
            // _typeWriterControlTags.Clear();
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

        public TMPE_Tag FindLastUnclosedTypingTag(int typeWriterIndex, char[] tagName, int nameStartIndex, int nameEndIndex) {
            if(typeWriterIndex < 0 || _typingTags.Count <= typeWriterIndex) return null;

            List<TMPE_Tag> tags = _typingTags[typeWriterIndex];
            for(int i = tags.Count - 1; i >= 0; i--) {
                TMPE_Tag currentTag = tags[i];
                if(
                    currentTag.EndIndex == -1
                    && currentTag.Name.EqualsPartialCharArray(tagName, nameStartIndex, nameEndIndex)
                ) {
                    return currentTag;
                }
            }
            return null;
        }

        public TMPE_Tag FindLastUnclosedTypingEventTag(int typeWriterIndex, char[] tagName, int nameStartIndex, int nameEndIndex) {
            if(typeWriterIndex < 0 || _typingEventTags.Count <= typeWriterIndex) return null;

            List<TMPE_Tag> tags = _typingEventTags[typeWriterIndex];
            for(int i = tags.Count - 1; i >= 0; i--) {
                TMPE_Tag currentTag = tags[i];
                if(
                    currentTag.EndIndex == -1
                    && currentTag.Name.EqualsPartialCharArray(tagName, nameStartIndex, nameEndIndex)
                ) {
                    return currentTag;
                }
            }
            return null;
        }

        public TMPE_Tag FindLastUnclosedTypeWriterControlTag(int typeWriterIndex, char[] tagName, int nameStartIndex, int nameEndIndex) {
            if(typeWriterIndex < 0 || _typeWriterControlTags.Count <= typeWriterIndex) return null;

            List<TMPE_Tag> tags = _typeWriterControlTags[typeWriterIndex];
            for(int i = tags.Count - 1; i >= 0; i--) {
                TMPE_Tag currentTag = tags[i];
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
            foreach(List<TMPE_Tag> tags in _typingTags) {
                foreach(TMPE_Tag tag in tags) {
                    if(tag.EndIndex == -1) tag.EndIndex = endIndex;
                }
            }
        }
    }
}