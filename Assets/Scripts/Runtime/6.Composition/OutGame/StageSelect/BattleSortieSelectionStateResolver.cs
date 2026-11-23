using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using SymphonyFramework.Services;

namespace KillChord.Runtime.Composition.OutGame.StageSelect
{
    /// <summary>
    ///     バトル出撃の選択状態をシーン間で共有するため、ServiceLocatorから取得し、無ければ生成して登録する。
    /// </summary>
    public static class BattleSortieSelectionStateResolver
    {
        /// <summary>
        ///     ServiceLocatorの選択状態を使う <see cref="BattleSortieSelectionService"/> を生成します。
        /// </summary>
        /// <returns> 生成したサービスです。 </returns>
        public static BattleSortieSelectionService CreateSelectionService()
        {
            return new BattleSortieSelectionService(ResolveSelectedBattleStageState, ResolveSelectedMissionState);
        }

        /// <summary>
        ///     バトルステージ選択状態を解決します。
        /// </summary>
        /// <returns> 解決した状態です。 </returns>
        private static SelectedBattleStageState ResolveSelectedBattleStageState()
        {
            if (Partitura.TryGet(out SelectedBattleStageState selectedBattleStageState))
            {
                return selectedBattleStageState;
            }

            selectedBattleStageState = new SelectedBattleStageState();
            Partitura.Register(selectedBattleStageState, ServiceLifetimeEnum.Persistent);
            return selectedBattleStageState;
        }

        /// <summary>
        ///     ミッション選択状態を解決します。
        /// </summary>
        /// <returns> 解決した状態です。 </returns>
        private static SelectedMissionState ResolveSelectedMissionState()
        {
            if (Partitura.TryGet(out SelectedMissionState selectedMissionState))
            {
                return selectedMissionState;
            }

            selectedMissionState = new SelectedMissionState();
            Partitura.Register(selectedMissionState, ServiceLifetimeEnum.Persistent);
            return selectedMissionState;
        }
    }
}
