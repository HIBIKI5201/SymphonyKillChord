using KillChord.Runtime.Adaptor.InGame.Target;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Reticle
{
    /// <summary>
    ///     TargetingSystemに登録された敵のうち、注目/候補以外の生存敵の
    ///     レティクル表示情報を算出するPresenter。
    ///     注目/候補の強調表示は既存の HUDEnemyHealthView が担当するため、
    ///     ここでは重複を避けて両IDを除外する。
    /// </summary>
    public sealed class ReticleHudPresenter
    {
        /// <summary>
        ///     ターゲットViewModelと投影処理を受け取り、Presenterを初期化する。
        /// </summary>
        /// <param name="targetSystemViewModel"> 登録ターゲットと現在ターゲットを提供するViewModel。 </param>
        /// <param name="screenProjector"> ワールド座標をスクリーン座標へ投影する処理。 </param>
        public ReticleHudPresenter(
            ITargetSystemViewModel targetSystemViewModel,
            IScreenProjector screenProjector)
        {
            _targetSystemViewModel = targetSystemViewModel
                ?? throw new ArgumentNullException(nameof(targetSystemViewModel));
            _screenProjector = screenProjector
                ?? throw new ArgumentNullException(nameof(screenProjector));
        }

        /// <summary>
        ///     表示すべきレティクル一覧をバッファへ書き込む。
        ///     状態変化を通知するイベントが無いため、毎フレーム呼び出すこと。
        /// </summary>
        /// <param name="buffer"> 結果を書き込むバッファ。呼び出し側が保持・再利用する。 </param>
        public void Tick(List<ReticleMarker> buffer)
        {
            if (buffer == null)
            {
                return;
            }

            buffer.Clear();

            // 注目/候補は既存HUDが担当するため除外する。
            Guid focusedId = _targetSystemViewModel.TryGetCurrentTargetId(out Guid currentId)
                ? currentId
                : Guid.Empty;
            Guid candidateId = _targetSystemViewModel.TryGetCurrentCandidateId(out Guid candidate)
                ? candidate
                : Guid.Empty;

            ITargetableViewModel[] targets = _targetSystemViewModel.GetRegisteredTargetsSnapshot();
            for (int i = 0; i < targets.Length; i++)
            {
                ITargetableViewModel target = targets[i];
                if (target == null || !target.IsAlive)
                {
                    continue;
                }

                Guid targetId = target.TargetId;
                if (targetId == focusedId || targetId == candidateId)
                {
                    continue;
                }

                if (!_screenProjector.TryWorldToScreen(target.Position, out Vector2 screenPosition))
                {
                    continue;
                }

                buffer.Add(new ReticleMarker(targetId, screenPosition));
            }
        }

        private readonly ITargetSystemViewModel _targetSystemViewModel;
        private readonly IScreenProjector _screenProjector;
    }
}
