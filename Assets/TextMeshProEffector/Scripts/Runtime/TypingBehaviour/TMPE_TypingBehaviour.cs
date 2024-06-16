using UnityEngine;

namespace TextMeshProEffector {
    public abstract class TMPE_TypingBehaviour<Status> : TMPE_TypingBehaviourBase where Status : TMPE_TypingBehaviourStatus, new() {
        public override TMPE_TypingBehaviourStatus GenerateStatus(TMPE_TypeWriter typeWriter) {
            Status status = new Status();
            status.Reset(typeWriter);
            InitializeStatus(typeWriter, status);
            return status;
        }

        public override bool IsValidStatus(TMPE_TypingBehaviourStatus status) {
            if(status == null) return false;
            return status is Status;
        }

        public sealed override void InitializeStatus(TMPE_TypeWriter typeWriter, TMPE_TypingBehaviourStatus status) {
            InitializeStatusMain(typeWriter, status as Status);
        }
        protected virtual void InitializeStatusMain(TMPE_TypeWriter typeWriter, Status status) {}

        public sealed override void UpdateTyping(TMPE_TypeWriter typeWriter) {
            TMPE_TypingBehaviourStatus status = typeWriter.GetTypingBehaviourStatus();

            if(status == null) return;

            UpdateTypingMain(typeWriter, status as Status);
        }
        protected abstract void UpdateTypingMain(TMPE_TypeWriter typeWriter, Status status);
    }
}
