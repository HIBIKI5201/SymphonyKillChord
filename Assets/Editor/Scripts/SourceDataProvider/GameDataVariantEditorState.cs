using System;
using UnityEditor;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     PlannerとEditor再生で共有するユーザーごとのゲームデータ種別を保持します。
    /// </summary>
    internal static class GameDataVariantEditorState
    {
        /// <summary> 現在選択中のゲームデータ種別です。 </summary>
        public static GameDataVariant SelectedVariant
        {
            get
            {
                string value = EditorPrefs.GetString(EDITOR_PREFS_KEY, nameof(GameDataVariant.Release));
                return Enum.TryParse(value, out GameDataVariant variant)
                    ? variant
                    : GameDataVariant.Release;
            }
        }

        /// <summary>
        ///     表示対象のゲームデータ種別を変更します。
        /// </summary>
        /// <param name="variant"> 変更後のゲームデータ種別です。 </param>
        public static void SetSelectedVariant(GameDataVariant variant)
        {
            EditorPrefs.SetString(EDITOR_PREFS_KEY, variant.ToString());
        }

        /// <summary>
        ///     指定種別に対応するAddressables Group名を取得します。
        /// </summary>
        /// <param name="variant"> 対象のゲームデータ種別です。 </param>
        /// <returns> Addressables Group名です。 </returns>
        public static string GetGroupName(GameDataVariant variant)
        {
            return variant == GameDataVariant.Demo ? DEMO_GROUP_NAME : RELEASE_GROUP_NAME;
        }

        internal const string RELEASE_GROUP_NAME = "GameData.Release";
        internal const string DEMO_GROUP_NAME = "GameData.Demo";
        internal const string SHARED_GROUP_NAME = "GameData.Shared";

        private const string EDITOR_PREFS_KEY = "KillChord.GameDataVariant";
    }
}
