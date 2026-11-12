using System;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     ポーズ画面のミッション欄に出す、メインミッションとサブミッションの一覧。
    /// </summary>
    public readonly ref struct PauseMissionDTO
    {
        /// <summary>
        ///     ミッション欄の内容を指定して生成する。
        /// </summary>
        /// <param name="hasMission"> 表示するミッションがある場合はtrue。 </param>
        /// <param name="mainMission"> メインミッション。 </param>
        /// <param name="subMissions"> サブミッションの一覧。 </param>
        /// <param name="emptySubMissionText"> サブミッションが無いときに出す文。 </param>
        public PauseMissionDTO(
            bool hasMission,
            PauseMissionItemDTO mainMission,
            ReadOnlySpan<PauseMissionItemDTO> subMissions,
            string emptySubMissionText)
        {
            HasMission = hasMission;
            MainMission = mainMission;
            SubMissions = subMissions;
            EmptySubMissionText = emptySubMissionText;
        }

        /// <summary> 表示するミッションがある場合はtrue。 </summary>
        public bool HasMission { get; }

        /// <summary> メインミッション。 </summary>
        public PauseMissionItemDTO MainMission { get; }

        /// <summary> サブミッションの一覧。 </summary>
        public ReadOnlySpan<PauseMissionItemDTO> SubMissions { get; }

        /// <summary> サブミッションが無いときに出す文。 </summary>
        public string EmptySubMissionText { get; }
    }
}
