using KillChord.Runtime.Application.InGame.Camera.Target;
using KillChord.Runtime.Domain.InGame.Camera.Target;
using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Adaptor.InGame.Camera.Target
{
    /// <summary>
    ///     ロックオン対象とキャラクターエンティティの登録・解除・取得をレジストリへ委譲するコントローラークラス。
    /// </summary>
    public class TargetEntityRegistryController
    {
        /// <summary>
        ///     対象エンティティのレジストリを受け取り、コントローラーを初期化するコンストラクタ。
        /// </summary>
        /// <param name="registry"> 対象エンティティの登録管理を行うレジストリ。</param>
        public TargetEntityRegistryController(TargetEntityRegistry registry)
        {
            _registry = registry;
        }

        /// <summary>
        ///     ロックオン対象とキャラクターエンティティをレジストリに登録する。
        /// </summary>
        /// <param name="lockOnTarget"> 登録するロックオン対象。</param>
        /// <param name="entity"> 対象に紐づけるキャラクターエンティティ。</param>
        public void RegisterTargetEntity(ILockOnTarget lockOnTarget, CharacterEntity entity)
        {
            _registry.Register(lockOnTarget, entity);
        }

        /// <summary>
        ///     指定したロックオン対象の登録をレジストリから解除する。
        /// </summary>
        /// <param name="lockOnTarget"> 解除するロックオン対象。</param>
        public void UnregisterTargetEntity(ILockOnTarget lockOnTarget)
        {
            _registry.Unregister(lockOnTarget);
        }

        /// <summary>
        ///     指定したロックオン対象に紐づくキャラクターエンティティの取得を試みる。
        /// </summary>
        /// <param name="lockOnTarget"> 取得対象のロックオン対象。</param>
        /// <param name="entity"> 取得したキャラクターエンティティ。取得失敗時は null。</param>
        /// <returns> 取得に成功した場合は true。</returns>
        public bool GetTargetEntity(ILockOnTarget lockOnTarget, out CharacterEntity entity)
        {
            return _registry.TryGetEntity(lockOnTarget, out entity);
        }

        private TargetEntityRegistry _registry;
    }
}
