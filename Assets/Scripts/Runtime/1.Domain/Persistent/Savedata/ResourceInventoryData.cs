using KillChord.Runtime.Domain.OutGame.Resource;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     プレイヤーが所持するゲーム内リソースの数量を、リソースIDごとに保存するクラスです。
    /// </summary>
    [Serializable]
    public sealed class ResourceInventoryData
    {
        /// <summary>
        ///     指定したリソースの所持数を取得します。
        /// </summary>
        /// <param name="resourceId"> リソースIDです。 </param>
        /// <returns> 所持数です。記録が無い場合は0です。 </returns>
        public int GetAmount(GameResourceId resourceId)
        {
            ResourceAmountEntry entry = FindEntry(resourceId.Value);
            return entry?.Amount ?? 0;
        }

        /// <summary>
        ///     指定したリソースの所持数を設定します。
        /// </summary>
        /// <param name="resourceId"> リソースIDです。 </param>
        /// <param name="amount"> 設定する所持数です。0以上である必要があります。 </param>
        public void SetAmount(GameResourceId resourceId, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "リソースの所持数は 0 以上である必要があります。");
            }

            ResourceAmountEntry entry = FindEntry(resourceId.Value);
            if (entry != null)
            {
                entry.SetAmount(amount);
                return;
            }

            // 未所持のリソースへ0を設定する場合は、空のエントリを増やさない。
            if (amount == 0)
            {
                return;
            }

            Items.Add(new ResourceAmountEntry(resourceId.Value, amount));
        }

        /// <summary>
        ///     指定したリソースの所持数を加算します。
        /// </summary>
        /// <param name="resourceId"> リソースIDです。 </param>
        /// <param name="amount"> 加算する数量です。0以上である必要があります。 </param>
        public void Add(GameResourceId resourceId, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "加算する数量は 0 以上である必要があります。");
            }

            if (amount == 0)
            {
                return;
            }

            SetAmount(resourceId, checked(GetAmount(resourceId) + amount));
        }

        /// <summary>
        ///     指定したリソースを消費します。
        /// </summary>
        /// <param name="resourceId"> リソースIDです。 </param>
        /// <param name="amount"> 消費する数量です。0以上である必要があります。 </param>
        /// <returns> 所持数が足りて消費できた場合はtrueです。 </returns>
        public bool TryConsume(GameResourceId resourceId, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "消費する数量は 0 以上である必要があります。");
            }

            int currentAmount = GetAmount(resourceId);
            if (currentAmount < amount)
            {
                return false;
            }

            SetAmount(resourceId, currentAmount - amount);
            return true;
        }

        [SerializeField, Tooltip("リソースIDごとの所持数")]
        private List<ResourceAmountEntry> _entries = new List<ResourceAmountEntry>();

        /// <summary> 欠損したリストを補完した所持数の一覧です。 </summary>
        private List<ResourceAmountEntry> Items => _entries ??= new List<ResourceAmountEntry>();

        /// <summary>
        ///     指定したリソースIDのエントリを検索します。
        /// </summary>
        /// <param name="resourceId"> リソースIDの値です。 </param>
        /// <returns> 見つかったエントリです。無い場合はnullです。 </returns>
        private ResourceAmountEntry FindEntry(int resourceId)
        {
            List<ResourceAmountEntry> items = Items;
            for (int i = 0; i < items.Count; i++)
            {
                ResourceAmountEntry entry = items[i];
                if (entry != null && entry.ResourceId == resourceId)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
