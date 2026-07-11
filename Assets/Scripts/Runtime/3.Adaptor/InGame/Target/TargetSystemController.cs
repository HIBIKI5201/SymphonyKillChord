using KillChord.Runtime.Domain.InGame.Character;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Target
{
    /// <summary>
    ///     ターゲット選択ViewModelとEntityレジストリを仲介するコントローラー。
    /// </summary>
    public sealed class TargetSystemController
    {
        /// <summary>
        ///     ターゲット選択ViewModelとレジストリを受け取り、コントローラーを初期化する。
        /// </summary>
        /// <param name="targetSystemViewModel"> ターゲット選択ViewModel。 </param>
        /// <param name="targetEntityRegistry"> ターゲットEntityレジストリ。 </param>
        public TargetSystemController(ITargetSystemViewModel targetSystemViewModel, TargetEntityRegistry targetEntityRegistry)
        {
            _targetSystemViewModel = targetSystemViewModel;
            _targetEntityRegistry = targetEntityRegistry;
        }

        /// <summary>
        ///     ターゲットと対応するEntityを登録する。
        /// </summary>
        /// <param name="targetable"> 登録するターゲット。 </param>
        /// <param name="entity"> 対応するEntity。 </param>
        public void RegisterTarget(ITargetableViewModel targetable, CharacterEntity entity)
        {
            if (targetable == null)
            {
                Debug.LogError("targetable が null です。");
                return;
            }

            _targetSystemViewModel.RegisterTarget(targetable);
            _targetEntityRegistry.RegisterEntity(targetable.TargetId, entity);
        }

        /// <summary>
        ///     ターゲット登録を解除する。
        /// </summary>
        /// <param name="targetable"> 解除するターゲット。 </param>
        public void UnregisterTarget(ITargetableViewModel targetable)
        {
            if (targetable == null)
            {
                return;
            }

            _targetSystemViewModel.UnregisterTarget(targetable);
            _targetEntityRegistry.UnregisterEntity(targetable.TargetId);
        }

        /// <summary>
        ///     現在のターゲットEntityの取得を試みる。
        /// </summary>
        /// <param name="entity"> 取得したEntity。取得失敗時は null。 </param>
        /// <returns> 取得に成功した場合は true。 </returns>
        public bool TryGetCurrentTargetEntity(out CharacterEntity entity)
        {
            entity = null;

            if (!_targetSystemViewModel.TryGetCurrentTargetId(out Guid targetId))
            {
                return false;
            }

            return _targetEntityRegistry.TryGetEntity(targetId, out entity);
        }

        /// <summary>
        ///     現在のターゲットの取得を試みる。
        /// </summary>
        /// <param name="targetable"> 取得したターゲット。 </param>
        /// <returns> 取得に成功した場合は true。 </returns>
        public bool TryGetCurrentTarget(out ITargetableViewModel targetable)
        {
            return _targetSystemViewModel.TryGetCurrentTarget(out targetable);
        }

        /// <summary>
        ///     現在のターゲット位置の取得を試みる。
        /// </summary>
        /// <param name="position"> 取得した位置。 </param>
        /// <returns> 取得に成功した場合は true。 </returns>
        public bool TryGetCurrentTargetPosition(out Vector3 position)
        {
            return _targetSystemViewModel.TryGetCurrentTargetPosition(out position);
        }

        /// <summary>
        ///     現在のターゲットを切り替える。
        /// </summary>
        /// <param name="playerPosition"> プレイヤー位置。 </param>
        /// <param name="direction"> 基準方向。 </param>
        public void ChangeTarget(in Vector3 playerPosition, in Vector3 direction)
        {
            _targetSystemViewModel.ChangeTarget(playerPosition, direction);
        }

        /// <summary>
        ///     現在のターゲット選択を解除する。
        /// </summary>
        public void ClearTarget()
        {
            _targetSystemViewModel.ClearTarget();
        }

        private readonly ITargetSystemViewModel _targetSystemViewModel;
        private readonly TargetEntityRegistry _targetEntityRegistry;
    }
}
