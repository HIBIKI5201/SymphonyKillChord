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
        /// <summary> 拍子情報を返す </summary>
        public SignatureData[] SignatureDataSpan => _signatureDataArray;

        /// <summary>
        ///     入力された拍子に対応する色を取得します。
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
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

        [Serializable]
        public struct SignatureData
        {
            public SignatureData(float signature) : this(signature, Color.white) { }

            public SignatureData(float signature, Color color)
            {
                _signature = signature;
                _color = color;
            }

            public float Signature => _signature;
            public Color Color => _color;

            [SerializeField, Tooltip("対応する拍子")]
            private float _signature;
            [SerializeField, Tooltip("HUDのノーツ色")]
            private Color _color;
        }

        [SerializeField, Tooltip("拍子と色の関連付けデータの配列。")]
        private SignatureData[] _signatureDataArray = { new(1), new(2), new(3), new(4) };
    }
}
