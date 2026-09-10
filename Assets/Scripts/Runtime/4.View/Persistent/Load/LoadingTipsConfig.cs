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
        /// <returns> ランダムに選ばれたTipsの文字列 </returns>
        public string GetRandomTip()
        {
            if (tips == null || tips.Length == 0)
            {
                return string.Empty;
            }

            return tips[Random.Range(0, tips.Length)];
        }

        [SerializeField, TextArea(1, 4), Tooltip("ロード画面にランダムに表示されるTips")]
        private string[] tips;
    }
}
