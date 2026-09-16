using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     リソースIDごとの所持数を保存するためのエントリです。
    /// </summary>
    [Serializable]
    public sealed class ResourceAmountEntry
    {
        /// <summary>
        ///     ResourceAmountEntry の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="resourceId"> リソースIDの値です。 </param>
        /// <param name="amount"> 所持数です。 </param>
        public ResourceAmountEntry(int resourceId, int amount)
        {
            _resourceId = resourceId;
            SetAmount(amount);
        }

        /// <summary> リソースIDの値です。 </summary>
        public int ResourceId => _resourceId;

        /// <summary> 所持数です。 </summary>
        public int Amount => _amount;

        /// <summary>
        ///     所持数を設定します。
        /// </summary>
        /// <param name="amount"> 設定する所持数です。0以上である必要があります。 </param>
        public void SetAmount(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "リソースの所持数は 0 以上である必要があります。");
            }

            _amount = amount;
        }

        [SerializeField, Tooltip("リソースID")]
        private int _resourceId;

        [SerializeField, Tooltip("所持数")]
        private int _amount;
    }
}
