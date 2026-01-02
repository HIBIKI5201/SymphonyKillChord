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
        #region パブリックプロパティ
        /// <summary> 拍子情報を返す </summary>
        public SignatureData[] SignatureDataSpan => _signatureDataArray;
        #endregion

        #region パブリックメソッド
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

            // 見つからなかった場合は白色を返す。
            Debug.LogWarning($"Signature {signature} not found. Returning default color.");
            return Color.white;
        }

        /// <summary>
        ///     入力された拍子に対応するSEキュー名を取得します。
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        public string GetSeCueNameBySignature(float signature)
        {
            foreach (var data in _signatureDataArray)
            {
                if (Mathf.Approximately(data.Signature, signature))
                {
                    return data.SeCueName;
                }
            }

            // 見つからなかった場合は空文字を返す。
            Debug.LogWarning($"Signature {signature} not found. Returning empty SE cue name.");
            return string.Empty;
        }
        #endregion

        #region プライベート構造体
        [Serializable]
        public struct SignatureData
        {
            public SignatureData(float signature) : this(signature, Color.white) { }

            public SignatureData(float signature, Color color)
            {
                _signature = signature;
                _color = color;
                _seCueName = string.Empty;
            }

            public float Signature => _signature;
            public Color Color => _color;
            public string SeCueName => _seCueName;

            [SerializeField, Tooltip("対応する拍子")]
            private float _signature;
            [SerializeField, Tooltip("HUDのノーツ色")]
            private Color _color;
            [SerializeField, Tooltip("射撃SEのキュー名")]
            private string _seCueName;
        }
        #endregion

        #region インスペクター表示フィールド
        [SerializeField, Tooltip("拍子と色の関連付けデータの配列。")]
        private SignatureData[] _signatureDataArray = { new(1), new(2), new(3), new(4) };
        #endregion
    }
}
