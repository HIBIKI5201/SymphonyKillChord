using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// キャラクターアニメーションの再生インデックスをまとめた設定。
    /// Composition で Domain enum から解決され、View へ渡される。
    /// </summary>
    public sealed class CharacterAnimationIndices
    {
        /// <param name="attack">アニメーションの再生インデックス</param>
        public CharacterAnimationIndices(int attack, int dodge, int damage = -1, IDictionary<string, int> oneShotIndices = null)
        {
            Attack = attack;
            Dodge = dodge;
            Damage = damage;

            _oneShotIndices = oneShotIndices != null
                ? new Dictionary<string, int>(oneShotIndices)
                : new Dictionary<string, int>();
        }
        public int Attack { get; }
        public int Dodge { get; }
        public int Damage { get; }

        /// <summary>
        ///     任意キーに対応するワンショットアニメーションの再生インデックスを取得する。
        /// </summary>
        /// <param name="key">アニメーションキー</param>
        /// <param name="index">取得した再生インデックス</param>
        /// <returns>存在する場合はtrue</returns>
        public bool TryGetOneShotIndex(string key, out int index)
            => _oneShotIndices.TryGetValue(key, out index);

        /// <summary>
        ///     任意キーのワンショットアニメーションを追加または上書きする。
        /// </summary>
        /// <param name="key">アニメーションキー</param>
        /// <param name="index">再生インデックス</param>
        public void SetOneShotIndex(string key, int index)
        {
            _oneShotIndices[key] = index;
        }

        /// <summary>
        ///     任意キーのワンショットアニメーションを削除する。
        /// </summary>
        /// <param name="key">アニメーションキー</param>
        /// <returns>削除できた場合はtrue</returns>
        public bool RemoveOneShotIndex(string key)
            => _oneShotIndices.Remove(key);

        /// <summary>
        ///     ワンショットアニメーションの全件を取得する。
        /// </summary>
        public IReadOnlyDictionary<string, int> OneShotIndices => _oneShotIndices;

        private readonly Dictionary<string, int> _oneShotIndices;
    }
}
