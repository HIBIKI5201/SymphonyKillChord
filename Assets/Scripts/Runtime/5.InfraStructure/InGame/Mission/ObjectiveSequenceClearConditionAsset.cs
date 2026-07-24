using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     複数の目標を順番に達成させるクリア条件のアセットクラス。
    ///     チュートリアルの練習ステップやボス戦のフェーズ管理など、ミッション全体で再利用できる。
    /// </summary>
    [Serializable]
    public class ObjectiveSequenceClearConditionAsset : MissionClearConditionAssetBase
    {
        /// <summary>
        ///     クリア条件を生成します。
        /// </summary>
        /// <returns>クリア条件。</returns>
        public override IMissionClearCondition Create()
        {
            List<ObjectiveSequenceStep> steps = new();

            if (_steps != null)
            {
                for (int i = 0; i < _steps.Count; i++)
                {
                    if (_steps[i] == null)
                    {
                        throw new InvalidOperationException($"ステップ[{i}]が未設定です。");
                    }

                    steps.Add(_steps[i].Create());
                }
            }

            return new ObjectiveSequenceClearCondition(steps);
        }

        /// <summary>
        ///     サマリーを構築します。
        /// </summary>
        /// <returns>サマリー文字列。</returns>
        protected override string BuildSummary()
        {
            if (_steps == null || _steps.Count == 0)
            {
                return "目標シーケンス（未設定）";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{_steps.Count}ステップの目標シーケンス");

            return sb.ToString();
        }

        [SerializeField, Tooltip("実行順のステップ一覧。")]
        private List<ObjectiveSequenceStepAsset> _steps = new();
    }
}
