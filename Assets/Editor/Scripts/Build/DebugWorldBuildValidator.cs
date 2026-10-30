using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace KillChord.Editor.Build
{
    /// <summary>
    ///     裏世界のシーンが開発ビルドにだけ入るよう、ビルドの前に確かめます。
    ///     <para> 裏世界のコードは開発ビルドでしかコンパイルされないため、リリースビルドに裏世界のシーンが入るとスクリプトが欠けます。 </para>
    /// </summary>
    public sealed class DebugWorldBuildValidator : BuildPlayerProcessor
    {
        /// <summary> Addressablesの梱包より前に確かめる実行順です。 </summary>
        public override int callbackOrder => VALIDATION_ORDER;

        /// <summary>
        ///     開発ビルドでないのに裏世界のシーンが入っていたら、ビルドを止めます。
        /// </summary>
        /// <param name="buildPlayerContext"> 対象のPlayerビルドです。 </param>
        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            BuildPlayerOptions options = buildPlayerContext.BuildPlayerOptions;
            string[] debugScenes = (options.scenes ?? Array.Empty<string>())
                .Where(IsDebugWorldScene)
                .ToArray();
            bool isDevelopment = (options.options & BuildOptions.Development) != 0;

            if (!isDevelopment && debugScenes.Length > 0)
            {
                throw new BuildFailedException(
                    $"[{nameof(DebugWorldBuildValidator)}] 開発ビルドではないのに裏世界のシーンが含まれています。"
                    + $"Build Profileのシーンリストから外してください: {string.Join(", ", debugScenes)}");
            }

            if (isDevelopment && debugScenes.Length == 0)
            {
                Debug.LogWarning(
                    $"[{nameof(DebugWorldBuildValidator)}] 開発ビルドですが裏世界のシーンが含まれていないため、裏世界は開けません。");
            }
        }

        private const int VALIDATION_ORDER = -1100;
        private const string DEBUG_WORLD_SCENE_FOLDER = "Assets/Level/Scenes/Debug/";

        /// <summary>
        ///     裏世界のシーンかを、置き場所のフォルダで判定します。
        /// </summary>
        /// <param name="scenePath"> シーンのパスです。 </param>
        /// <returns> 裏世界のシーンの場合はtrueです。 </returns>
        private static bool IsDebugWorldScene(string scenePath)
        {
            return scenePath != null
                && scenePath.Replace('\\', '/').StartsWith(DEBUG_WORLD_SCENE_FOLDER, StringComparison.Ordinal);
        }
    }
}
