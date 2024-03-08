using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TextMeshProEffector {
    public static class StringExtension {
        public static bool EqualsPartialCharArray(this string search, char[] target, int targetStartIndex, int targetEndIndex) {
            int targetLength = targetEndIndex - targetStartIndex + 1;
            if(search.Length != targetLength) return false;

            for(int i = 0; i < search.Length; i++) {
                if(search[i] != target[targetStartIndex + i]) return false;
            }

            return true;
        }
    }
}