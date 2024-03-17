using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextMeshProEffector {
    public class BackgroundColorScope : GUI.Scope {
        private Color defaultColor;

        public BackgroundColorScope(Color overwriteColor) {
            defaultColor = GUI.backgroundColor;
            GUI.backgroundColor = overwriteColor;
        }

        protected override void CloseScope() {
            GUI.backgroundColor = defaultColor;
        }
    }
}