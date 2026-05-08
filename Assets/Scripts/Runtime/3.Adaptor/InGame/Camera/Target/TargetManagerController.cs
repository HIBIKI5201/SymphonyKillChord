using KillChord.Runtime.Application.InGame.Camera.Target;

namespace KillChord.Runtime.Adaptor.InGame.Camera.Target
{
    /// <summary>
    ///     ロックオン対象ゲートウェイの登録・解除をターゲットマネージャーへ委譲するコントローラークラス。
    /// </summary>
    public sealed class TargetManagerController
    {
        /// <summary>
        ///     ターゲットマネージャーを受け取り、コントローラーを初期化するコンストラクタ。
        /// </summary>
        /// <param name="targetManager"> 対象の登録管理を行うターゲットマネージャー。</param>
        public TargetManagerController(TargetManager targetManager)
        {
            _manager = targetManager;
        }

        /// <summary>
        ///     ロックオン対象ゲートウェイをマネージャーに登録する。
        /// </summary>
        /// <param name="gateway"> 登録するロックオン対象ゲートウェイ。</param>
        public void Register(LockOnTargetGateway gateway)
        {
            _manager.Register(gateway);
        }

        /// <summary>
        ///     ロックオン対象ゲートウェイの登録をマネージャーから解除する。
        /// </summary>
        /// <param name="gateway"> 解除するロックオン対象ゲートウェイ。</param>
        public void Unregister(LockOnTargetGateway gateway)
        {
            _manager.Unregister(gateway);
        }

        private readonly TargetManager _manager;
    }
}
