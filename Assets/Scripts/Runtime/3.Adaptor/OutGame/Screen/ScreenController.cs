using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     画面操作をユースケースへ伝達する Controller。
    /// </summary>
    public sealed class ScreenController : IScreenController
    {
        /// <summary>
        ///     Controller を初期化します。
        /// </summary>
        public ScreenController(
            ShowScreenUseCase showScreenUseCase,
            CloseCurrentScreenUseCase closeCurrentScreenUseCase,
            ResetToHomeScreenUseCase resetToHomeScreenUseCase)
        {
            _showScreenUseCase = showScreenUseCase;
            _closeCurrentScreenUseCase = closeCurrentScreenUseCase;
            _resetToHomeScreenUseCase = resetToHomeScreenUseCase;
        }

        /// <summary>
        ///    タイトル画面を表示します。
        /// </summary>
        public bool ShowTitle()
        {
            if (!TryBeginTransition()) { return false; }

            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Title));
            return true;
        }

        /// <summary>
        ///    メニュー画面を表示します。
        /// </summary>
        public bool ShowMenu()
        {
            if (!TryBeginTransition()) { return false; }

            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Menu));
            return true;
        }

        /// <summary>
        ///     オプション画面を表示します。
        /// </summary>
        public bool ShowOptions()
        {
            if (!TryBeginTransition()) { return false; }

            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Options));
            return true;
        }

        /// <summary>
        ///    クレジット画面を表示します。
        /// </summary>
        public bool ShowCredit()
        {
            if (!TryBeginTransition()) { return false; }

            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Credit));
            return true;
        }

        /// <summary>
        ///     ホーム画面を表示します。
        /// </summary>
        public bool ShowHome()
        {
            if (!TryBeginTransition()) { return false; }

            _resetToHomeScreenUseCase.Execute();
            return true;
        }

        /// <summary>
        ///     作戦画面を表示します。
        /// </summary>
        public bool ShowStageSelect()
        {
            if (!TryBeginTransition()) { return false; }

            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.StageSelect));
            return true;
        }

        /// <summary>
        ///     研究画面を表示します。
        /// </summary>
        public bool ShowSkillTree()
        {
            if (!TryBeginTransition()) { return false; }

            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.SkillTree));
            return true;
        }

        /// <summary>
        ///     改造画面を表示します。
        /// </summary>
        public bool ShowSkillBuild()
        {
            if (!TryBeginTransition()) { return false; }

            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.SkillBuild));
            return true;
        }

        /// <summary>
        ///     戦闘準備画面を表示します。
        /// </summary>
        public bool ShowBattlePreparation(string targetSceneName)
        {
            if (!TryBeginTransition()) { return false; }

            _showScreenUseCase.Execute(
                new ShowScreenCommand(ScreenId.BattlePreparation, targetSceneName));
            return true;
        }

        /// <summary>
        ///     設定画面を表示します。
        /// </summary>
        public bool ShowSetting()
        {
            if (!TryBeginTransition()) { return false; }

            _showScreenUseCase.Execute(new ShowScreenCommand(ScreenId.Setting));
            return true;
        }

        /// <summary>
        ///     現在画面を閉じます。
        /// </summary>
        public bool CloseCurrent()
        {
            if (!TryBeginTransition()) { return false; }

            _closeCurrentScreenUseCase.Execute();
            return true;
        }

        /// <summary>
        ///     画面遷移の受付を再開するまでのクールダウン秒数。
        ///     USS の transition-duration と揃えており、
        ///     トランジション再生中の連打による多重遷移を防ぎます。
        /// </summary>
        private const float TRANSITION_COOLDOWN_SEC = 0.2f;

        /// <summary>
        ///     画面遷移要求を受け付けるかどうかを判定します。
        ///     受け付ける場合はクールダウンを更新します。
        /// </summary>
        /// <returns> 受け付ける場合は true を返します。 </returns>
        private bool TryBeginTransition()
        {
            // タイムスケールの影響を受けないよう実時間で判定する。
            float now = Time.realtimeSinceStartup;

            if (now < _acceptableTime)
            {
                return false;
            }

            _acceptableTime = now + TRANSITION_COOLDOWN_SEC;
            return true;
        }

        private readonly CloseCurrentScreenUseCase _closeCurrentScreenUseCase;
        private readonly ResetToHomeScreenUseCase _resetToHomeScreenUseCase;
        private readonly ShowScreenUseCase _showScreenUseCase;

        /// <summary> 次に画面遷移を受け付けられるようになる実時間。 </summary>
        private float _acceptableTime;
    }
}
