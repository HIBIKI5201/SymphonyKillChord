using Mock.MusicBattle.Basis;
using System;
using UnityEngine;

namespace Mock.MusicBattle
{
    [CreateAssetMenu(fileName = nameof(RythemPatternData), menuName = EditorConstraint.CREATE_ASSET_PATH + nameof(RythemPatternData))]
    public class RythemPatternData : ScriptableObject
    {
        public bool IsMatch(ReadOnlySpan<float> input)
        {
            if (_signaturePattern.Length < 1) { return false; }

            // 直近の入力からパターンを取得。
            ReadOnlySpan<float> pattern = input.Slice(0, _signaturePattern.Length);
            return pattern.SequenceEqual(_signaturePattern);
        }

        [SerializeField]
        private float[] _signaturePattern;
    }
}
