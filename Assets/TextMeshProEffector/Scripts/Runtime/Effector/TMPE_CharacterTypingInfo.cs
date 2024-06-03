namespace TextMeshProEffector {
    public enum CharacterVisiblity {
        Invisible = 0,
        Visible = 1,
    }

    public struct TMPE_CharacterTypingInfo {
        public CharacterVisiblity Visiblity {get; set;}
        public void Reset(CharacterVisiblity visiblity) {
            Visiblity = visiblity;
        }
    }
}