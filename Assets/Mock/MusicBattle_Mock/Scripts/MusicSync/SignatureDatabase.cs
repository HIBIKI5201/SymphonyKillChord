using UnityEngine;
using Mock.MusicBattle.Basis;
using System;

namespace Mock.MusicBattle.MusicSync
{
    [CreateAssetMenu(
        fileName = nameof(SignatureDatabase),
        menuName = EditorConstraint.CREATE_ASSET_PATH + nameof(SignatureDatabase))]
    public class SignatureDatabase : ScriptableObject
    {
        public ReadOnlySpan<SignatureData> SignatureDataSpan => _signatureDataArray;

        public Color GetColorBySignature(float signature)
        {
            foreach (var data in _signatureDataArray)
            {
                if (Mathf.Approximately(data.Signature, signature))
                {
                    return data.Color;
                }
            }
            Debug.LogWarning($"Signature {signature} not found. Returning default color.");
            return Color.white;
        }

        public struct SignatureData
        {
            public float Signature => _signature;
            public Color Color => _color;

            [SerializeField, Tooltip("対応する拍子")]
            private float _signature;
            [SerializeField, Tooltip("HUDのノーツ色")]
            private Color _color;
        }

        [SerializeField, Tooltip("拍子と色の関連付けデータの配列。")]
        private SignatureData[] _signatureDataArray;
    }
}
