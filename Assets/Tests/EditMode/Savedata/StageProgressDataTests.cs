using KillChord.Runtime.Domain.Persistent.Savedata;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace KillChord.Tests.EditMode.Savedata
{
    /// <summary>
    ///     ステージのクリア記録と、ステージIDの移行（旧ID → 新ID）を検証します。
    /// </summary>
    public sealed class StageProgressDataTests
    {
        /// <summary>
        ///     初回クリアと新しいサブミッション達成だけを「変更あり」とすることを確認します。
        /// </summary>
        [Test]
        public void RecordClear_ReportsChangeOnlyWhenNew()
        {
            StageProgressData progress = new();

            Assert.That(progress.RecordClear(STAGE_A, new[] { "mission1" }), Is.True);
            Assert.That(progress.RecordClear(STAGE_A, new[] { "mission1" }), Is.False);
            Assert.That(progress.RecordClear(STAGE_A, new[] { " mission1 ", "mission2" }), Is.True);
            Assert.That(progress.IsStageCleared(STAGE_A), Is.True);
            Assert.That(progress.IsStageCleared(STAGE_B), Is.False);
            Assert.That(progress.ClearDatas[0].AchievedEvaluationIds, Is.EqualTo(new[] { "mission1", "mission2" }));
        }

        /// <summary>
        ///     ステージID 0 は記録できず、クリア済みにもならないことを確認します。
        /// </summary>
        [Test]
        public void RecordClear_ZeroId_Throws()
        {
            StageProgressData progress = new();

            Assert.Throws<ArgumentOutOfRangeException>(() => progress.RecordClear(0, Array.Empty<string>()));
            Assert.That(progress.IsStageCleared(0), Is.False);
        }

        /// <summary>
        ///     ID移行で同じ新IDになった記録を統合し、0へ移した記録を捨てることを確認します。
        /// </summary>
        [Test]
        public void RemapStageIds_MergesAndDropsRecords()
        {
            StageProgressData progress = new();
            progress.RecordClear(STAGE_A, new[] { "mission1" });
            progress.RecordClear(STAGE_B, new[] { "mission2" });
            progress.RecordClear(STAGE_C, Array.Empty<string>());

            bool isChanged = progress.RemapStageIds(new Dictionary<int, int>
            {
                { STAGE_A, STAGE_NEW },
                { STAGE_B, STAGE_NEW },
                { STAGE_C, 0 },
            });

            Assert.That(isChanged, Is.True);
            Assert.That(progress.ClearDatas.Count, Is.EqualTo(1));
            Assert.That(progress.ClearDatas[0].StageId, Is.EqualTo(STAGE_NEW));
            Assert.That(progress.ClearDatas[0].AchievedEvaluationIds, Is.EquivalentTo(new[] { "mission1", "mission2" }));
        }

        /// <summary>
        ///     対応表に無いIDだけなら変更なしとすることを確認します。
        /// </summary>
        [Test]
        public void RemapStageIds_NoMatch_IsUnchanged()
        {
            StageProgressData progress = new();
            progress.RecordClear(STAGE_A, Array.Empty<string>());

            Assert.That(progress.RemapStageIds(new Dictionary<int, int> { { STAGE_B, STAGE_NEW } }), Is.False);
            Assert.That(progress.IsStageCleared(STAGE_A), Is.True);
        }

        private const int STAGE_A = 101;
        private const int STAGE_B = 102;
        private const int STAGE_C = 103;
        private const int STAGE_NEW = 201;
    }
}
