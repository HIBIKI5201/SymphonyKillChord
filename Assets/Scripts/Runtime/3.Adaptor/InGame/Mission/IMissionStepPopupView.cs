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
        /// <param name="imageEntryKey"> TutorialPopupImagesテーブルのエントリーキーです。 </param>
        /// <param name="fallbackImage"> ローカライズ画像を取得できない場合に表示する画像です。未設定の場合はnullです。 </param>
        void Show(string imageEntryKey, Sprite fallbackImage);

        /// <summary>
        ///     ポップアップを非表示にします。
        /// </summary>
        void Hide();
    }
}
