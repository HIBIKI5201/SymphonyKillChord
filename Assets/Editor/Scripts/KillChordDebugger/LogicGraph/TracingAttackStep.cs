using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using System;

namespace KillChord.Editor.Debugger.LogicGraph
{
    /// <summary>
    ///     元の攻撃ステップを包み、実行の前後を記録係へ渡すエディタ専用のステップ。
    ///     <para> 処理は元のステップにそのまま任せ、結果を変えない。 </para>
    ///     <para> Play中だけ <see cref="AttackPipelineTraceInstaller"/> がアセットの配列へ差し込む。 </para>
    /// </summary>
    public sealed class TracingAttackStep : IAttackStep
    {
        /// <summary>
        ///     包む対象と、パイプライン内での位置を指定して初期化する。
        /// </summary>
        /// <param name="inner"> 元の攻撃ステップ。 </param>
        /// <param name="stepIndex"> 0から数えたステップの位置。 </param>
        /// <param name="stepCount"> パイプラインのステップ数。 </param>
        /// <param name="recorder"> 記録係。 </param>
        public TracingAttackStep(
            IAttackStep inner,
            int stepIndex,
            int stepCount,
            AttackPipelineTraceRecorder recorder)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _stepIndex = stepIndex;
            _stepCount = stepCount;
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
            _stepName = inner.GetType().Name;
        }

        /// <summary> 包んでいる元の攻撃ステップ。 </summary>
        public IAttackStep Inner => _inner;

        /// <summary>
        ///     元のステップを実行し、前後の文脈を記録する。
        ///     先頭のステップで1回分の記録を始め、最後のステップで結果を記録して閉じる。
        /// </summary>
        /// <param name="context"> 前のステップの文脈。 </param>
        /// <returns> 元のステップが返した文脈。 </returns>
        public AttackStepContext Execute(in AttackStepContext context)
        {
            if (_stepIndex == 0)
            {
                _recorder.RecordAttackStarted(context, _stepCount);
            }

            AttackStepContext result = _inner.Execute(context);
            _recorder.RecordStepExecuted(_stepIndex, _stepName, context, result);

            if (_stepIndex == _stepCount - 1)
            {
                // AttackPipeline は最後の文脈から結果を作るので、同じ作り方で記録する。
                _recorder.RecordAttackCompleted(new AttackResult(result));
            }

            return result;
        }

        private readonly IAttackStep _inner;
        private readonly int _stepIndex;
        private readonly int _stepCount;
        private readonly string _stepName;
        private readonly AttackPipelineTraceRecorder _recorder;
    }
}
