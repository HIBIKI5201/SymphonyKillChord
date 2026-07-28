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
        public bool IsTutorialCompleted => _isTutorialCompleted;

        /// <summary>
        ///     チュートリアルを完了済みにします。
        /// </summary>
        /// <returns> 状態が変化した場合はtrueです。 </returns>
        public bool Complete()
        {
            if (_isTutorialCompleted)
            {
                return false;
            }

            _isTutorialCompleted = true;
            return true;
        }

        [SerializeField, Tooltip("プレイヤーがチュートリアルを完了したかどうか")]
        private bool _isTutorialCompleted = false;
    }
}
