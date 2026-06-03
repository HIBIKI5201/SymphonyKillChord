namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// キャラクターアニメーションの再生インデックスをまとめた設定。
    /// Composition で Domain enum から解決され、View へ渡される。
    /// </summary>
    public sealed class CharacterAnimationIndices
    {
        /// <param name="attack">アニメーションの再生インデックス</param>
        public CharacterAnimationIndices(int attack, int dodge, int damage = -1)
        {
            Attack = attack;
            Dodge = dodge;
            Damage = damage;
        }
        public int Attack { get; }
        public int Dodge { get; }
        public int Damage { get; }

    }
}
