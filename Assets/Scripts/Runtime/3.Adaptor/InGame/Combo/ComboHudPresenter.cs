namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///    コンボHUDの表示内容を更新するためのクラス。
    /// </summary>
    public class ComboHudPresenter
    {
        /// <summary>
        ///   コンボHUDの表示内容を更新するためのクラスを作成します。
        /// </summary>
        /// <param name="comboHudViewModel"> 表示更新の対象となるコンボHUDのViewModelです。 </param>
        public ComboHudPresenter(IComboHudViewModel comboHudViewModel)
        {
            _comboHudViewModel = comboHudViewModel;
        }
        /// <summary>
        ///    コンボHUDの表示内容を更新します。
        /// </summary>
        /// <param name="comboCount"> 表示する現在のコンボ数です。 </param>
        public void Present(int comboCount)
        { 
            var dto = new ComboDTO(comboCount);
            _comboHudViewModel.Apply(dto);
        }
        private readonly IComboHudViewModel _comboHudViewModel;
    }
}
