using Mock.MusicBattle.Basis;
using System;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     リズムパターンを定義するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(RythemPatternData), menuName = EditorConstraint.CREATE_ASSET_PATH + nameof(RythemPatternData))]
    public class RythemPatternData : ScriptableObject
    {
        #region Publicプロパティ
        /// <summary> 指定されたインデックスの拍子パターンを取得します。 </summary>
        public float this[int index] => _signaturePattern[index];
        /// <summary> 拍子パターンの読み取り専用スパンを取得します。 </summary>
        public ReadOnlySpan<float> SignaturePattern => _signaturePattern;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     指定された入力がこのリズムパターンに一致するかどうかを判断します。
        /// </summary>
        /// <param name="input">比較する入力のシーケンス。</param>
        /// <returns>パターンが一致する場合はtrue、それ以外はfalse。</returns>
        public bool IsMatch(ReadOnlySpan<float> input)
        {
            if (_signaturePattern.Length < MIN_PATTERN_LENGTH) { return false; }
            if (input.Length < _signaturePattern.Length) { return false; }

            // 直近の入力からパターンを取得。
            ReadOnlySpan<float> pattern = input.Slice(0, _signaturePattern.Length);
            return pattern.SequenceEqual(_signaturePattern);
        }
        #endregion

        #region 定数
        private const int MIN_PATTERN_LENGTH = 1;
        #endregion

        #region インスペクター表示フィールド
        [SerializeField, Tooltip("拍子のパターン。")]
        private float[] _signaturePattern = { 4, 4, 4, 4 };
        #endregion
    }
}

