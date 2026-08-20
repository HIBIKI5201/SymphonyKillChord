namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     敵方向表示の表示状態を受け取るViewModelインターフェース。
    /// </summary>
    public interface IEnemyDirectionIndicatorViewModel
    {
        /// <summary> 使用可能な表示スロット数。 </summary>
        int Capacity { get; }

        /// <summary>
        ///     1スロット分の表示情報を更新する。
        /// </summary>
        /// <param name="dto"> 反映する表示情報。 </param>
        void Update(in EnemyDirectionIndicatorDTO dto);
    }
}
