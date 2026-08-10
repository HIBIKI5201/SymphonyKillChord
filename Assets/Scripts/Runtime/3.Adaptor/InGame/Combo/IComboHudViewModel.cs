namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     コンボHUDの表示内容を更新するためのインターフェース。
    /// </summary>
    public interface IComboHudViewModel
    {
        /// <summary>
        ///    DTOを受け取って、コンボHUDの表示内容を更新します。
        /// </summary>
        /// <param name="dto"> 反映するコンボ表示用のDTOです。 </param>
        void Apply(in ComboDTO dto);
    }
}
