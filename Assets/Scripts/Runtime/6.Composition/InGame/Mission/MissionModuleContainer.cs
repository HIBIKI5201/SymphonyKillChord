using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Application.InGame.Mission;

namespace KillChord.Runtime.Composition.InGame.Mission
{
    /// <summary>
    ///     ミッションモジュールの公開物を保持するContainerです。
    /// </summary>
    public sealed class MissionModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="missionRuntimeService"> ミッション実行時サービスです。 </param>
        /// <param name="missionEventController"> ミッションイベント制御です。 </param>
        public MissionModuleContainer(
            MissionRuntimeService missionRuntimeService,
            MissionEventController missionEventController)
        {
            MissionRuntimeService = missionRuntimeService;
            MissionEventController = missionEventController;
        }

        /// <summary> ミッション実行時サービスです。 </summary>
        public MissionRuntimeService MissionRuntimeService { get; }

        /// <summary> ミッションイベント制御です。 </summary>
        public MissionEventController MissionEventController { get; }
    }
}
