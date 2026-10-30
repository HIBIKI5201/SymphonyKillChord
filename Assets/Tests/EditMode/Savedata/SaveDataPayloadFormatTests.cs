using KillChord.Runtime.InfraStructure.Persistent.Savedata;
using NUnit.Framework;
using System;

namespace KillChord.Tests.EditMode.Savedata
{
    /// <summary>
    ///     セーブデータ本体にバージョン番号を付けて包む処理と、外して移す処理を検証します。
    /// </summary>
    public sealed class SaveDataPayloadFormatTests
    {
        /// <summary>
        ///     包んだものを外すと、今のバージョン番号と元の本体JSONに戻ることを確認します。
        /// </summary>
        [Test]
        public void Wrap_ThenUnwrap_ReturnsCurrentVersionAndJson()
        {
            string wrapped = SaveDataPayloadFormat.Wrap(SAMPLE_JSON);

            bool isRead = SaveDataPayloadFormat.TryUnwrap(wrapped, out int version, out string json);

            Assert.That(isRead, Is.True);
            Assert.That(version, Is.EqualTo(SaveDataPayloadFormat.CURRENT_VERSION));
            Assert.That(json, Is.EqualTo(SAMPLE_JSON));
        }

        /// <summary>
        ///     バージョン番号の無い以前の形式は、バージョン0として文字列全体を本体JSONにすることを確認します。
        /// </summary>
        [Test]
        public void TryUnwrap_LegacyJson_ReturnsVersionZeroAndWholeText()
        {
            bool isRead = SaveDataPayloadFormat.TryUnwrap(SAMPLE_JSON, out int version, out string json);

            Assert.That(isRead, Is.True);
            Assert.That(version, Is.EqualTo(0));
            Assert.That(json, Is.EqualTo(SAMPLE_JSON));
        }

        /// <summary>
        ///     JSONとして壊れた文字列と空の文字列は、読めないと判定することを確認します。
        /// </summary>
        [TestCase("{\"_stageIds\":[")]
        [TestCase("")]
        [TestCase(null)]
        public void TryUnwrap_BrokenText_ReturnsFalse(string text)
        {
            bool isRead = SaveDataPayloadFormat.TryUnwrap(text, out _, out _);

            Assert.That(isRead, Is.False);
        }

        /// <summary>
        ///     以前の形式の本体JSONは、形を変えずに今の形式へ移すことを確認します。
        /// </summary>
        [Test]
        public void MigrateToCurrent_LegacyVersion_KeepsJson()
        {
            string json = SaveDataPayloadFormat.MigrateToCurrent(0, SAMPLE_JSON);

            Assert.That(json, Is.EqualTo(SAMPLE_JSON));
        }

        /// <summary>
        ///     今のアプリより新しい形式は移せないことを確認します。
        /// </summary>
        [Test]
        public void MigrateToCurrent_NewerVersion_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => SaveDataPayloadFormat.MigrateToCurrent(SaveDataPayloadFormat.CURRENT_VERSION + 1, SAMPLE_JSON));
        }

        private const string SAMPLE_JSON = "{\"_stageIds\":[\"stage_01\"],\"_saveDate\":\"2026-10-30\"}";
    }
}
