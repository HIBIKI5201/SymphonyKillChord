using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///    プレイヤーのチュートリアル進行状況のセーブデータを表すクラス。
    ///    チュートリアルを完了している場合、プレイヤーはチュートリアルをスキップできるようになる。
    /// </summary>
    [Serializable]
    public sealed class TutorialData
    {
        /// <summary>
        ///     プレイヤーがチュートリアルを完了したかどうかを示すプロパティ。
        /// </summary>
        public bool IsTutorialCompleted
        {
            get => _isTutorialCompleted;
            set => _isTutorialCompleted = value;
        }

        [SerializeField, Tooltip("プレイヤーがチュートリアルを完了したかどうか")]
        private bool _isTutorialCompleted = false;
    }
}
