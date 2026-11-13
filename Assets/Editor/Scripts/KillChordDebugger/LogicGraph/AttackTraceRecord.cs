using System.Collections.Generic;

namespace KillChord.Editor.Debugger.LogicGraph
{
    /// <summary>
    ///     攻撃パイプライン1回分の実行記録。
    /// </summary>
    public sealed class AttackTraceRecord
    {
        /// <summary>
        ///     実行開始時の情報で記録を初期化する。
        /// </summary>
        /// <param name="sequenceNumber"> 記録の通し番号。 </param>
        /// <param name="frameCount"> 実行したフレーム番号。 </param>
        /// <param name="time"> 実行した時刻（秒）。 </param>
        /// <param name="attackName"> 攻撃定義の名前。 </param>
        /// <param name="baseDamage"> パイプラインに渡した基礎ダメージ。 </param>
        /// <param name="isJustHit"> Justで当たったか。 </param>
        /// <param name="isOutOfRange"> 射程外だったか。 </param>
        /// <param name="stepCount"> パイプラインのステップ数。 </param>
        public AttackTraceRecord(
            int sequenceNumber,
            int frameCount,
            float time,
            string attackName,
            float baseDamage,
            bool isJustHit,
            bool isOutOfRange,
            int stepCount)
        {
            SequenceNumber = sequenceNumber;
            FrameCount = frameCount;
            Time = time;
            AttackName = attackName;
            BaseDamage = baseDamage;
            IsJustHit = isJustHit;
            IsOutOfRange = isOutOfRange;
            StepCount = stepCount;
            _steps = new List<AttackStepTraceEntry>(stepCount);
        }

        /// <summary> 記録の通し番号。 </summary>
        public int SequenceNumber { get; }

        /// <summary> 実行したフレーム番号。 </summary>
        public int FrameCount { get; }

        /// <summary> 実行した時刻（秒）。 </summary>
        public float Time { get; }

        /// <summary> 攻撃定義の名前。 </summary>
        public string AttackName { get; }

        /// <summary> パイプラインに渡した基礎ダメージ。 </summary>
        public float BaseDamage { get; }

        /// <summary> Justで当たったか。 </summary>
        public bool IsJustHit { get; }

        /// <summary> 射程外だったか。 </summary>
        public bool IsOutOfRange { get; }

        /// <summary> パイプラインのステップ数。 </summary>
        public int StepCount { get; }

        /// <summary> 実行したステップの記録。 </summary>
        public IReadOnlyList<AttackStepTraceEntry> Steps => _steps;

        /// <summary> 最後まで実行して結果を受け取ったか。 </summary>
        public bool IsCompleted => _isCompleted;

        /// <summary> 最終ダメージ。完了していない場合は0。 </summary>
        public float FinalDamage => _finalDamage;

        /// <summary> クリティカルになったか。 </summary>
        public bool IsCritical => _isCritical;

        /// <summary>
        ///     ステップの記録を追加する。
        /// </summary>
        /// <param name="entry"> 追加するステップの記録。 </param>
        public void AddStep(in AttackStepTraceEntry entry)
        {
            _steps.Add(entry);
        }

        /// <summary>
        ///     実行結果を記録して完了にする。
        /// </summary>
        /// <param name="finalDamage"> 最終ダメージ。 </param>
        /// <param name="isCritical"> クリティカルになったか。 </param>
        public void Complete(float finalDamage, bool isCritical)
        {
            _finalDamage = finalDamage;
            _isCritical = isCritical;
            _isCompleted = true;
        }

        private readonly List<AttackStepTraceEntry> _steps;
        private float _finalDamage;
        private bool _isCritical;
        private bool _isCompleted;
    }
}
