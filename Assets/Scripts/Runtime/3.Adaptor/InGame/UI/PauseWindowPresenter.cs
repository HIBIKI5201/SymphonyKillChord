using KillChord.Runtime.Adaptor.Persistent.Localization;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using System;

namespace KillChord.Runtime.Adaptor.InGame.UI
{
    /// <summary>
    ///     ポーズ画面の文言と、ミッションの達成状況を表示へ渡すプレゼンター。
    /// </summary>
    public class PauseWindowPresenter
    {
        /// <summary>
        ///     表示先と文言の取得元、ミッションの状態を指定して生成する。
        /// </summary>
        /// <param name="viewModel"> ポーズ画面のViewModel。 </param>
        /// <param name="textProvider"> 文言の取得元。nullなら日本語で表示する。 </param>
        /// <param name="missionRuntimeService"> ミッションの状態。nullならミッション欄を空にする。 </param>
        public PauseWindowPresenter(
            IPauseWindowViewModel viewModel,
            IUiTextProvider textProvider,
            MissionRuntimeService missionRuntimeService)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _textProvider = textProvider;
            _missionRuntimeService = missionRuntimeService;
        }

        /// <summary>
        ///     選択中の言語の文言と、現在のミッションの達成状況を表示へ渡す。
        /// </summary>
        public void Present()
        {
            PresentLabels();
            PresentMissions();
        }

        private const string RESUME_KEY = "ui.pause.resume";
        private const string RESUME_FALLBACK = "ゲームに戻る";
        private const string OPTION_KEY = "ui.pause.option";
        private const string OPTION_FALLBACK = "オプション";
        private const string RETRY_KEY = "ui.pause.retry";
        private const string RETRY_FALLBACK = "ステージを再挑戦する";
        private const string GIVE_UP_KEY = "ui.pause.give_up";
        private const string GIVE_UP_FALLBACK = "ステージをあきらめる";
        private const string MISSION_KEY = "ui.pause.mission";
        private const string MISSION_FALLBACK = "ミッション";
        private const string MAIN_MISSION_KEY = "ui.pause.main_mission";
        private const string MAIN_MISSION_FALLBACK = "メインミッション";
        private const string SUB_MISSION_KEY = "ui.pause.sub_mission";
        private const string SUB_MISSION_FALLBACK = "サブミッション";
        private const string ACHIEVED_KEY = "ui.pause.achieved";
        private const string ACHIEVED_FALLBACK = "達成";
        private const string NOT_ACHIEVED_KEY = "ui.pause.not_achieved";
        private const string NOT_ACHIEVED_FALLBACK = "未達成";
        private const string NO_SUB_MISSION_KEY = "ui.pause.no_sub_mission";
        private const string NO_SUB_MISSION_FALLBACK = "なし";
        private const string BGM_VOLUME_KEY = "ui.pause.bgm_volume";
        private const string BGM_VOLUME_FALLBACK = "BGM音量";
        private const string SOUND_EFFECT_VOLUME_KEY = "ui.pause.se_volume";
        private const string SOUND_EFFECT_VOLUME_FALLBACK = "SE音量";
        private const string VOICE_VOLUME_KEY = "ui.pause.voice_volume";
        private const string VOICE_VOLUME_FALLBACK = "ボイス音量";
        private const string RHYTHM_OFFSET_KEY = "ui.pause.rhythm_offset";
        private const string RHYTHM_OFFSET_FALLBACK = "判定調整";

        // 設定画面と同じ「とじる」を使う。
        private const string CLOSE_KEY = "ui.setting.close";
        private const string CLOSE_FALLBACK = "とじる";

        private readonly IPauseWindowViewModel _viewModel;
        private readonly IUiTextProvider _textProvider;
        private readonly MissionRuntimeService _missionRuntimeService;

        /// <summary>
        ///     ボタンと見出しの文言を表示へ渡す。
        /// </summary>
        private void PresentLabels()
        {
            PauseWindowLabelDTO labels = new(
                GetText(RESUME_KEY, RESUME_FALLBACK),
                GetText(OPTION_KEY, OPTION_FALLBACK),
                GetText(RETRY_KEY, RETRY_FALLBACK),
                GetText(GIVE_UP_KEY, GIVE_UP_FALLBACK),
                GetText(MISSION_KEY, MISSION_FALLBACK),
                GetText(MAIN_MISSION_KEY, MAIN_MISSION_FALLBACK),
                GetText(SUB_MISSION_KEY, SUB_MISSION_FALLBACK),
                GetText(BGM_VOLUME_KEY, BGM_VOLUME_FALLBACK),
                GetText(SOUND_EFFECT_VOLUME_KEY, SOUND_EFFECT_VOLUME_FALLBACK),
                GetText(VOICE_VOLUME_KEY, VOICE_VOLUME_FALLBACK),
                GetText(RHYTHM_OFFSET_KEY, RHYTHM_OFFSET_FALLBACK),
                GetText(CLOSE_KEY, CLOSE_FALLBACK));
            _viewModel.ApplyLabels(labels);
        }

        /// <summary>
        ///     メインミッションとサブミッションの達成状況を表示へ渡す。
        /// </summary>
        private void PresentMissions()
        {
            string emptySubMissionText = GetText(NO_SUB_MISSION_KEY, NO_SUB_MISSION_FALLBACK);
            if (_missionRuntimeService?.MissionDefinition == null)
            {
                _viewModel.ApplyMissions(new PauseMissionDTO(
                    false,
                    default,
                    ReadOnlySpan<PauseMissionItemDTO>.Empty,
                    emptySubMissionText));
                return;
            }

            // メインミッションは、ミッションをクリアで終えたときだけ達成とする。
            bool isMainMissionAchieved = _missionRuntimeService.MissionProgress.EndReason == MissionEndReason.Clear;
            PauseMissionItemDTO mainMission = CreateItem(
                _missionRuntimeService.MissionDefinition.MainMissionText,
                isMainMissionAchieved);

            // サブミッションは評価条件の成功状態をそのまま達成として扱う。
            MissionEvaluationProgress[] progresses = _missionRuntimeService.BuildEvaluationResult().Progresses;
            PauseMissionItemDTO[] subMissions = new PauseMissionItemDTO[progresses.Length];
            for (int i = 0; i < progresses.Length; i++)
            {
                subMissions[i] = CreateItem(progresses[i].Description, progresses[i].IsSucceeded);
            }

            _viewModel.ApplyMissions(new PauseMissionDTO(true, mainMission, subMissions, emptySubMissionText));
        }

        /// <summary>
        ///     ミッション1件分の表示情報を作る。
        /// </summary>
        /// <param name="description"> ミッションの表示文。 </param>
        /// <param name="isAchieved"> 達成している場合はtrue。 </param>
        /// <returns> ミッション1件分の表示情報。 </returns>
        private PauseMissionItemDTO CreateItem(string description, bool isAchieved)
        {
            string statusText = isAchieved
                ? GetText(ACHIEVED_KEY, ACHIEVED_FALLBACK)
                : GetText(NOT_ACHIEVED_KEY, NOT_ACHIEVED_FALLBACK);
            return new PauseMissionItemDTO(description ?? string.Empty, isAchieved, statusText);
        }

        /// <summary>
        ///     キーに対応する選択中の言語の文を取得する。
        /// </summary>
        /// <param name="key"> UICommon テーブルのキー。 </param>
        /// <param name="fallback"> 取得できないときの日本語の文。 </param>
        /// <returns> 選択中の言語の文。 </returns>
        private string GetText(string key, string fallback)
        {
            return UiText.Get(_textProvider, key, fallback);
        }
    }
}
