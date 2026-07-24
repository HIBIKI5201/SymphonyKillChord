using KillChord.Runtime.Adaptor.InGame.Target;
using SymphonyFrameWork.System.ServiceLocate;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     クロスヘア上のリズムコマンドView群の表示を仲介するコントローラー。
    ///     同じ入力履歴で複数スキルが同時に進行中の場合（同じ拍で始動した場合）は、そのすべてを表示する。
    ///     ロックオン対象が存在しない（クロスヘア非表示の）間は表示しない。
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

            bool isLockedOn = ServiceLocator.TryGetInstance<TargetSystemController>(out var targetSystemController)
                && targetSystemController.TryGetCurrentTargetEntity(out _);

            crosshairView.SetVisible(isInProgress && isLockedOn);
        }
    }
}
