using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     目標ステップの説明ポップアップ表示のインタフェース。
    /// </summary>
    public interface IMissionStepPopupView
    {
        /// <summary>
        ///     ポップアップを表示します。
        /// </summary>
        /// <param name="popupPrefab"> 表示する説明ポップアップのPrefabです。未設定の場合は何も表示しません。 </param>
        /// <param name="closeInput"> ポップアップを閉じる入力です。閉じる案内のアイコンに使います。 </param>
        void Show(GameObject popupPrefab, MissionStepPopupCloseInput closeInput);

        /// <summary>
        ///     ポップアップを非表示にします。
        /// </summary>
        void Hide();
    }
}
