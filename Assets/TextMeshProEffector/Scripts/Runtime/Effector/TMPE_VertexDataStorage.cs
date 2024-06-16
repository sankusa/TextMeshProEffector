using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TextMeshProEffector {
    public class TMPE_VertexDataStorage {
        protected List<Color32[]> _originalColors32 = new List<Color32[]>();
        protected List<Vector3[]> _originalVertices = new List<Vector3[]>();

        // TMP_TextInfoの元々の頂点情報を格納
        public void StoreOriginalVertexData(TMP_TextInfo textInfo) {
            // Color32
            for(int i = 0; i < textInfo.materialCount; i++) {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                // 配列数の確保
                if(i == _originalColors32.Count) _originalColors32.Add(new Color32[Mathf.NextPowerOfTwo(meshInfo.colors32.Length)]);
                // 配列の要素数の確保
                if(_originalColors32[i].Length < meshInfo.colors32.Length) {
                    _originalColors32[i] = new Color32[Mathf.NextPowerOfTwo(meshInfo.colors32.Length)];
                }

                // 値の吸い上げ
                Color32[] currentColors32 = _originalColors32[i];
                for(int j = 0; j < meshInfo.colors32.Length; j++) {
                    currentColors32[j] = meshInfo.colors32[j];
                }
            }

            // Vertex
            for(int i = 0; i < textInfo.materialCount; i++) {
                TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
                // 配列数の確保
                if(i == _originalVertices.Count) _originalVertices.Add(new Vector3[Mathf.NextPowerOfTwo(meshInfo.vertices.Length)]);
                // 配列の要素数の確保
                if(_originalVertices[i].Length < meshInfo.vertices.Length) {
                    _originalVertices[i] = new Vector3[Mathf.NextPowerOfTwo(meshInfo.vertices.Length)];
                }

                // 値の吸い上げ
                Vector3[] currentVertices = _originalVertices[i];
                for(int j = 0; j < meshInfo.vertices.Length; j++) {
                    currentVertices[j] = meshInfo.vertices[j];
                }
            }
        }

        // TMP_TextInfoの元々の頂点情報をTextInfoに復元
        public void RestoreOriginalVertexData(TMP_TextInfo textInfo) {
            for(int i = 0; i < textInfo.materialCount; i++) {
                Array.Copy(_originalColors32[i], textInfo.meshInfo[i].colors32, textInfo.meshInfo[i].colors32.Length);
                Array.Copy(_originalVertices[i], textInfo.meshInfo[i].vertices, textInfo.meshInfo[i].vertices.Length);
            }
        }
    }
}