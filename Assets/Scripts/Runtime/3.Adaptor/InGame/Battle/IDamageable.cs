namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     ダメージを受けることが可能なオブジェクトを表すインターフェース。
    /// </summary>
    public interface IDamageable
    {
        /// <summary> プレイヤー攻撃コントローラーを取得する。 </summary>
        public PlayerAttackController PlayerAttackController { get; }
    }
}
