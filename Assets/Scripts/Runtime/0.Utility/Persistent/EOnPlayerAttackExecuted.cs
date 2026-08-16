namespace KillChord.Runtime.Utility.Persistent
{
    /// <summary>
    ///     イベント定義：プレイヤーが攻撃を実行した時
    /// </summary>
    public readonly struct EOnPlayerAttackExecuted : IEvent
    {
        /// <summary> 攻撃時にロックオン対象が存在した場合はtrue。 </summary>
        public readonly bool HasTarget;

        /// <summary>
        ///     ロックオン対象の有無を指定してイベントを生成するコンストラクタ。
        /// </summary>
        /// <param name="hasTarget"> 攻撃時にロックオン対象が存在した場合はtrue。</param>
        public EOnPlayerAttackExecuted(bool hasTarget)
        {
            HasTarget = hasTarget;
        }
    }
}
