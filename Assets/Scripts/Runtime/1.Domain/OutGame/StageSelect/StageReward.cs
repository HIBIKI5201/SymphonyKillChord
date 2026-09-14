using KillChord.Runtime.Domain.OutGame.Resource;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.OutGame.StageSelect
{
    /// <summary>
    ///     ステージクリア時に付与するリソースの一覧を表す値オブジェクト。
    /// </summary>
    public readonly struct StageReward
    {
        /// <summary>
        ///     ステージクリア報酬を初期化する。
        /// </summary>
        /// <param name="items"> 付与するリソースと数量の一覧。nullの場合は報酬なしとして扱う。 </param>
        public StageReward(IReadOnlyList<GameResourceAmount> items)
        {
            if (items == null || items.Count == 0)
            {
                _items = Array.Empty<GameResourceAmount>();
                return;
            }

            // 呼び出し元のリストが後から変更されても報酬内容が変わらないよう複製する。
            GameResourceAmount[] copiedItems = new GameResourceAmount[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                copiedItems[i] = items[i];
            }

            _items = copiedItems;
        }

        /// <summary> 報酬なしを表すインスタンス。 </summary>
        public static StageReward Empty => default;

        /// <summary> 付与するリソースと数量の一覧。 </summary>
        public IReadOnlyList<GameResourceAmount> Items => _items ?? Array.Empty<GameResourceAmount>();

        /// <summary> 付与するリソースが無い場合はtrue。 </summary>
        public bool IsEmpty => Items.Count == 0;

        private readonly IReadOnlyList<GameResourceAmount> _items;
    }
}
