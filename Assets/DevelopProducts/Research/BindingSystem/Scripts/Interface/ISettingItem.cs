namespace DevelopProducts.BindingSystem
{
    /// <summary>
    ///     個々の設定項目（音量・感度・グラフィック品質など）が実装する共通インターフェース。
    /// </summary>
    public interface ISettingItem
    {
        /// <summary>保存されている値を読み込み、UIと実システムの両方に反映する。</summary>
        void Load();

        /// <summary>現在のUIの値を実システムに反映し、保存する。</summary>
        void Apply();

        /// <summary>デフォルト値に戻す（保存はしない）。</summary>
        void ResetToDefault();
        /// <summary>パネルの初期化処理。必要に応じてPauseMenuPanelViewを参照して他の項目と連携することも可能。</summary>
        void Initialize(PauseMenuPanelView pauseMenu);
    }
}
