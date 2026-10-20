namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     説明ポップアップを閉じる入力の種類です。閉じる案内のアイコンを切り替えるために使います。
    /// </summary>
    public enum MissionStepPopupCloseInput
    {
        /// <summary> 案内を出さない。 </summary>
        None,
        /// <summary> UIの決定ボタン。 </summary>
        Submit,
        /// <summary> スマートフォンの画面タップ。 </summary>
        ScreenTap,
        /// <summary> 攻撃ボタン。 </summary>
        Attack,
        /// <summary> 回避ボタン。 </summary>
        Dodge,
        /// <summary> 移動入力。 </summary>
        Move,
        /// <summary> ロックオンボタン。 </summary>
        LockOn,
    }
}
