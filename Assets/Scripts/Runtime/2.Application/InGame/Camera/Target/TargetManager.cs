using KillChord.Runtime.Domain.InGame.Camera.Target;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Camera.Target
{
    /// <summary>
    ///     ロックオン対象の登録・解除・列挙を管理するクラス。
    /// </summary>
    public sealed class TargetManager
    {
        /// <summary> 現在登録されているロックオン対象の数。 </summary>
        public int Count => _targets.Count;

        /// <summary> 現在登録されているロックオン対象の列挙。 </summary>
        public IEnumerable<ILockOnTarget> GetTargets => _targets;

        /// <summary>
        ///     ロックオン対象を登録する。
        ///     既に登録済みの場合は警告ログを出力する。
        /// </summary>
        /// <param name="target"> 登録するロックオン対象。</param>
        public void Register(ILockOnTarget target)
        {
            if (_targets.Add(target))
            { return; }
            Debug.LogWarning($"Target {target} is already registered.");
        }

        /// <summary>
        ///     ロックオン対象の登録を解除する。
        ///     未登録の場合は警告ログを出力する。
        /// </summary>
        /// <param name="target"> 解除するロックオン対象。</param>
        public void Unregister(ILockOnTarget target)
        {
            if (_targets.Remove(target))
            { return; }
            Debug.LogWarning($"Target {target} is not registered.");
        }

        private readonly HashSet<ILockOnTarget> _targets = new();
    }
}
