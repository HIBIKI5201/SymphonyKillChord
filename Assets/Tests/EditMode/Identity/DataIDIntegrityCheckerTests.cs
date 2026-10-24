using KillChord.Editor.SourceDataProvider.Core;
using KillChord.Runtime.Utility.Identity;
using NUnit.Framework;
using System.Collections.Generic;

namespace KillChord.Tests.EditMode.Identity
{
    /// <summary>
    ///     DataIDのハッシュ計算と、ビルド前検証で使う整合チェックを検証します。
    /// </summary>
    public sealed class DataIDIntegrityCheckerTests
    {
        /// <summary>
        ///     ハッシュは同じ入力で常に同じ値になり、空の入力は0、CollectionKeyが違えば別の値になることを確認します。
        /// </summary>
        [Test]
        public void DataIDHasher_IsStableAndScopedByCollection()
        {
            Assert.That(DataIDHasher.Compute(SKILL, "fire"), Is.EqualTo(DataIDHasher.Compute(SKILL, "fire")));
            Assert.That(DataIDHasher.Compute(SKILL, "fire"), Is.Not.EqualTo(DataIDHasher.Compute(STAGE, "fire")));
            Assert.That(DataIDHasher.Compute(SKILL, " "), Is.EqualTo(0));
            Assert.That(DataIDHasher.Compute(string.Empty, "fire"), Is.EqualTo(0));
        }

        /// <summary>
        ///     正しく焼き込まれた登録と参照だけなら不整合なしになることを確認します。
        /// </summary>
        [Test]
        public void Validate_ConsistentData_HasNoIssues()
        {
            DataIDIntegrityChecker checker = new();
            checker.AddRegisteredId(RELEASE, SKILL, "fire", Hash(SKILL, "fire"), "skills");
            checker.AddReference(SKILL, "fire", Hash(SKILL, "fire"), "loadout");
            checker.AddReference(SKILL, string.Empty, 0, "optional");

            Assert.That(checker.Validate(), Is.Empty);
            Assert.That(checker.CheckedCount, Is.EqualTo(2));
        }

        /// <summary>
        ///     同じ範囲での重複登録を検出し、別の範囲（ゲームデータ種別）なら重複にしないことを確認します。
        /// </summary>
        [Test]
        public void Validate_DuplicateInSameScope_IsReported()
        {
            DataIDIntegrityChecker checker = new();
            checker.AddRegisteredId(RELEASE, SKILL, "fire", Hash(SKILL, "fire"), "a");
            checker.AddRegisteredId(DEMO, SKILL, "fire", Hash(SKILL, "fire"), "b");
            Assert.That(checker.Validate(), Is.Empty);

            checker.AddRegisteredId(RELEASE, SKILL, "fire", Hash(SKILL, "fire"), "c");
            IReadOnlyList<string> issues = checker.Validate();

            Assert.That(issues.Count, Is.EqualTo(1));
            StringAssert.StartsWith("重複:", issues[0]);
        }

        /// <summary>
        ///     保存されたハッシュが計算値と違う（焼き込み忘れ）ことを検出することを確認します。
        /// </summary>
        [Test]
        public void Validate_StaleHash_IsReported()
        {
            DataIDIntegrityChecker checker = new();
            checker.AddRegisteredId(RELEASE, SKILL, "fire", Hash(SKILL, "fire"), "skills");
            checker.AddReference(SKILL, "fire", Hash(SKILL, "fire") + 1, "loadout");

            IReadOnlyList<string> issues = checker.Validate();

            Assert.That(issues.Count, Is.EqualTo(1));
            StringAssert.StartsWith("焼き込み忘れ:", issues[0]);
        }

        /// <summary>
        ///     どの範囲にも登録されていないIDへの参照を検出することを確認します。
        /// </summary>
        [Test]
        public void Validate_UnknownReference_IsReported()
        {
            DataIDIntegrityChecker checker = new();
            checker.AddRegisteredId(DEMO, SKILL, "fire", Hash(SKILL, "fire"), "skills");
            checker.AddReference(SKILL, "fire", Hash(SKILL, "fire"), "demo loadout");
            checker.AddReference(SKILL, "ice", Hash(SKILL, "ice"), "loadout");

            IReadOnlyList<string> issues = checker.Validate();

            Assert.That(issues.Count, Is.EqualTo(1));
            StringAssert.StartsWith("参照切れ:", issues[0]);
            StringAssert.Contains("ice", issues[0]);
        }

        /// <summary>
        ///     同じ内容の不整合は1件にまとめることを確認します。
        /// </summary>
        [Test]
        public void AddIssue_DeduplicatesMessages()
        {
            DataIDIntegrityChecker checker = new();
            checker.AddIssue("same");
            checker.AddIssue("same");
            checker.AddIssue(" ");

            Assert.That(checker.IssueCount, Is.EqualTo(1));
        }

        /// <summary>
        ///     CollectionKeyと文字列IDから正しいハッシュを計算します。
        /// </summary>
        /// <param name="collectionKey"> IDのCollectionKeyです。 </param>
        /// <param name="id"> 文字列IDです。 </param>
        /// <returns> 数値IDです。 </returns>
        private static int Hash(string collectionKey, string id)
        {
            return DataIDHasher.Compute(collectionKey, id);
        }

        private const string RELEASE = "Release";
        private const string DEMO = "Demo";
        private const string SKILL = "Skill";
        private const string STAGE = "StageAsset";
    }
}
