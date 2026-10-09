using KillChord.Editor.Utility;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.Credits
{
    /// <summary>
    ///     プレイヤービルドの前にクレジット JSON を検査します。
    /// </summary>
    public sealed class CreditJsonBuildValidator : IPreprocessBuildWithReport
    {
        /// <summary> 他のビルド前処理より先に検査する実行順です。 </summary>
        public int callbackOrder => VALIDATION_ORDER;

        private const int VALIDATION_ORDER = -200;

        /// <summary>
        ///     ビルド開始前にクレジット JSON を検査します。
        /// </summary>
        /// <param name="report"> ビルドレポートです。 </param>
        /// <exception cref="BuildFailedException"> クレジット JSON に問題がある場合に送出されます。 </exception>
        public void OnPreprocessBuild(BuildReport report)
        {
            List<string> errors = CollectErrors();
            if (errors.Count == 0)
            {
                return;
            }

            throw new BuildFailedException(CreateErrorMessage(errors));
        }

        /// <summary>
        ///     メニュー操作からクレジット JSON を検査します。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "Build/Validate Credits Json")]
        private static void ValidateFromMenu()
        {
            List<string> errors = CollectErrors();
            if (errors.Count == 0)
            {
                Debug.Log($"[{nameof(CreditJsonBuildValidator)}] クレジット JSON に問題はありません。");
                EditorUtility.DisplayDialog("Credits Json Validation", "クレジット JSON に問題はありません。", "OK");
                return;
            }

            string errorMessage = CreateErrorMessage(errors);
            Debug.LogError(errorMessage);
            EditorUtility.DisplayDialog("Credits Json Validation", errorMessage, "OK");
        }

        /// <summary>
        ///     クレジット JSON の問題を収集します。
        /// </summary>
        /// <returns> 検出した問題の一覧です。 </returns>
        private static List<string> CollectErrors()
        {
            if (!File.Exists(CreditJsonValidator.CREDIT_JSON_PATH))
            {
                return new List<string> { $"クレジット JSON が見つかりません。Path: {CreditJsonValidator.CREDIT_JSON_PATH}" };
            }

            return CreditJsonValidator.Validate(
                File.ReadAllText(CreditJsonValidator.CREDIT_JSON_PATH, Encoding.UTF8));
        }

        /// <summary>
        ///     問題の一覧をログ表示用メッセージに変換します。
        /// </summary>
        /// <param name="errors"> 検出した問題の一覧です。 </param>
        /// <returns> ログ表示用メッセージです。 </returns>
        private static string CreateErrorMessage(List<string> errors)
        {
            return $"[{nameof(CreditJsonBuildValidator)}] クレジット JSON に問題があります。\n- "
                + string.Join("\n- ", errors);
        }
    }
}
