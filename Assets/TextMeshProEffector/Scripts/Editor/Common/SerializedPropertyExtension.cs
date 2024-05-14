#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TextMeshProEffector {
    public static class SerializedPropertyExtension {
        public static object GetObject(this SerializedProperty prop) {
            string path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            string[] pathElements = path.Split('.');
            foreach(string pathElement in pathElements.Take(pathElements.Length)) {
                if(pathElement.Contains("[")) {
                    string arrayName = pathElement.Substring(0, pathElement.IndexOf("["));
                    int index = int.Parse(pathElement.Substring(pathElement.IndexOf("[")).Replace("[","").Replace("]",""));
                    obj = GetValue(obj, arrayName, index);
                }
                else {
                    obj = GetValue(obj, pathElement);
                }
            }
            return obj;
        }

        private static object GetValue(object source, string name) {
            if(source == null) return null;

            Type type = source.GetType();

            while(type != null) {
                FieldInfo fieldInfo = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if(fieldInfo != null) return fieldInfo.GetValue(source);

                PropertyInfo propertyInfo = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if(propertyInfo != null) return propertyInfo.GetValue(source, null);

                type = type.BaseType;
            }

            return null;
        }

        private static object GetValue(object source, string name, int index) {
            IEnumerable enumerable = GetValue(source, name) as IEnumerable;
            IEnumerator enumerator = enumerable.GetEnumerator();
            
            for(int i = 0; i <= index; i++) {
                enumerator.MoveNext();
            }

            return enumerator.Current;
        }
    }
}
#endif