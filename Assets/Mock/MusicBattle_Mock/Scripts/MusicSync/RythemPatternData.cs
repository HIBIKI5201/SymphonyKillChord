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
        /// <summary>
        ///     指定された入力がこのリズムパターンに一致するかどうかを判断します。
        /// </summary>
        /// <param name="input">比較する入力のシーケンス。</param>
        /// <returns>パターンが一致する場合はtrue、それ以外はfalse。</returns>
        public bool IsMatch(ReadOnlySpan<float> input)
        {
            if (_signaturePattern.Length < 1) { return false; }

            // 直近の入力からパターンを取得。
            ReadOnlySpan<float> pattern = input.Slice(0, _signaturePattern.Length);
            return pattern.SequenceEqual(_signaturePattern);
        }

        /// <summary> 拍子のパターン。 </summary>
        [SerializeField, Tooltip("拍子のパターン。")]
        private float[] _signaturePattern;
    }
}
