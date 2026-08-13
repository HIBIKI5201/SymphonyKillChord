using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     複数のクロスヘア用リズムコマンドUIを1つのViewとして束ねるクラス。
    ///     同じ入力進捗をクロスヘア周辺の複数箇所へ同時に表示する場合に使う。
    /// </summary>
    public sealed class SkillCrosshairProgressViewGroup : ISkillCrosshairProgressView
    {
        /// <summary>
        ///     束ねる対象のViewを指定して初期化する。
        /// </summary>
        /// <param name="views"> 同じ進捗を表示するView群。 </param>
        /// <exception cref="ArgumentNullException"> View配列がnullの場合。 </exception>
        public SkillCrosshairProgressViewGroup(ISkillCrosshairProgressView[] views)
        {
            _views = views ?? throw new ArgumentNullException(nameof(views));
        }

        /// <inheritdoc />
        public void UpdateSteps(SkillInputProgressUpdateDTO dto)
        {
            for (int i = 0; i < _views.Length; i++)
            {
                _views[i].UpdateSteps(dto);
            }
        }

        /// <inheritdoc />
        public void SetVisible(bool visible)
        {
            for (int i = 0; i < _views.Length; i++)
            {
                _views[i].SetVisible(visible);
            }
        }

        private readonly ISkillCrosshairProgressView[] _views;
    }
}
