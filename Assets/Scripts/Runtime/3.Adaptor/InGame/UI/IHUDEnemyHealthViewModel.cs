namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     HUD に表示する敵 HP の ViewModel。
    /// </summary>
    public interface IHUDEnemyHealthViewModel
    {
        /// <summary>
        ///     敵 HP の表示を更新する。
        /// </summary>
        void Update(in HUDEnemyHealthDTO dto);
    }
}
