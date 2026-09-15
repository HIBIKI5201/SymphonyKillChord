using KillChord.Editor.SourceDataProvider;

namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     ゲームデータ種別とビルドモードの組み合わせで決まる、自動ビルドの設定枠です。
    /// </summary>
    internal readonly struct AutoBuildSlot
    {
        /// <summary>
        ///     設定枠を生成します。
        /// </summary>
        /// <param name="variant"> この枠でビルドするゲームデータ種別です。 </param>
        /// <param name="mode"> この枠のビルドモードです。 </param>
        /// <param name="pathPropertyName"> 出力先パスを保持するフィールド名です。 </param>
        /// <param name="profilesPropertyName"> Build Profile一覧を保持するフィールド名です。 </param>
        public AutoBuildSlot(
            GameDataVariant variant,
            AutoBuildMode mode,
            string pathPropertyName,
            string profilesPropertyName)
        {
            Variant = variant;
            Mode = mode;
            PathPropertyName = pathPropertyName;
            ProfilesPropertyName = profilesPropertyName;
        }

        /// <summary> この枠でビルドするゲームデータ種別です。 </summary>
        public GameDataVariant Variant { get; }

        /// <summary> この枠のビルドモードです。 </summary>
        public AutoBuildMode Mode { get; }

        /// <summary> 出力先パスを保持するフィールド名です。 </summary>
        public string PathPropertyName { get; }

        /// <summary> Build Profile一覧を保持するフィールド名です。 </summary>
        public string ProfilesPropertyName { get; }

        /// <summary> 画面表示用の名前です。 </summary>
        public string Label => $"{Variant} {Mode}";
    }
}
