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
        /// <param name="message"> 表示するメッセージです。 </param>
        /// <param name="image"> 表示する画像です。未設定の場合はnullです。 </param>
        void Show(string message, Sprite image);

        /// <summary>
        ///     ポップアップを非表示にします。
        /// </summary>
        void Hide();
    }
}
