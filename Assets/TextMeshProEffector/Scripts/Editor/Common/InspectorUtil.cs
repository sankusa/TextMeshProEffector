using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TextMeshProEffector {
    public static class InspectorUtil {
        public static void OpenAnotherInspector(Object obj) {
            if(obj == null) {
                Debug.Log("Target object is null.");
                return;
            }
            
            Object[] originalSelections = Selection.objects;
            Selection.objects = new Object[] {obj};

            Type inspectorWindowType = Assembly.Load("UnityEditor").GetType("UnityEditor.InspectorWindow");
            EditorWindow inspectorWindow = EditorWindow.CreateInstance(inspectorWindowType) as EditorWindow;

            PropertyInfo isLockedPropertyInfo = inspectorWindowType.GetProperty("isLocked", BindingFlags.Public | BindingFlags.Instance);
            isLockedPropertyInfo.SetValue(inspectorWindow, true);

            Selection.objects = originalSelections;

            inspectorWindow.Show(true);
        }
    }
}