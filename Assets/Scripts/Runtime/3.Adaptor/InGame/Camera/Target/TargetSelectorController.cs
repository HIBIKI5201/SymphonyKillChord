using KillChord.Runtime.Application.InGame.Camera.Target;
using KillChord.Runtime.Domain.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Camera.Target
{
    /// <summary>
    ///     ロックオン対象の選択と、対象に紐づくキャラクターエンティティの取得をセレクターへ委譲するコントローラークラス。
    /// </summary>
    public class TargetSelectorController
    {
        /// <summary>
        ///     セレクターとレジストリコントローラーを受け取り、コントローラーを初期化するコンストラクタ。
        /// </summary>
        /// <param name="selector"> ロックオン対象の選択を管理するセレクター。</param>
        /// <param name="registryController"> 対象エンティティの取得に使用するレジストリコントローラー。</param>
        public TargetSelectorController(TargetSelector selector, TargetEntityRegistryController registryController)
        {
            _selector = selector;
            _registryController = registryController;
        }

        /// <summary>
        ///     プレイヤー位置と方向をもとに、ロックオン対象を切り替える。
        /// </summary>
        /// <param name="playerPosition"> プレイヤーの現在位置。</param>
        /// <param name="direction"> カメラの向いている方向。</param>
        public void ChangeTarget(in Vector3 playerPosition, in Vector3 direction)
        {
            _selector.ChangeTarget(playerPosition, direction);
        }

        /// <summary>
        ///     現在のロックオン対象に紐づくキャラクターエンティティの取得を試みる。
        /// </summary>
        /// <param name="entity"> 取得したキャラクターエンティティ。取得失敗時は null。</param>
        /// <returns> 対象が存在しエンティティの取得に成功した場合は true。</returns>
        public bool TryGetCurrentTargetEntity(out CharacterEntity entity)
        {
            entity = null;

            if (!_selector.TryGetCurrentTarget(out var target))
            { return false; }

            return _registryController.GetTargetEntity(target, out entity);
        }

        private readonly TargetSelector _selector;
        private readonly TargetEntityRegistryController _registryController;
    }
}
