using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Target
{
    /// <summary>
    ///     ターゲット登録・選択・現在ターゲット取得を仲介するViewModelインターフェース。
    /// </summary>
    public interface ITargetSystemViewModel
    {
        /// <summary>
        ///     ターゲットを登録する。
        /// </summary>
        /// <param name="targetable"> 登録するターゲット。 </param>
        void RegisterTarget(ITargetableViewModel targetable);

        /// <summary>
        ///     ターゲットの登録を解除する。
        /// </summary>
        /// <param name="targetable"> 解除するターゲット。 </param>
        void UnregisterTarget(ITargetableViewModel targetable);

        /// <summary>
        ///     現在のターゲットの取得を試みる。
        /// </summary>
        /// <param name="targetable"> 取得したターゲット。取得失敗時は null。 </param>
        /// <returns> 取得に成功した場合は true。 </returns>
        bool TryGetCurrentTarget(out ITargetableViewModel targetable);

        /// <summary>
        ///     現在のターゲットIDの取得を試みる。
        /// </summary>
        /// <param name="targetId"> 取得したターゲットID。取得失敗時は <see cref="Guid.Empty"/>。 </param>
        /// <returns> 取得に成功した場合は true。 </returns>
        bool TryGetCurrentTargetId(out Guid targetId);

        /// <summary>
        ///     現在のターゲット位置の取得を試みる。
        /// </summary>
        /// <param name="result"> 取得した位置。取得失敗時は <see cref="Vector3.zero"/>。 </param>
        /// <returns> 取得に成功した場合は true。 </returns>
        bool TryGetCurrentTargetPosition(out Vector3 result);

        /// <summary>
        ///     現在の候補ターゲットの取得を試みる。
        /// </summary>
        /// <param name="targetable"> 取得した候補ターゲット。取得失敗時はnull。 </param>
        /// <returns> 取得に成功した場合はtrue。 </returns>
        bool TryGetCurrentCandidate(out ITargetableViewModel targetable);

        /// <summary>
        ///     現在の候補ターゲットIDの取得を試みる。
        /// </summary>
        /// <param name="targetId"> 取得した候補ターゲットID。取得失敗時は <see cref="Guid.Empty"/>。 </param>
        /// <returns> 取得に成功した場合はtrue。 </returns>
        bool TryGetCurrentCandidateId(out Guid targetId);

        /// <summary>
        ///     現在の候補ターゲット位置の取得を試みる。
        /// </summary>
        /// <param name="result"> 取得した位置。取得失敗時は <see cref="Vector3.zero"/>。 </param>
        /// <returns> 取得に成功した場合はtrue。 </returns>
        bool TryGetCurrentCandidatePosition(out Vector3 result);

        /// <summary>
        ///     現在登録されているターゲット一覧のスナップショットを取得する。
        /// </summary>
        /// <returns> 登録ターゲット一覧です。 </returns>
        ITargetableViewModel[] GetRegisteredTargetsSnapshot();

        /// <summary>
        ///     プレイヤー位置と方向をもとに最適なターゲットへ切り替える。
        /// </summary>
        /// <param name="playerPosition"> プレイヤーの現在位置。 </param>
        /// <param name="direction"> 選択基準に使用する方向。 </param>
        void ChangeTarget(in Vector3 playerPosition, in Vector3 direction);

        /// <summary>
        ///     プレイヤー位置と方向をもとに候補ターゲットを更新する。
        /// </summary>
        /// <param name="playerPosition"> プレイヤーの現在位置。 </param>
        /// <param name="direction"> 選択基準に使用する方向。 </param>
        void UpdateCandidate(in Vector3 playerPosition, in Vector3 direction);

        /// <summary>
        ///     指定方向で評価した別ターゲットへの切り替えを試みる。
        /// </summary>
        /// <param name="playerPosition"> プレイヤーの現在位置。 </param>
        /// <param name="direction"> 選択基準に使用する方向。 </param>
        /// <returns> 別ターゲットへ切り替えた場合はtrue。 </returns>
        bool TrySwitchTarget(in Vector3 playerPosition, in Vector3 direction);

        /// <summary>
        ///     指定IDのターゲットを現在のターゲットとして設定することを試みる。
        /// </summary>
        /// <param name="targetId"> 設定対象のターゲットID。 </param>
        /// <returns> 設定に成功した場合は true。 </returns>
        bool TrySetCurrentTarget(Guid targetId);

        /// <summary>
        ///     現在のターゲット選択を解除する。
        /// </summary>
        void ClearTarget();
    }
}
