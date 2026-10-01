using KillChord.Runtime.Adaptor.InGame.Target;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     クロスヘア上のリズムコマンドView群の表示を仲介するコントローラー。
    ///     同じ入力履歴で複数スキルが同時に進行中の場合（同じ拍で始動した場合）は、そのすべてを表示する。
    ///     ロックオン対象が存在しない（クロスヘア非表示の）間は表示しない。
    ///     ロックオンの着脱は拍の入力とは非同期に起こるため、<see cref="Tick"/>を毎フレーム呼び出して追従させる。
    /// </summary>
    public sealed class SkillCrosshairProgressController
    {
        /// <summary>
        ///     ロックオン状態の取得元を指定して生成する。
        /// </summary>
        /// <param name="targetSystemProvider">
        ///     ターゲットシステムを返す処理。登録順に依存しないよう判定のたびに呼ぶ。取得できない場合はnullを返す。
        /// </param>
        public SkillCrosshairProgressController(Func<TargetSystemController> targetSystemProvider)
        {
            _targetSystemProvider = targetSystemProvider
                ?? throw new ArgumentNullException(nameof(targetSystemProvider));
        }

        /// <summary>
        ///     スキルごとの入力進行状態を報告する。
        /// </summary>
        /// <param name="isInProgress"> 入力が進行中かどうか。 </param>
        /// <param name="crosshairView"> 報告元スキルのクロスヘア用View。 </param>
        /// <exception cref="ArgumentNullException"> Viewがnullの場合。 </exception>
        public void ReportProgress(bool isInProgress, ISkillCrosshairProgressView crosshairView)
        {
            if (crosshairView == null)
            {
                throw new ArgumentNullException(nameof(crosshairView));
            }

            _progressStates[crosshairView] = isInProgress;
            ApplyVisibility(crosshairView, isInProgress, IsLockedOn());
        }

        /// <summary>
        ///     ロックオン状態の変化を拍入力を待たずに反映するため、毎フレーム呼び出す。
        /// </summary>
        public void Tick()
        {
            if (_progressStates.Count == 0)
            {
                return;
            }

            bool isLockedOn = IsLockedOn();
            foreach (KeyValuePair<ISkillCrosshairProgressView, bool> state in _progressStates)
            {
                ApplyVisibility(state.Key, state.Value, isLockedOn);
            }
        }

        /// <summary>
        ///     入力中かつロックオン中のときだけ照準の進捗表示を出す。
        /// </summary>
        private static void ApplyVisibility(ISkillCrosshairProgressView crosshairView, bool isInProgress, bool isLockedOn)
        {
            crosshairView.SetVisible(isInProgress && isLockedOn);
        }

        /// <summary>
        ///     ロックオン中のターゲットがいるかを判定する。
        /// </summary>
        private bool IsLockedOn()
        {
            TargetSystemController targetSystemController = _targetSystemProvider();
            return targetSystemController != null
                && targetSystemController.TryGetCurrentTargetEntity(out _);
        }

        private readonly Func<TargetSystemController> _targetSystemProvider;
        private readonly Dictionary<ISkillCrosshairProgressView, bool> _progressStates = new();
    }
}
