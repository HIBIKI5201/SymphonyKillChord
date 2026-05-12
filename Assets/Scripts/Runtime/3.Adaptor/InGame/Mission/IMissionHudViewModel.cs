namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     ミッションHUDの表示内容を更新するためのインターフェース。
    /// </summary>
    public interface IMissionHudViewModel
    {
        /// <summary>
        ///     DTOを受け取って、ミッションHUDの表示内容を更新します。
        /// </summary>
        /// <param name="dto"> ミッションHUDの表示内容を保持するDTO。 </param>
        public void Apply(in MissionHudDTO dto);
    }
}
