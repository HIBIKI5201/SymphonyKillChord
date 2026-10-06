using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Credits
{
    /// <summary>
    ///     クレジット JSON の取り込み・移動・削除を検知して Console に知らせます。
    /// </summary>
    public sealed class CreditJsonPostprocessor : AssetPostprocessor
    {
        /// <summary>
        ///     アセットの取り込み後に呼ばれ、クレジット JSON の変更を検査します。
        /// </summary>
        /// <param name="importedAssets"> 取り込まれたアセットです。 </param>
        /// <param name="deletedAssets"> 削除されたアセットです。 </param>
        /// <param name="movedAssets"> 移動後のアセットです。 </param>
        /// <param name="movedFromAssetPaths"> 移動前のアセットパスです。 </param>
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (Array.IndexOf(movedFromAssetPaths, CreditJsonValidator.CREDIT_JSON_PATH) >= 0)
            {
                Debug.LogError(
                    $"[{nameof(CreditJsonPostprocessor)}] {CreditJsonValidator.CREDIT_JSON_PATH} が移動されました。ゲームとホームページが読めなくなるため元の場所に戻してください。");
            }

            if (Array.IndexOf(deletedAssets, CreditJsonValidator.CREDIT_JSON_PATH) >= 0)
            {
                Debug.LogError(
                    $"[{nameof(CreditJsonPostprocessor)}] {CreditJsonValidator.CREDIT_JSON_PATH} が削除されました。ゲームとホームページが読めなくなるため元に戻してください。");
            }

            if (Array.IndexOf(importedAssets, CreditJsonValidator.CREDIT_JSON_PATH) < 0)
            {
                return;
            }

            List<string> errors =
                CreditJsonValidator.Validate(File.ReadAllText(CreditJsonValidator.CREDIT_JSON_PATH, Encoding.UTF8));
            if (errors.Count > 0)
            {
                Debug.LogError(
                    $"[{nameof(CreditJsonPostprocessor)}] {CreditJsonValidator.CREDIT_JSON_PATH} に問題があります。\n- "
                    + string.Join("\n- ", errors));
            }
        }
    }
}
