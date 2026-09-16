using System;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Load
{
    /// <summary>
    ///     ロード画面に表示するTips1件分のデータ。タイトルと本文で構成される。
    /// </summary>
    [Serializable]
    public struct LoadingTip
    {
        /// <summary> Tipsのタイトル（例：「○○について」）。 </summary>
        public string Title => _title;

        /// <summary> Tipsの本文（例：「○○はなんとかかんとかである」）。 </summary>
        public string Body => _body;

        [SerializeField, Tooltip("Tipsのタイトル（例：「○○について」）")]
        private string _title;

        [SerializeField, TextArea(1, 4), Tooltip("Tipsの本文")]
        private string _body;
    }
}
