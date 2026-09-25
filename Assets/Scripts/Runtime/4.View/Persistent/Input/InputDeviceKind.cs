namespace KillChord.Runtime.View.Persistent.Input
{
    /// <summary>
    ///     操作案内の表示を切り替えるための入力機器の種類です。
    /// </summary>
    public enum InputDeviceKind
    {
        /// <summary> キーボードとマウスです。 </summary>
        Keyboard,

        /// <summary> Xboxコントローラーと、種類を判別できないゲームパッドです。 </summary>
        Xbox,

        /// <summary> DualShockとDualSenseです。 </summary>
        PlayStation,

        /// <summary> Nintendo Switch Proコントローラーです。 </summary>
        Switch,
    }
}
