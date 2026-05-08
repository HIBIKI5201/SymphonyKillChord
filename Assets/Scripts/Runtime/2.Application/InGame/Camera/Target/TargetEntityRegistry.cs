using KillChord.Runtime.Domain.InGame.Camera.Target;
using KillChord.Runtime.Domain.InGame.Character;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera.Target
{
    /// <summary>
    ///     ロックオン対象とキャラクターエンティティの対応関係を管理するレジストリクラス。
    /// </summary>
    public class TargetEntityRegistry
    {
        /// <summary>
        ///     ロックオン対象とキャラクターエンティティを登録する。
        ///     引数がnullの場合はエラーログを出力して処理を中断する。
        /// </summary>
        /// <param name="target"> 登録するロックオン対象。</param>
        /// <param name="entity"> 対象に紐づけるキャラクターエンティティ。</param>
        public void Register(ILockOnTarget target, CharacterEntity entity)
        {
            if (target == null)
            {
                Debug.LogError("Targetがnull");
                return;
            }

            if (entity == null)
            {
                Debug.LogError("Entityがnull");
                return;
            }

            _targetToEntity[target] = entity;
        }

        /// <summary>
        ///     指定したロックオン対象の登録を解除する。
        ///     引数がnullの場合はエラーログを出力して処理を中断する。
        /// </summary>
        /// <param name="target"> 解除するロックオン対象。</param>
        public void Unregister(ILockOnTarget target)
        {
            if (target == null)
            {
                Debug.LogError("Targetがnull");
                return;
            }
            _targetToEntity.Remove(target);
        }

        /// <summary>
        ///     指定したロックオン対象に紐づくキャラクターエンティティの取得を試みる。
        ///     引数がnullの場合はエラーログを出力してfalseを返す。
        /// </summary>
        /// <param name="target"> 取得対象のロックオン対象。</param>
        /// <param name="entity"> 取得したキャラクターエンティティ。取得失敗時は null。</param>
        /// <returns> 取得に成功した場合は true。</returns>
        public bool TryGetEntity(ILockOnTarget target, out CharacterEntity entity)
        {
            if (target == null)
            {
                Debug.LogError("Target が null");
                entity = null;
                return false;
            }
            return _targetToEntity.TryGetValue(target, out entity);
        }

        private readonly Dictionary<ILockOnTarget, CharacterEntity> _targetToEntity = new();
    }
}
