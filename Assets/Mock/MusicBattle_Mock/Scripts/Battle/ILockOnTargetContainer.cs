using System.Collections.Generic;
using UnityEngine;

namespace Mock.MusicBattle.Battle
{
    /// <summary>
    ///     ロックオン可能なターゲットを管理するコンテナのインターフェース。
    /// </summary>
    public interface ILockOnTargetContainer
    {
        /// <summary>
        ///     指定されたインデックスのターゲットを取得します。
        ///     インデックスが範囲外の場合でも、ループして有効なターゲットを返します。
        /// </summary>
        /// <param name="index">ターゲットのインデックス。</param>
        /// <returns>指定されたインデックスのTransform。ターゲットがない場合はnull。</returns>
        public Transform this[int index] => Targets.Count != 0 ? Targets[(index + Targets.Count) % Targets.Count] : null;

        /// <summary>
        ///     すべてのロックオン可能なターゲットの読み取り専用リストを取得します。
        /// </summary>
        public IReadOnlyList<Transform> Targets { get; }

        /// <summary>
        ///     近い順にソートされたロックオン可能なターゲットの読み取り専用リストを取得します。
        /// </summary>
        public IReadOnlyList<Transform> NearerTargets { get; }
    }
}

