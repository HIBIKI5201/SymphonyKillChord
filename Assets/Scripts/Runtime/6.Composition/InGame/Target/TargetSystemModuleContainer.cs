using KillChord.Runtime.Adaptor.InGame.Target;

namespace KillChord.Runtime.Composition.InGame.Target
{
    /// <summary>
    ///     ターゲットモジュールの公開サービスを保持するContainerです。
    /// </summary>
    public sealed class TargetSystemModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="targetSystemController"> ターゲット制御Controllerです。 </param>
        /// <param name="targetSystemViewModel"> ターゲットViewModelです。 </param>
        public TargetSystemModuleContainer(
            TargetSystemController targetSystemController,
            ITargetSystemViewModel targetSystemViewModel)
        {
            TargetSystemController = targetSystemController;
            TargetSystemViewModel = targetSystemViewModel;
        }

        /// <summary> ターゲット制御Controllerです。 </summary>
        public TargetSystemController TargetSystemController { get; }

        /// <summary> ターゲットViewModelです。 </summary>
        public ITargetSystemViewModel TargetSystemViewModel { get; }
    }
}
