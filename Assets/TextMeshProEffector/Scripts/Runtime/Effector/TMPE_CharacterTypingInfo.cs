namespace TextMeshProEffector {
    public enum CharacterTypingState {Untyped, Typed}
    public struct TMPE_CharacterTypingInfo {
        private CharacterTypingState _typingState;
        public void Reset(CharacterTypingState typingState) {
            _typingState = typingState;
        }
        public void DoType() {
            _typingState = CharacterTypingState.Typed;
        }
        
        public bool IsTyped() {
            return _typingState == CharacterTypingState.Typed;
        }
    }
}