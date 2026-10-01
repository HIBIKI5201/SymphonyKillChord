using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     回答文と、候補内に存在することを確認した引用番号を保持する。
    /// </summary>
    public sealed partial class SpecSearchAnswer
    {
        /// <summary>
        ///     検証済みの回答と引用番号を保持する。
        /// </summary>
        private SpecSearchAnswer(string text, int[] sourceNumbers)
        {
            Text = text;
            SourceNumbers = Array.AsReadOnly(sourceNumbers);
        }

        /// <summary> 回答の本文。 </summary>
        public string Text { get; }
        /// <summary> 実際に回答中へ付けた、1始まりの引用番号。 </summary>
        public IReadOnlyList<int> SourceNumbers { get; }

        /// <summary>
        ///     AIのJSONを検証し、範囲外の番号や引用のない断定を表示しない。
        /// </summary>
        public static SpecSearchAnswer Parse(string json, int sourceCount)
        {
            JObject response = JObject.Parse(json.Trim());
            if (response["answer"]?.Type != JTokenType.String || response["sources"] is not JArray sources)
            {
                throw new InvalidDataException("回答のJSON形式が不正です。");
            }
            string answer = response["answer"]!.Value<string>() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(answer) || answer.Length > MAXIMUM_ANSWER_LENGTH)
            {
                throw new InvalidDataException("回答の長さが不正です。");
            }
            List<int> numbers = new();
            foreach (JToken token in sources)
            {
                if (token.Type != JTokenType.Integer || !int.TryParse(token.ToString(), out int number)
                    || number < 1 || number > sourceCount)
                {
                    throw new InvalidDataException("回答に候補外の引用番号が含まれています。");
                }
                if (!numbers.Contains(number)) { numbers.Add(number); }
            }
            if (numbers.Count == 0)
            {
                return new SpecSearchAnswer("取得した資料では、質問への根拠を確認できませんでした。", Array.Empty<int>());
            }
            HashSet<int> inlineNumbers = new();
            foreach (Match match in CitationRegex().Matches(answer))
            {
                if (!int.TryParse(match.Groups["number"].Value, out int number))
                {
                    throw new InvalidDataException("回答中の引用番号が不正です。");
                }
                inlineNumbers.Add(number);
            }
            if (!inlineNumbers.SetEquals(numbers))
            {
                throw new InvalidDataException("回答中の引用番号と参照一覧が一致しません。");
            }
            return new SpecSearchAnswer(answer, numbers.ToArray());
        }

        private const int MAXIMUM_ANSWER_LENGTH = 2000;

        /// <summary>
        ///     回答文中の引用番号を抽出する。
        /// </summary>
        [GeneratedRegex(@"\[(?<number>[0-9]+)\]")]
        private static partial Regex CitationRegex();
    }
}
