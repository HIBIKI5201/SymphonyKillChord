using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace KillChord.Tests.EditMode.Music
{
    /// <summary>
    ///     使える拍子からゲージの範囲を作る処理を検証します。
    ///     ジャスト範囲は実際の判定定義アセットと同じ値を使います。
    /// </summary>
    public sealed class RhythmJudgmentDefinitionFactoryTests
    {
        /// <summary>
        ///     3・6拍子が未解放のときは、1・2・4・8拍子の拍の位置の中間で範囲を分けることを確認します。
        /// </summary>
        [Test]
        public void Create_WithoutNarrowBeats_SplitsAtMidpoints()
        {
            RhythmJudgmentDefinitionFactory factory = CreateFactory();

            RhythmJudgmentDefinition definition = factory.Create(
                factory.ResolveAvailableBeatTypes(Array.Empty<StageId>(), null));

            AssertRanges(definition, new[]
            {
                (BeatType.Eight, 0f, 0.1875f),
                (BeatType.Four, 0.1875f, 0.375f),
                (BeatType.Two, 0.375f, 0.75f),
                (BeatType.One, 0.75f, 1f),
            });
        }

        /// <summary>
        ///     3・6拍子を解放すると、ジャストの幅の1.5倍だけを3・6拍子にし、残りは元の拍子のままにすることを確認します。
        /// </summary>
        [Test]
        public void Create_WithNarrowBeats_CarvesJustNeighborhood()
        {
            RhythmJudgmentDefinitionFactory factory = CreateFactory();

            RhythmJudgmentDefinition definition = factory.Create(factory.AllBeatTypes);

            AssertRanges(definition, new[]
            {
                (BeatType.Eight, 0f, 0.1390625f),
                (BeatType.Six, 0.1390625f, 0.1859375f),
                (BeatType.Eight, 0.1859375f, 0.1875f),
                (BeatType.Four, 0.1875f, 0.3140625f),
                (BeatType.Three, 0.3140625f, 0.3609375f),
                (BeatType.Four, 0.3609375f, 0.375f),
                (BeatType.Two, 0.375f, 0.75f),
                (BeatType.One, 0.75f, 1f),
            });
        }

        /// <summary>
        ///     未解放の拍子のジャストは判定されず、隣の拍子になることを確認します。
        /// </summary>
        [Test]
        public void Create_WithoutNarrowBeats_IgnoresLockedJust()
        {
            RhythmJudgmentDefinitionFactory factory = CreateFactory();
            RhythmJudgmentDefinition definition = factory.Create(
                factory.ResolveAvailableBeatTypes(Array.Empty<StageId>(), null));

            Assert.That(definition.TryResolveJustBeatType(0.34f, out _), Is.False);
            Assert.That(definition.TryResolveBeatType(0.34f, out BeatType beatType), Is.True);
            Assert.That(beatType, Is.EqualTo(BeatType.Four));
        }

        /// <summary>
        ///     解放条件のステージをクリア済み、または挑戦中のときに3・6拍子を使えることを確認します。
        /// </summary>
        [Test]
        public void ResolveAvailableBeatTypes_UnlocksByClearedOrCurrentStage()
        {
            RhythmJudgmentDefinitionFactory factory = CreateFactory();

            IReadOnlyList<BeatType> locked = factory.ResolveAvailableBeatTypes(new[] { new StageId(1) }, new StageId(1));
            IReadOnlyList<BeatType> cleared = factory.ResolveAvailableBeatTypes(new[] { UNLOCK_STAGE_ID }, null);
            IReadOnlyList<BeatType> playing = factory.ResolveAvailableBeatTypes(Array.Empty<StageId>(), UNLOCK_STAGE_ID);

            Assert.That(locked, Is.EquivalentTo(new[] { BeatType.Eight, BeatType.Four, BeatType.Two, BeatType.One }));
            Assert.That(cleared, Is.EquivalentTo(factory.AllBeatTypes));
            Assert.That(playing, Is.EquivalentTo(factory.AllBeatTypes));
        }

        /// <summary>
        ///     同じ拍子の設定が重複していたら拒否することを確認します。
        /// </summary>
        [Test]
        public void Constructor_DuplicateBeatType_Throws()
        {
            BeatJudgmentSetting setting = new(BeatType.Four, 0.2f, 0.3f, false, default);

            Assert.Throws<ArgumentException>(() => new RhythmJudgmentDefinitionFactory(new[] { setting, setting }, 1.5f));
        }

        private const float TOLERANCE = 0.00001f;
        private static readonly StageId UNLOCK_STAGE_ID = new(-562443898);

        /// <summary>
        ///     判定定義アセットと同じ値のファクトリーを生成します。
        /// </summary>
        /// <returns> ファクトリーです。 </returns>
        private static RhythmJudgmentDefinitionFactory CreateFactory()
        {
            return new RhythmJudgmentDefinitionFactory(new[]
            {
                new BeatJudgmentSetting(BeatType.Eight, 0.121875f, 0.153125f, false, default),
                new BeatJudgmentSetting(BeatType.Six, 0.146875f, 0.178125f, true, UNLOCK_STAGE_ID),
                new BeatJudgmentSetting(BeatType.Four, 0.246875f, 0.278125f, false, default),
                new BeatJudgmentSetting(BeatType.Three, 0.321875f, 0.353125f, true, UNLOCK_STAGE_ID),
                new BeatJudgmentSetting(BeatType.Two, 0.496875f, 0.528125f, false, default),
                new BeatJudgmentSetting(BeatType.One, 0.996875f, 1.028125f, false, default),
            }, 1.5f);
        }

        /// <summary>
        ///     判定範囲が期待どおりの順番・拍子・位置になっているか確認します。
        /// </summary>
        /// <param name="definition"> 確認する判定定義です。 </param>
        /// <param name="expected"> 期待する拍子と範囲です。 </param>
        private static void AssertRanges(
            RhythmJudgmentDefinition definition,
            (BeatType BeatType, float Start, float End)[] expected)
        {
            IReadOnlyList<RhythmJudgmentRange> ranges = definition.JudgmentRanges;
            Assert.That(ranges.Count, Is.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(ranges[i].BeatType, Is.EqualTo(expected[i].BeatType), $"index {i}");
                Assert.That(ranges[i].StartNormalized, Is.EqualTo(expected[i].Start).Within(TOLERANCE), $"index {i}");
                Assert.That(ranges[i].EndNormalized, Is.EqualTo(expected[i].End).Within(TOLERANCE), $"index {i}");
            }
        }
    }
}
