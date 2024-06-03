using System;
using System.Collections.Generic;

namespace TextMeshProEffector {
    public class TMPE_TagContainer {
        private List<TMPE_Tag> _tags = new List<TMPE_Tag>();
        public IReadOnlyList<TMPE_Tag> Tags => _tags;

        internal void AddTag(TMPE_Tag tag) {
            if(_tags.Contains(tag)) throw new InvalidOperationException("Tag has been already added");

            _tags.Add(tag);
        }

        public TMPE_Tag FindLastUnclosedTag(char[] tagName, int nameStartIndex, int nameEndIndex) {
            for(int i = _tags.Count - 1; i >= 0; i--) {
                TMPE_Tag tag = _tags[i];
                if(
                    tag.IsClosed() == false
                    && tag.Name.EqualsPartialCharArray(tagName, nameStartIndex, nameEndIndex)
                ) {
                    return tag;
                }
            }
            return null;
        }

        public void CloseUnclosedTags(int endIndex) {
            foreach(TMPE_Tag tag in _tags) {
                tag.CloseIfUnclosed(endIndex);
            }
        }

        public void Clear() {
            foreach(TMPE_Tag tag in _tags) {
                TMPE_Tag.ReleaseInstance(tag);
            }
            _tags.Clear();
        }
    }
}