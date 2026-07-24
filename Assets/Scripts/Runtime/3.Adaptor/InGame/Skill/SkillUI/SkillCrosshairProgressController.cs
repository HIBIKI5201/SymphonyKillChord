using KillChord.Runtime.Adaptor.InGame.Target;
using SymphonyFrameWork.System.ServiceLocate;
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

        private static void ApplyVisibility(ISkillCrosshairProgressView crosshairView, bool isInProgress, bool isLockedOn)
        {
            crosshairView.SetVisible(isInProgress && isLockedOn);
        }

        private static bool IsLockedOn()
        {
            return ServiceLocator.TryGetInstance<TargetSystemController>(out var targetSystemController)
                && targetSystemController.TryGetCurrentTargetEntity(out _);
        }

        private readonly Dictionary<ISkillCrosshairProgressView, bool> _progressStates = new();
    }
}
