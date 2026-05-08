namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵攻撃の砲弾のViewModel。
    /// </summary>
    public interface IShellViewModel
    {
        /// <summary>
        ///     砲弾爆発時処理。
        /// </summary>
        public void Detonate();
        /// <summary>
        ///     爆発時プレイヤーに命中したか確認する。
        /// </summary>
        /// <returns></returns>
        public bool FindDamageTarget();
    }
}
