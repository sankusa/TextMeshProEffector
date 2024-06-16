using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    public static class TMPUtil {
        /// <summary>
        /// TMP_TextInfoの頂点情報をリストにコピー
        /// </summary>
        public static void CopyVertexData(TMP_TextInfo textInfo, List<Color32[]> colors32, List<Vector3[]> vertices) {
            // Color32
            for(int i = 0; i < textInfo.materialCount; i++) {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                // 配列数の確保
                if(i == colors32.Count) colors32.Add(new Color32[Mathf.NextPowerOfTwo(meshInfo.colors32.Length)]);
                // 配列の要素数の確保
                if(colors32[i].Length < meshInfo.colors32.Length) {
                    colors32[i] = new Color32[Mathf.NextPowerOfTwo(meshInfo.colors32.Length)];
                }

                // 値の吸い上げ
                Color32[] currentColors32 = colors32[i];
                for(int j = 0; j < meshInfo.colors32.Length; j++) {
                    currentColors32[j] = meshInfo.colors32[j];
                }
            }

            // Vertex
            for(int i = 0; i < textInfo.materialCount; i++) {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                // 配列数の確保
                if(i == vertices.Count) vertices.Add(new Vector3[Mathf.NextPowerOfTwo(meshInfo.vertices.Length)]);
                // 配列の要素数の確保
                if(vertices[i].Length < meshInfo.vertices.Length) {
                    vertices[i] = new Vector3[Mathf.NextPowerOfTwo(meshInfo.vertices.Length)];
                }

                // 値の吸い上げ
                Vector3[] currentVertices = vertices[i];
                for(int j = 0; j < meshInfo.vertices.Length; j++) {
                    currentVertices[j] = meshInfo.vertices[j];
                }
            }
        }
    }
}