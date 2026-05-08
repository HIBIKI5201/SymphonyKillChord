using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     スキルの実行（Effect + Visual）を行うインターフェース。
    /// </summary>
    public interface IViewAction
    {
        /// <summary>
        ///     指定したIDのスキルを実行する（Effect + Visual）。
        /// </summary>
        void Execute(int skillId);
    }
}
