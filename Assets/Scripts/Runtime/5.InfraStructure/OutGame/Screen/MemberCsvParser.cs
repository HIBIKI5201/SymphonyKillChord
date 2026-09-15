using KillChord.Runtime.Domain.OutGame.Screen;
using KillChord.Runtime.InfraStructure.Csv;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Screen
{
    /// <summary>
    ///     制作メンバー CSV を <see cref="MemberData"/> の一覧へ解析するクラス。
    /// </summary>
    /// <remarks>
    ///     CSV の列構成は「名前, 役職, 所属」です。
    ///     先頭行がヘッダーの場合は読み飛ばし、空行と <c>#</c> で始まる行はコメントとして無視します。
    ///     この型は CSV という保存形式に依存するため、InfraStructure 層の内部実装として閉じています。
    /// </remarks>
    internal sealed class MemberCsvParser
    {
        /// <summary>
        ///     制作メンバー CSV を解析します。
        /// </summary>
        /// <param name="csvText"> CSV の全文です。 </param>
        /// <returns> 解析した制作メンバー情報の一覧です。 </returns>
        public IReadOnlyList<MemberData> Parse(string csvText)
        {
            var members = new List<MemberData>();
            if (string.IsNullOrWhiteSpace(csvText))
            {
                return members;
            }

            string[] lines = csvText.Split(LINE_FEED);
            bool isHeaderChecked = false;

            for (int i = 0; i < lines.Length; i++)
            {
                // CRLF 改行に備えて行末の CR を除去する。
                string line = lines[i].TrimEnd(CARRIAGE_RETURN);

                // 先頭行の BOM は列名の一致判定を壊すため除去する。
                if (i == 0)
                {
                    line = line.TrimStart(BYTE_ORDER_MARK);
                }

                if (IsSkippableLine(line))
                {
                    continue;
                }

                List<string> fields = CsvLineParser.ParseLine(line);

                // 最初の有効行がヘッダー行なら読み飛ばす。
                if (!isHeaderChecked)
                {
                    isHeaderChecked = true;
                    if (IsHeaderLine(fields))
                    {
                        continue;
                    }
                }

                if (TryCreateMemberData(fields, i + 1, out MemberData memberData))
                {
                    members.Add(memberData);
                }
            }

            return members;
        }

        /// <summary> 名前列の列番号です。 </summary>
        private const int NAME_COLUMN_INDEX = 0;

        /// <summary> 役職列の列番号です。 </summary>
        private const int CLASS_COLUMN_INDEX = 1;

        /// <summary> 所属列の列番号です。 </summary>
        private const int AFFILIATION_COLUMN_INDEX = 2;

        /// <summary> ヘッダー行を判別するための名前列の列名です。 </summary>
        private const string NAME_COLUMN_HEADER = "名前";

        /// <summary> コメント行を示す接頭辞です。 </summary>
        private const string COMMENT_PREFIX = "#";

        /// <summary> 改行文字です。 </summary>
        private const char LINE_FEED = '\n';

        /// <summary> 復帰文字です。 </summary>
        private const char CARRIAGE_RETURN = '\r';

        /// <summary> UTF-8 BOM の文字です。 </summary>
        private const char BYTE_ORDER_MARK = '\uFEFF';

        /// <summary>
        ///     読み飛ばすべき行かどうかを判定します。
        /// </summary>
        /// <param name="line"> 判定する行です。 </param>
        /// <returns> 空行またはコメント行の場合はtrue。 </returns>
        private static bool IsSkippableLine(string line)
        {
            string trimmedLine = line.Trim();
            return trimmedLine.Length == 0 || trimmedLine.StartsWith(COMMENT_PREFIX, StringComparison.Ordinal);
        }

        /// <summary>
        ///     ヘッダー行かどうかを判定します。
        /// </summary>
        /// <param name="fields"> 列ごとの文字列一覧です。 </param>
        /// <returns> ヘッダー行の場合はtrue。 </returns>
        private static bool IsHeaderLine(IReadOnlyList<string> fields)
        {
            string firstField = CsvLineParser.GetField(fields, NAME_COLUMN_INDEX).Trim();
            return string.Equals(firstField, NAME_COLUMN_HEADER, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     1 行分の列から制作メンバー情報の生成を試みます。
        /// </summary>
        /// <param name="fields"> 列ごとの文字列一覧です。 </param>
        /// <param name="lineNumber"> 警告表示に使う行番号です。 </param>
        /// <param name="memberData"> 生成した制作メンバー情報です。 </param>
        /// <returns> 生成に成功した場合はtrue。 </returns>
        private static bool TryCreateMemberData(IReadOnlyList<string> fields, int lineNumber, out MemberData memberData)
        {
            memberData = default;

            string name = CsvLineParser.GetField(fields, NAME_COLUMN_INDEX).Trim();
            string className = CsvLineParser.GetField(fields, CLASS_COLUMN_INDEX).Trim();
            string affiliationName = CsvLineParser.GetField(fields, AFFILIATION_COLUMN_INDEX).Trim();

            // 1 行の不備で一覧全体が表示できなくなることを避けるため、不正な行は警告して読み飛ばす。
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(className) || string.IsNullOrEmpty(affiliationName))
            {
                Debug.LogWarning(
                    $"[{nameof(MemberCsvParser)}] {lineNumber} 行目に空の列があるため読み飛ばします。名前={name} 役職={className} 所属={affiliationName}");
                return false;
            }

            memberData = new MemberData(
                new MemberName(name),
                new MemberClassName(className),
                new MemberAffiliationName(affiliationName));
            return true;
        }
    }
}
