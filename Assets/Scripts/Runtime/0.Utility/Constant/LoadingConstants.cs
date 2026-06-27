using UnityEngine;

namespace KillChord.Runtime.Utility.Constant
{
    /// <summary>
    ///    ロード処理に関する定数をまとめたクラス。
    /// </summary>
    public static class LoadingConstants
    {
        /// <summary>
        ///     パーセント表示へ変換する倍率。
        /// </summary>
        public const float PERCENTAGE_MULTIPLIER = 100f;

        /// <summary>
        ///     通常のシーン遷移におけるロード完了位置。
        /// </summary>
        public const float SCENE_LOAD_END_PROGRESS = 0.8f;

        /// <summary>
        ///     ActiveScene切り替え処理へ割り当てる進捗幅。
        /// </summary>
        public const float ACTIVE_SCENE_PROGRESS = 0.05f;

        /// <summary>
        ///     OutGameからInGameへの遷移完了位置。
        /// </summary>
        public const float IN_GAME_SCENE_LOAD_END_PROGRESS = 0.35f;

        /// <summary>
        ///     InGame側のステージロード終了位置。
        ///     InGame側に割り当てられた進捗範囲内で使用する。
        /// </summary>
        public const float STAGE_LOAD_END_PROGRESS = 0.7f;

        /// <summary>
        ///     リザルトからの遷移における、
        ///     バトルシーンのアンロード完了位置。
        /// </summary>
        public const float RESULT_BATTLE_SCENE_UNLOAD_END_PROGRESS = 0.25f;

        /// <summary>
        ///     シーン再読み込みにおけるアンロード完了位置。
        /// </summary>
        public const float SCENE_RELOAD_UNLOAD_END_PROGRESS = 0.35f;
    }
}
