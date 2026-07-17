using System.Collections.Generic;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     キャラクターアニメーションの再生インデックスと再生時間を保持する。
    /// </summary>
    public sealed class CharacterAnimationPlaybackMap
    {
        /// <summary>
        ///     再生定義を初期化する。
        /// </summary>
        /// <param name="attack"> 攻撃アニメーションのインデックス。 </param>
        /// <param name="dodge"> 回避アニメーションのインデックス。 </param>
        /// <param name="clipLengths"> インデックス順のクリップ長一覧。 </param>
        /// <param name="enterBlendFrameCounts"> インデックス順の開始ブレンドフレーム数一覧。 </param>
        /// <param name="exitBlendFrameCounts"> インデックス順の終了ブレンドフレーム数一覧。 </param>
        /// <param name="damage"> ダメージアニメーションのインデックス。 </param>
        /// <param name="oneShotIndices"> ワンショットキーとインデックスの対応表。 </param>
        /// <param name="attackIndices"> 攻撃BeatTypeとインデックスの対応表。 </param>
        public CharacterAnimationPlaybackMap(
            int attack,
            int dodge,
            float[] clipLengths,
            int[] enterBlendFrameCounts,
            int[] exitBlendFrameCounts,
            int damage = -1,
            IDictionary<string, int> oneShotIndices = null,
            IDictionary<int, int> attackIndices = null)
        {
            Attack = attack;
            Dodge = dodge;
            Damage = damage;
            _clipLengths = clipLengths != null
                ? (float[])clipLengths.Clone()
                : System.Array.Empty<float>();
            _enterBlendFrameCounts = enterBlendFrameCounts != null
                ? (int[])enterBlendFrameCounts.Clone()
                : System.Array.Empty<int>();
            _exitBlendFrameCounts = exitBlendFrameCounts != null
                ? (int[])exitBlendFrameCounts.Clone()
                : System.Array.Empty<int>();
            _oneShotIndices = oneShotIndices != null
                ? new Dictionary<string, int>(oneShotIndices)
                : new Dictionary<string, int>();
            _attackIndices = attackIndices != null
                ? new Dictionary<int, int>(attackIndices)
                : new Dictionary<int, int>();
        }

        /// <summary> 攻撃アニメーションのインデックスです。 </summary>
        public int Attack { get; }

        /// <summary> 回避アニメーションのインデックスです。 </summary>
        public int Dodge { get; }

        /// <summary> ダメージアニメーションのインデックスです。 </summary>
        public int Damage { get; }

        /// <summary> ワンショットアニメーションの全件を取得する。 </summary>
        public IReadOnlyDictionary<string, int> OneShotIndices => _oneShotIndices;

        /// <summary>
        ///     任意キーに対応するワンショットアニメーションの再生インデックスを取得する。
        /// </summary>
        /// <param name="key"> アニメーションキー。 </param>
        /// <param name="index"> 取得した再生インデックス。 </param>
        /// <returns> 存在する場合はtrue。 </returns>
        public bool TryGetOneShotIndex(string key, out int index)
        {
            return _oneShotIndices.TryGetValue(key, out index);
        }

        /// <summary>
        ///     攻撃BeatTypeに対応するアニメーションの再生インデックスを取得します。
        /// </summary>
        /// <param name="attackType"> 攻撃BeatTypeです。 </param>
        /// <param name="index"> 取得した再生インデックスです。 </param>
        /// <returns> 対応する設定が存在する場合はtrueです。 </returns>
        public bool TryGetAttackIndex(int attackType, out int index)
        {
            return _attackIndices.TryGetValue(attackType, out index);
        }

        /// <summary>
        ///     指定インデックスのクリップ長を取得する。
        /// </summary>
        /// <param name="index"> 再生インデックス。 </param>
        /// <returns> クリップ長です。 </returns>
        public float GetClipLength(int index)
        {
            return index >= 0 && index < _clipLengths.Length
                ? _clipLengths[index]
                : 0f;
        }

        /// <summary>
        ///     指定インデックスの開始ブレンドフレーム数を取得する。
        /// </summary>
        /// <param name="index"> 再生インデックス。 </param>
        /// <returns> 開始ブレンドフレーム数です。 </returns>
        public int GetEnterBlendFrameCount(int index)
        {
            return index >= 0 && index < _enterBlendFrameCounts.Length
                ? _enterBlendFrameCounts[index]
                : 0;
        }

        /// <summary>
        ///     指定インデックスの終了ブレンドフレーム数を取得する。
        /// </summary>
        /// <param name="index"> 再生インデックス。 </param>
        /// <returns> 終了ブレンドフレーム数です。 </returns>
        public int GetExitBlendFrameCount(int index)
        {
            return index >= 0 && index < _exitBlendFrameCounts.Length
                ? _exitBlendFrameCounts[index]
                : 0;
        }

        /// <summary> 攻撃アニメーションの再生時間です。 </summary>
        public float AttackDuration => GetClipLength(Attack);

        /// <summary> 回避アニメーションの再生時間です。 </summary>
        public float DodgeDuration => GetClipLength(Dodge);

        private readonly float[] _clipLengths;
        private readonly int[] _enterBlendFrameCounts;
        private readonly int[] _exitBlendFrameCounts;
        private readonly Dictionary<string, int> _oneShotIndices;
        private readonly Dictionary<int, int> _attackIndices;
    }
}
