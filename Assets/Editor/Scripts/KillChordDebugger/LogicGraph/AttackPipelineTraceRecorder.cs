using KillChord.Runtime.Domain.InGame.Battle;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Editor.Debugger.LogicGraph
{
    /// <summary>
    ///     攻撃パイプラインの実行を、新しい順に一定数まで記録する。
    ///     <para> 記録用に包んだ攻撃ステップ（<see cref="TracingAttackStep"/>）から呼ばれる。 </para>
    /// </summary>
    public sealed class AttackPipelineTraceRecorder
    {
        /// <summary>
        ///     記録する最大件数を指定して初期化する。
        /// </summary>
        /// <param name="capacity"> 記録する最大件数。1未満は1として扱う。 </param>
        public AttackPipelineTraceRecorder(int capacity = DEFAULT_CAPACITY)
        {
            _capacity = capacity < 1 ? 1 : capacity;
        }

        /// <summary> 完了した記録（古い順）。 </summary>
        public IReadOnlyList<AttackTraceRecord> Records => _records;

        /// <summary> 記録が変わるたびに増える版番号。表示の更新判定に使う。 </summary>
        public int Version => _version;

        /// <summary> 記録を一時停止しているか。 </summary>
        public bool IsPaused => _isPaused;

        /// <summary> 既定の最大件数。 </summary>
        public const int DEFAULT_CAPACITY = 200;

        /// <summary>
        ///     記録の一時停止を切り替える。
        /// </summary>
        /// <param name="isPaused"> 一時停止する場合はtrue。 </param>
        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
        }

        /// <summary>
        ///     すべての記録を消す。
        /// </summary>
        public void Clear()
        {
            _records.Clear();
            _pending = null;
            _version++;
        }

        /// <summary>
        ///     攻撃パイプラインの実行開始を記録する。
        /// </summary>
        /// <param name="context"> 最初のステップに渡す攻撃処理の文脈。 </param>
        /// <param name="stepCount"> このパイプラインのステップ数。 </param>
        public void RecordAttackStarted(in AttackStepContext context, int stepCount)
        {
            if (_isPaused) { return; }

            // 前の実行が完了しないまま次が始まった場合は、未完了のまま残す。
            if (_pending != null) { Push(_pending); }

            string attackName = context.AttackDefinition != null
                ? context.AttackDefinition.AttackName
                : UNKNOWN_ATTACK_NAME;

            _pending = new AttackTraceRecord(
                ++_sequenceNumber,
                Time.frameCount,
                Time.realtimeSinceStartup,
                attackName,
                context.Damage.Value,
                context.IsJustHit,
                context.IsOutOfRange,
                stepCount);
        }

        /// <summary>
        ///     1つのステップの実行結果を記録する。
        /// </summary>
        /// <param name="stepIndex"> 0から数えたステップの位置。 </param>
        /// <param name="stepName"> 実行したステップの名前。 </param>
        /// <param name="before"> ステップに渡した文脈。 </param>
        /// <param name="after"> ステップが返した文脈。 </param>
        public void RecordStepExecuted(
            int stepIndex,
            string stepName,
            in AttackStepContext before,
            in AttackStepContext after)
        {
            if (_pending == null) { return; }

            AttackStepTraceEntry entry = new AttackStepTraceEntry(
                stepIndex,
                string.IsNullOrEmpty(stepName) ? UNKNOWN_STEP_NAME : stepName,
                before.Damage.Value,
                after.Damage.Value,
                after.CriticalCount);
            _pending.AddStep(entry);
        }

        /// <summary>
        ///     攻撃パイプラインの実行完了を記録する。
        /// </summary>
        /// <param name="result"> パイプラインが返した攻撃結果。 </param>
        public void RecordAttackCompleted(in AttackResult result)
        {
            if (_pending == null) { return; }

            _pending.Complete(result.FinalDamage.Value, result.IsCritical);
            Push(_pending);
            _pending = null;
        }

        private const string UNKNOWN_ATTACK_NAME = "(攻撃定義なし)";
        private const string UNKNOWN_STEP_NAME = "(不明なステップ)";

        private readonly List<AttackTraceRecord> _records = new();
        private readonly int _capacity;
        private AttackTraceRecord _pending;
        private int _sequenceNumber;
        private int _version;
        private bool _isPaused;

        /// <summary>
        ///     記録を追加し、上限を超えた古い記録を捨てる。
        /// </summary>
        /// <param name="record"> 追加する記録。 </param>
        private void Push(AttackTraceRecord record)
        {
            _records.Add(record);
            if (_records.Count > _capacity)
            {
                _records.RemoveRange(0, _records.Count - _capacity);
            }

            _version++;
        }
    }
}
