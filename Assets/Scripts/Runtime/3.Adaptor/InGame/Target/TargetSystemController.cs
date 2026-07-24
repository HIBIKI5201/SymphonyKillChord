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
        /// <summary> 有効なターゲットをロックオンした時に発火します。 </summary>
        public event Action OnTargetLocked;

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
        ///     現在の候補ターゲットEntityの取得を試みる。
        /// </summary>
        /// <param name="entity"> 取得したEntity。取得失敗時はnull。 </param>
        /// <returns> 取得に成功した場合はtrue。 </returns>
        public bool TryGetCurrentCandidateEntity(out CharacterEntity entity)
        {
            entity = null;
            if (!_targetSystemViewModel.TryGetCurrentCandidateId(out Guid targetId))
            {
                return false;
            }

            return _targetEntityRegistry.TryGetEntity(targetId, out entity);
        }

        /// <summary>
        ///     現在の候補ターゲット位置の取得を試みる。
        /// </summary>
        /// <param name="position"> 取得した位置。 </param>
        /// <returns> 取得に成功した場合はtrue。 </returns>
        public bool TryGetCurrentCandidatePosition(out Vector3 position)
        {
            return _targetSystemViewModel.TryGetCurrentCandidatePosition(out position);
        }

        /// <summary>
        ///     現在のターゲットを切り替える。
        /// </summary>
        /// <param name="playerPosition"> プレイヤー位置。 </param>
        /// <param name="direction"> 基準方向。 </param>
        public void ChangeTarget(in Vector3 playerPosition, in Vector3 direction)
        {
            _targetSystemViewModel.ChangeTarget(playerPosition, direction);
            NotifyTargetLockedIfAvailable();
        }

        /// <summary>
        ///     プレイヤー位置と方向をもとに候補ターゲットを更新する。
        /// </summary>
        /// <param name="playerPosition"> プレイヤー位置。 </param>
        /// <param name="direction"> 基準方向。 </param>
        public void UpdateCandidate(in Vector3 playerPosition, in Vector3 direction)
        {
            _targetSystemViewModel.UpdateCandidate(playerPosition, direction);
        }

        /// <summary>
        ///     指定方向で評価した別ターゲットへの切り替えを試みる。
        /// </summary>
        /// <param name="playerPosition"> プレイヤー位置。 </param>
        /// <param name="direction"> 基準方向。 </param>
        /// <returns> 別ターゲットへ切り替えた場合はtrue。 </returns>
        public bool TrySwitchTarget(in Vector3 playerPosition, in Vector3 direction)
        {
            return _targetSystemViewModel.TrySwitchTarget(playerPosition, direction);
        }

        /// <summary>
        ///     指定IDのターゲットを現在のターゲットとして設定することを試みる。
        /// </summary>
        /// <param name="targetId"> 設定対象のターゲットID。 </param>
        /// <returns> 設定に成功した場合は true。 </returns>
        public bool TrySetCurrentTarget(Guid targetId)
        {
            bool succeeded = _targetSystemViewModel.TrySetCurrentTarget(targetId);
            if (succeeded)
            {
                OnTargetLocked?.Invoke();
            }

            return succeeded;
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

        /// <summary>
        ///     現在のターゲットが有効な場合にロックオン成立を通知します。
        /// </summary>
        private void NotifyTargetLockedIfAvailable()
        {
            if (_targetSystemViewModel.TryGetCurrentTarget(out _))
            {
                OnTargetLocked?.Invoke();
            }
        }
    }
}
