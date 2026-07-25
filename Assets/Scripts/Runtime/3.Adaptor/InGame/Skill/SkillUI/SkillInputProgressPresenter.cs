using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行状態をViewに反映するプレゼンタークラス。
    /// </summary>
    public class SkillInputProgressPresenter
    {
        /// <summary>
        ///     Presenterを初期化する。
        /// </summary>
        /// <param name="rowView"> 下部の入力進行UIのRowView。 </param>
        /// <param name="crosshairView"> クロスヘア上のリズムコマンドUI。未使用の場合はnull可。 </param>
        /// <param name="crosshairController"> クロスヘア上の表示権を管理するコントローラー。未使用の場合はnull可。 </param>
        public SkillInputProgressPresenter(
            ISkillInputProgressRowView rowView,
            ISkillCrosshairProgressView crosshairView = null,
            SkillCrosshairProgressController crosshairController = null)
        {
            _rowView = rowView ?? throw new ArgumentNullException(nameof(rowView));
            _crosshairView = crosshairView;
            _crosshairController = crosshairController;
        }

        /// <summary>
        ///     スキル入力進行状態を更新する。
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateRow(SkillInputProgressUpdateDTO dto)
        {
            _rowView.UpdateSteps(dto);

            if (_crosshairView == null)
            {
                return;
            }

            _crosshairView.UpdateSteps(dto);
            _crosshairController?.ReportProgress(dto.PatternMatchCount > 0, _crosshairView);
        }

        private readonly ISkillInputProgressRowView _rowView;
        private readonly ISkillCrosshairProgressView _crosshairView;
        private readonly SkillCrosshairProgressController _crosshairController;
    }
}
