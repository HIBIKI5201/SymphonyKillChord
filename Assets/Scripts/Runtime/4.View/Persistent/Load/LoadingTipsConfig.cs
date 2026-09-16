using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Load
{
    /// <summary>
    ///     ロード画面に表示されるTipsの設定を保持するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = nameof(LoadingTipsConfig),
        menuName = "KillChord/Persistent/LoadingTipsConfig")]
    public class LoadingTipsConfig : ScriptableObject
    {
        /// <summary>
        ///     ランダムにTipsを取得する
        /// </summary>
        /// <returns> ランダムに選ばれたTips。Tipsが1件も存在しない場合はデフォルト値。 </returns>
        public LoadingTip GetRandomTip()
        {
            if (tips == null || tips.Length == 0)
            {
                return default;
            }

            return tips[Random.Range(0, tips.Length)];
        }

        [SerializeField, Tooltip("ロード画面にランダムに表示されるTips")]
        private LoadingTip[] tips;
    }
}
