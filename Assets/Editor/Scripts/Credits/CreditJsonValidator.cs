using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Editor.Credits
{
    /// <summary>
    ///     クレジット JSON の中身を検査し、問題の一覧を返します。
    /// </summary>
    /// <remarks>
    ///     ゲーム側の <c>MemberJsonRepository</c> と同じキー名・同じ判定（役職表記は <c>roleDetail</c> が空なら <c>role</c>）で調べます。
    /// </remarks>
    public static class CreditJsonValidator
    {
        /// <summary> クレジット JSON のアセットパスです。 </summary>
        public const string CREDIT_JSON_PATH = "Assets/StreamingAssets/Credits/credits.json";

        /// <summary> クレジット JSON を置くフォルダのアセットパスです。 </summary>
        public const string CREDIT_FOLDER_PATH = "Assets/StreamingAssets/Credits";

        /// <summary>
        ///     クレジット JSON の全文を検査します。
        /// </summary>
        /// <param name="jsonText"> クレジット JSON の全文です。 </param>
        /// <returns> 検出した問題の一覧です。問題が無ければ空です。 </returns>
        public static List<string> Validate(string jsonText)
        {
            List<string> errors = new();
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                errors.Add("JSON が空です。");
                return errors;
            }

            CreditDocument document;
            try
            {
                document = JsonUtility.FromJson<CreditDocument>(jsonText);
            }
            catch (ArgumentException e)
            {
                errors.Add($"JSON として読めません。{e.Message}");
                return errors;
            }

            if (document?.categories == null || document.categories.Length == 0)
            {
                errors.Add("categories がありません。");
                return errors;
            }

            HashSet<string> names = new();
            for (int c = 0; c < document.categories.Length; c++)
            {
                ValidateCategory(document.categories[c], c, names, errors);
            }

            return errors;
        }

        /// <summary>
        ///     カテゴリ1件分を検査します。
        /// </summary>
        /// <param name="category"> 検査するカテゴリです。 </param>
        /// <param name="categoryIndex"> カテゴリの添字です。 </param>
        /// <param name="names"> これまでに出現した名前です。 </param>
        /// <param name="errors"> 検出した問題の一覧です。 </param>
        private static void ValidateCategory(
            CreditCategory category,
            int categoryIndex,
            HashSet<string> names,
            List<string> errors)
        {
            string location = $"categories[{categoryIndex}]";
            if (category?.roles == null)
            {
                errors.Add($"{location}: roles がありません。");
                return;
            }

            for (int r = 0; r < category.roles.Length; r++)
            {
                CreditRole role = category.roles[r];
                string roleLocation = $"{location}.roles[{r}]";
                if (role?.names == null)
                {
                    errors.Add($"{roleLocation}: names がありません。");
                    continue;
                }

                for (int n = 0; n < role.names.Length; n++)
                {
                    ValidateName(role, role.names[n], $"{roleLocation}.names[{n}]", names, errors);
                }
            }
        }

        /// <summary>
        ///     名前1件分を検査します。
        /// </summary>
        /// <param name="role"> 名前が属する役職です。 </param>
        /// <param name="entry"> 検査する名前です。 </param>
        /// <param name="location"> エラー表示用の位置です。 </param>
        /// <param name="names"> これまでに出現した名前です。 </param>
        /// <param name="errors"> 検出した問題の一覧です。 </param>
        private static void ValidateName(
            CreditRole role,
            CreditName entry,
            string location,
            HashSet<string> names,
            List<string> errors)
        {
            if (entry == null)
            {
                errors.Add($"{location}: 記載が空です。");
                return;
            }

            string name = entry.name?.Trim() ?? string.Empty;
            string className = string.IsNullOrWhiteSpace(entry.roleDetail) ? role.role : entry.roleDetail;

            if (name.Length == 0)
            {
                errors.Add($"{location}: name が空です。");
            }
            else if (!names.Add(name))
            {
                errors.Add($"{location}: 名前「{name}」が重複しています。");
            }

            if (string.IsNullOrWhiteSpace(className))
            {
                errors.Add($"{location}: roleDetail と role がどちらも空です。");
            }

            if (string.IsNullOrWhiteSpace(entry.affiliation))
            {
                errors.Add($"{location}: affiliation が空です。");
            }
        }

        // 以下は JsonUtility で読むための型で、MemberJsonRepository の型とキー名を揃える必要がある。

        /// <summary> クレジット JSON のルート。 </summary>
        [Serializable]
        private sealed class CreditDocument
        {
            /// <summary> カテゴリの一覧です。 </summary>
            public CreditCategory[] categories;
        }

        /// <summary> クレジットのカテゴリ。 </summary>
        [Serializable]
        private sealed class CreditCategory
        {
            /// <summary> カテゴリ名です。 </summary>
            public string category;

            /// <summary> カテゴリ内の役職の一覧です。 </summary>
            public CreditRole[] roles;
        }

        /// <summary> まとめた役職。 </summary>
        [Serializable]
        private sealed class CreditRole
        {
            /// <summary> まとめた役職名です。 </summary>
            public string role;

            /// <summary> 役職に属する名前の一覧です。 </summary>
            public CreditName[] names;
        }

        /// <summary> 名前1件分の記載。 </summary>
        [Serializable]
        private sealed class CreditName
        {
            /// <summary> 名前です。 </summary>
            public string name;

            /// <summary> 読み仮名です。 </summary>
            public string reading;

            /// <summary> ゲーム内で表示する役職表記です。 </summary>
            public string roleDetail;

            /// <summary> 所属です。 </summary>
            public string affiliation;
        }
    }
}
