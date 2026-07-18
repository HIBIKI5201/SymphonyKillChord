using KillChord.Runtime.Domain.InGame.Character;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Target
{
    /// <summary>
    ///     ターゲットIDと CharacterEntity の対応を管理するレジストリ。
    /// </summary>
    public sealed class TargetEntityRegistry
    {
        /// <summary>
        ///     ターゲットIDと CharacterEntity を登録する。
        /// </summary>
        /// <param name="targetId"> 登録対象のターゲットID。</param>
        /// <param name="entity"> 対応する CharacterEntity。</param>
        public void RegisterEntity(Guid targetId, CharacterEntity entity)
        {
            if (entity == null)
            {
                Debug.LogError("entity が null です。");
                return;
            }

            _entities[targetId] = entity;
        }

        /// <summary>
        ///     ターゲットIDの登録を解除する。
        /// </summary>
        /// <param name="targetId"> 解除するターゲットID。</param>
        public void UnregisterEntity(Guid targetId)
        {
            _entities.Remove(targetId);
        }

        /// <summary>
        ///     ターゲットIDに対応する CharacterEntity の取得を試みる。
        /// </summary>
        /// <param name="targetId"> 取得するターゲットID。</param>
        /// <param name="entity"> 取得した CharacterEntity。取得失敗時は null。</param>
        /// <returns> 取得に成功した場合は true。</returns>
        public bool TryGetEntity(Guid targetId, out CharacterEntity entity)
        {
            return _entities.TryGetValue(targetId, out entity);
        }

        private readonly Dictionary<Guid, CharacterEntity> _entities = new();
    }
}
