using System;

namespace KillChord.Runtime.Utility.Constant
{
    /// <summary>
    ///     シーン名からロード優先度を解決するクラス。
    /// </summary>
    public static class ScenePriorityResolver
    {
        /// <summary> 常駐シーンの優先度です。 </summary>
        public const int PERSISTENT_SCENE_PRIORITY = -1;

        /// <summary> 基本シーンの優先度です。 </summary>
        public const int DEFAULT_SCENE_PRIORITY = 0;

        /// <summary> ステージシーンの優先度です。 </summary>
        public const int STAGE_SCENE_PRIORITY = 10;

        /// <summary>
        ///     シーン名から優先度を解決します。
        /// </summary>
        /// <param name="sceneName"> シーン名です。 </param>
        /// <returns> 解決した優先度です。 </returns>
        public static int Resolve(string sceneName)
        {
            if (string.Equals(sceneName, PERSISTENT_SCENE_NAME, StringComparison.Ordinal))
            {
                return PERSISTENT_SCENE_PRIORITY;
            }

            if (sceneName != null
                && sceneName.StartsWith(STAGE_SCENE_PREFIX, StringComparison.Ordinal))
            {
                return STAGE_SCENE_PRIORITY;
            }

            return DEFAULT_SCENE_PRIORITY;
        }

        private const string PERSISTENT_SCENE_NAME = "Persistent";
        private const string STAGE_SCENE_PREFIX = "Stage";
    }
}
