namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵攻撃の砲弾のViewインタフェース。
    /// </summary>
    public interface IShellView
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
        /// <summary>
        ///     着弾予告デカールの変化開始タイミングで再生するSEを鳴らす。
        /// </summary>
        public void PlayAreaWarning();
    }
}
