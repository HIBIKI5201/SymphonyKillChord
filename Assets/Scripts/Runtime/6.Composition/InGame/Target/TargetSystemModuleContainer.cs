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
        /// <param name="targetEntityRegistry"> ターゲットEntityレジストリです。 </param>
        /// <param name="targetAreaQuery"> 扇形範囲クエリです。 </param>
        public TargetSystemModuleContainer(
            TargetSystemController targetSystemController,
            ITargetSystemViewModel targetSystemViewModel,
            TargetEntityRegistry targetEntityRegistry,
            TargetAreaQuery targetAreaQuery)
        {
            TargetSystemController = targetSystemController;
            TargetSystemViewModel = targetSystemViewModel;
            TargetEntityRegistry = targetEntityRegistry;
            TargetAreaQuery = targetAreaQuery;
        }

        /// <summary> ターゲット制御Controllerです。 </summary>
        public TargetSystemController TargetSystemController { get; }

        /// <summary> ターゲットViewModelです。 </summary>
        public ITargetSystemViewModel TargetSystemViewModel { get; }

        /// <summary> ターゲットEntityレジストリです。 </summary>
        public TargetEntityRegistry TargetEntityRegistry { get; }

        /// <summary> 扇形範囲クエリです。 </summary>
        public TargetAreaQuery TargetAreaQuery { get; }
    }
}
