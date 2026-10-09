using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Credits
{
    /// <summary>
    ///     クレジット JSON と Credits フォルダのエディタ上での移動・名前変更・削除を拒否します。
    /// </summary>
    /// <remarks>
    ///     ホームページは相対パスで、ゲームは StreamingAssets の固定パスでこの JSON を読むため、場所を変えられないようにします。
    /// </remarks>
    public sealed class CreditJsonModificationGuard : AssetModificationProcessor
    {
        /// <summary>
        ///     アセットの移動・名前変更の前に呼ばれます。
        /// </summary>
        /// <param name="sourcePath"> 移動元のパスです。 </param>
        /// <param name="destinationPath"> 移動先のパスです。 </param>
        /// <returns> 保護対象なら FailedMove、それ以外は DidNotMove です。 </returns>
        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (!IsProtected(sourcePath))
            {
                return AssetMoveResult.DidNotMove;
            }

            Debug.LogError(
                $"[{nameof(CreditJsonModificationGuard)}] {sourcePath} はゲームとホームページが固定パスで読むため、移動・名前変更できません。");
            return AssetMoveResult.FailedMove;
        }

        /// <summary>
        ///     アセットの削除の前に呼ばれます。
        /// </summary>
        /// <param name="assetPath"> 削除するパスです。 </param>
        /// <param name="options"> 削除の設定です。 </param>
        /// <returns> 保護対象なら FailedDelete、それ以外は DidNotDelete です。 </returns>
        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (!IsProtected(assetPath))
            {
                return AssetDeleteResult.DidNotDelete;
            }

            Debug.LogError(
                $"[{nameof(CreditJsonModificationGuard)}] {assetPath} はゲームとホームページが読むため、削除できません。");
            return AssetDeleteResult.FailedDelete;
        }

        /// <summary>
        ///     保護対象のパスかどうかを判定します。
        /// </summary>
        /// <param name="path"> 判定するアセットパスです。 </param>
        /// <returns> クレジット JSON か、それを含むフォルダ（祖先を含む）なら true。 </returns>
        private static bool IsProtected(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string normalized = path.Replace('\\', '/').TrimEnd('/');

            // 自身か、保護対象を含むフォルダ（StreamingAssets や Assets ごとの移動も含む）を対象にする。
            return normalized == CreditJsonValidator.CREDIT_JSON_PATH
                || CreditJsonValidator.CREDIT_JSON_PATH.StartsWith(normalized + "/", System.StringComparison.Ordinal);
        }
    }
}
