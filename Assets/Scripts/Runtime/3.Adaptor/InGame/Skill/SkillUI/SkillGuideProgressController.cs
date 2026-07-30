using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     リズムGUI下部のスキルコマンド表示を、全スキルの入力進捗から横断的に制御するコントローラー。
    ///     いずれかのスキルの入力が始動している間は、最も進捗が進んでいるスキルのコマンドのみを表示する。
    ///     入力が進むにつれて候補から外れたスキルが消えていき、確定した時点でそのスキルだけが残る。
    ///     未入力状態（全スキルの進捗が0）では全スキルの走り出しを表示する。
    ///     <para>
    ///         スキル判定は入力履歴のサフィックス一致であるため、コマンド入力の途中でも別スキルの走り出しが
    ///         成立してしまう（例：(2,4,1)の2拍目で(4,4,4,4)が1拍一致する）。表示上はプレイヤーの意図に近い
    ///         最長一致のみを見せ、予想が外れた場合は次の入力で自動的に切り替わる。
    ///     </para>
    /// </summary>
    public sealed class SkillGuideProgressController
    {
        /// <summary>
        ///     表示対象のスキルコマンドViewを登録する。
        /// </summary>
        /// <param name="guideView"> 登録するView。 </param>
        /// <exception cref="ArgumentNullException"> Viewがnullの場合。 </exception>
        public void Register(ISkillInputProgressRowView guideView)
        {
            if (guideView == null)
            {
                throw new ArgumentNullException(nameof(guideView));
            }

            _patternMatchCounts[guideView] = 0;
            ApplyVisibility();
        }

        /// <summary>
        ///     スキルごとの入力進捗を報告する。
        /// </summary>
        /// <param name="patternMatchCount"> マッチしている入力拍子数。 </param>
        /// <param name="guideView"> 報告元スキルのView。 </param>
        /// <exception cref="ArgumentNullException"> Viewがnullの場合。 </exception>
        public void ReportProgress(int patternMatchCount, ISkillInputProgressRowView guideView)
        {
            if (guideView == null)
            {
                throw new ArgumentNullException(nameof(guideView));
            }

            _patternMatchCounts[guideView] = patternMatchCount;
            ApplyVisibility();
        }

        /// <summary>
        ///     現在の進捗状況に応じて各Viewの表示可否を反映する。
        /// </summary>
        private void ApplyVisibility()
        {
            int maxMatchCount = 0;
            foreach (KeyValuePair<ISkillInputProgressRowView, int> entry in _patternMatchCounts)
            {
                if (entry.Value > maxMatchCount)
                {
                    maxMatchCount = entry.Value;
                }
            }

            foreach (KeyValuePair<ISkillInputProgressRowView, int> entry in _patternMatchCounts)
            {
                // 始動中は最長一致のスキルのみ、未入力状態なら全スキルを表示する。
                entry.Key.SetVisible(maxMatchCount <= 0 || entry.Value == maxMatchCount);
            }
        }

        private readonly Dictionary<ISkillInputProgressRowView, int> _patternMatchCounts = new();
    }
}
