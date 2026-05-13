using KillChord.Runtime.Application;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     インゲームにおけるミッションの制御を行うコントローラークラス。
    /// </summary>
    public class InGameMissionController
    {
        /// <summary>
        ///     InGameMissionController クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="selectedMissionState">選択されたミッションの状態。</param>
        /// <param name="missionDefinitionRepository">ミッション定義リポジトリ。</param>
        /// <param name="missionFactory">ミッションファクトリ。</param>
        public InGameMissionController(
            SelectedMissionState selectedMissionState,
            IMissionDefinitionRepository missionDefinitionRepository,
            MissionFactory missionFactory)
        {
            _selectedMissionState = selectedMissionState;
            _missionDefinitionRepository = missionDefinitionRepository;
            _missionFactory = missionFactory;
        }

        /// <summary>
        ///     ミッション定義をロードします。
        /// </summary>
        /// <returns>ミッション定義。</returns>
        public MissionDefinition LoadDefinition()
        {
            return _missionDefinitionRepository.Get(_selectedMissionState.CurrentMissionId);
        }

        /// <summary>
        ///     ミッション進行状況を生成します。
        /// </summary>
        /// <returns>ミッション進行状況。</returns>
        public MissionProgress CreateProgress()
        {
            return _missionFactory.CreateMissionProgress();
        }

        /// <summary> 選択されたミッションの状態。 </summary>
        private readonly SelectedMissionState _selectedMissionState;
        /// <summary> ミッション定義リポジトリ。 </summary>
        private readonly IMissionDefinitionRepository _missionDefinitionRepository;
        /// <summary> ミッションファクトリ。 </summary>
        private readonly MissionFactory _missionFactory;
    }
}
