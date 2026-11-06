using KillChord.Editor.Debugger.LogicGraph;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Battle;
using NUnit.Framework;

namespace KillChord.Tests.EditMode.Battle
{
    /// <summary>
    ///     攻撃パイプラインの実行記録（ロジックのグラフ表示用）を検証します。
    /// </summary>
    public sealed class AttackPipelineTraceTests
    {
        /// <summary>
        ///     観測者に、開始・各ステップの前後のダメージ・結果が順に記録されることを確認します。
        /// </summary>
        [Test]
        public void Execute_WithTracer_RecordsEachStep()
        {
            AttackPipeline pipeline = new AttackPipeline(new IAttackStep[]
            {
                new MultiplyStep(2f),
                new MultiplyStep(1f),
                new MultiplyStep(1.5f)
            });
            AttackPipelineTraceRecorder recorder = new AttackPipelineTraceRecorder();

            AttackResult result = pipeline.Execute(CreateContext(10f, isJustHit: true), recorder);

            Assert.That(result.FinalDamage.Value, Is.EqualTo(30f).Within(TOLERANCE));
            Assert.That(recorder.Records.Count, Is.EqualTo(1));

            AttackTraceRecord record = recorder.Records[0];
            Assert.That(record.IsCompleted, Is.True);
            Assert.That(record.IsJustHit, Is.True);
            Assert.That(record.BaseDamage, Is.EqualTo(10f).Within(TOLERANCE));
            Assert.That(record.FinalDamage, Is.EqualTo(30f).Within(TOLERANCE));
            Assert.That(record.StepCount, Is.EqualTo(3));
            Assert.That(record.Steps.Count, Is.EqualTo(3));

            Assert.That(record.Steps[0].StepName, Is.EqualTo(nameof(MultiplyStep)));
            Assert.That(record.Steps[0].DamageBefore, Is.EqualTo(10f).Within(TOLERANCE));
            Assert.That(record.Steps[0].DamageAfter, Is.EqualTo(20f).Within(TOLERANCE));
            Assert.That(record.Steps[0].HasDamageChanged, Is.True);
            Assert.That(record.Steps[1].HasDamageChanged, Is.False);
            Assert.That(record.Steps[2].DamageAfter, Is.EqualTo(30f).Within(TOLERANCE));
        }

        /// <summary>
        ///     観測者を渡さなくても、結果が観測者ありの場合と同じになることを確認します。
        /// </summary>
        [Test]
        public void Execute_WithoutTracer_ReturnsSameResult()
        {
            AttackPipeline pipeline = new AttackPipeline(new IAttackStep[] { new MultiplyStep(3f) });

            AttackResult withoutTracer = pipeline.Execute(CreateContext(4f), null);
            AttackResult withTracer = pipeline.Execute(CreateContext(4f), new AttackPipelineTraceRecorder());

            Assert.That(withoutTracer.FinalDamage.Value, Is.EqualTo(withTracer.FinalDamage.Value).Within(TOLERANCE));
        }

        /// <summary>
        ///     上限を超えた古い記録が捨てられ、一時停止中は記録されないことを確認します。
        /// </summary>
        [Test]
        public void Recorder_DropsOldRecordsAndHonorsPause()
        {
            AttackPipeline pipeline = new AttackPipeline(new IAttackStep[] { new MultiplyStep(1f) });
            AttackPipelineTraceRecorder recorder = new AttackPipelineTraceRecorder(capacity: 2);

            pipeline.Execute(CreateContext(1f), recorder);
            pipeline.Execute(CreateContext(2f), recorder);
            pipeline.Execute(CreateContext(3f), recorder);

            Assert.That(recorder.Records.Count, Is.EqualTo(2));
            Assert.That(recorder.Records[0].BaseDamage, Is.EqualTo(2f).Within(TOLERANCE));
            Assert.That(recorder.Records[1].BaseDamage, Is.EqualTo(3f).Within(TOLERANCE));

            int version = recorder.Version;
            recorder.SetPaused(true);
            pipeline.Execute(CreateContext(4f), recorder);

            Assert.That(recorder.Records.Count, Is.EqualTo(2));
            Assert.That(recorder.Version, Is.EqualTo(version));
        }

        private const float TOLERANCE = 0.0001f;

        /// <summary>
        ///     攻撃定義・攻撃者・防御者なしの攻撃処理の文脈を作ります。
        /// </summary>
        /// <param name="baseDamage"> 基礎ダメージ。 </param>
        /// <param name="isJustHit"> Justで当たったか。 </param>
        /// <returns> 攻撃処理の文脈。 </returns>
        private static AttackStepContext CreateContext(float baseDamage, bool isJustHit = false)
        {
            return new AttackStepContext(null, null, null, isJustHit, new Damage(baseDamage));
        }

        /// <summary>
        ///     ダメージに倍率を掛けるだけのテスト用ステップ。
        /// </summary>
        private sealed class MultiplyStep : IAttackStep
        {
            /// <summary>
            ///     倍率を指定して初期化します。
            /// </summary>
            /// <param name="multiplier"> ダメージに掛ける倍率。 </param>
            public MultiplyStep(float multiplier)
            {
                _multiplier = multiplier;
            }

            /// <summary>
            ///     ダメージに倍率を掛けた文脈を返します。
            /// </summary>
            /// <param name="context"> 前のステップの文脈。 </param>
            /// <returns> 倍率を掛けた文脈。 </returns>
            public AttackStepContext Execute(in AttackStepContext context)
            {
                return new AttackStepContext(context.Damage * _multiplier, context.CriticalCount, context);
            }

            private readonly float _multiplier;
        }
    }
}
