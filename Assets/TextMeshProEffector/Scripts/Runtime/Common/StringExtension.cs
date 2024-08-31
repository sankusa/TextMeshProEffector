using System;

namespace TextMeshProEffector {
    public static class StringExtension {
        internal static bool EqualsPartialCharArray(this string search, char[] target, int targetStartIndex, int targetEndIndex) {
            if(search == null) throw new ArgumentNullException(nameof(search));
            if(target == null) throw new ArgumentNullException(nameof(target));
            if(targetStartIndex < 0 || target.Length <= targetStartIndex) throw new ArgumentOutOfRangeException(nameof(targetStartIndex));
            if(targetEndIndex < 0 || target.Length <= targetEndIndex) throw new ArgumentOutOfRangeException(nameof(targetEndIndex));
            if(targetStartIndex > targetEndIndex) throw new ArgumentException($"{nameof(targetStartIndex)} must be less than or equal to {nameof(targetEndIndex)}");
            
            // 文字数不一致の場合を弾く(後続処理でエラーになるため)
            int targetLength = targetEndIndex - targetStartIndex + 1;
            if(search.Length != targetLength) return false;

            for(int i = 0; i < search.Length; i++) {
                if(search[i] != target[targetStartIndex + i]) return false;
            }

            return true;
        }
    }
}