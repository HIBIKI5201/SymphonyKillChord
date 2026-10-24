using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Screen
{
    /// <summary>
    ///     StreamingAssets の JSON からサードパーティ素材の表記を取得するリポジトリ。
    /// </summary>
    /// <remarks>
    ///     <c>Assets/StreamingAssets/Credits/third-party-notices.json</c> は、リポジトリ直下の
    ///     <c>third-party-notices.md</c> の内容をゲーム内のクレジット「使用アセット」タブ用にまとめたものです。
    ///     素材を足したときは両方を更新します。並び順は JSON の記載順です。
    /// </remarks>
    public sealed class ThirdPartyNoticeJsonRepository : IThirdPartyNoticeRepository
    {
        /// <summary>
        ///     サードパーティ素材の表記 JSON を解析してリポジトリを初期化します。
        /// </summary>
        /// <param name="jsonText"> サードパーティ素材の表記 JSON の全文です。 </param>
        /// <exception cref="ArgumentNullException"> JSON の全文が null の場合に発生します。 </exception>
        /// <exception cref="ArgumentException"> JSON の書式が不正な場合に発生します。 </exception>
        public ThirdPartyNoticeJsonRepository(string jsonText)
        {
            if (jsonText == null)
            {
                throw new ArgumentNullException(nameof(jsonText));
            }

            _notices = Parse(jsonText);
        }

        /// <summary>
        ///     StreamingAssets のサードパーティ素材の表記 JSON を読み込んでリポジトリを生成します。
        /// </summary>
        /// <param name="ct"> キャンセルトークンです。 </param>
        /// <returns> 生成したリポジトリです。 </returns>
        /// <exception cref="IOException"> ファイルを読み込めなかった場合に発生します。 </exception>
        public static async ValueTask<ThirdPartyNoticeJsonRepository> LoadAsync(CancellationToken ct)
        {
            string jsonText = await StreamingAssetsTextReader.ReadAllTextAsync(NOTICE_DIRECTORY_NAME, NOTICE_FILE_NAME, ct);
            return new ThirdPartyNoticeJsonRepository(jsonText);
        }

        /// <summary>
        ///     全てのサードパーティ素材の表記を取得します。
        /// </summary>
        /// <returns> サードパーティ素材の表記の一覧です。 </returns>
        public IReadOnlyList<ThirdPartyNoticeData> GetAllNotices()
        {
            return _notices;
        }

        /// <summary> StreamingAssets 内のサードパーティ素材の表記 JSON のフォルダ名です。 </summary>
        private const string NOTICE_DIRECTORY_NAME = "Credits";

        /// <summary> サードパーティ素材の表記 JSON のファイル名です。 </summary>
        private const string NOTICE_FILE_NAME = "third-party-notices.json";

        private readonly IReadOnlyList<ThirdPartyNoticeData> _notices;

        /// <summary>
        ///     サードパーティ素材の表記 JSON を表記の一覧へ解析します。
        /// </summary>
        /// <param name="jsonText"> サードパーティ素材の表記 JSON の全文です。 </param>
        /// <returns> 解析したサードパーティ素材の表記の一覧です。 </returns>
        private static IReadOnlyList<ThirdPartyNoticeData> Parse(string jsonText)
        {
            var notices = new List<ThirdPartyNoticeData>();
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                return notices;
            }

            NoticeDocument document = JsonUtility.FromJson<NoticeDocument>(jsonText);
            if (document?.notices == null)
            {
                return notices;
            }

            foreach (NoticeEntry entry in document.notices)
            {
                // 1 件の不備で一覧全体が表示できなくなることを避けるため、名前の無い記載は警告して読み飛ばす。
                if (entry == null || string.IsNullOrWhiteSpace(entry.name))
                {
                    Debug.LogWarning($"[{nameof(ThirdPartyNoticeJsonRepository)}] 素材名が空のため読み飛ばします。");
                    continue;
                }

                notices.Add(new ThirdPartyNoticeData(
                    entry.name.Trim(),
                    entry.holder?.Trim(),
                    entry.license?.Trim(),
                    entry.text?.Trim()));
            }

            return notices;
        }

        // 以下は JsonUtility で読むための型で、フィールド名を JSON のキー名と一致させる必要がある。

        /// <summary>
        ///     サードパーティ素材の表記 JSON のルート。
        /// </summary>
        [Serializable]
        private sealed class NoticeDocument
        {
            /// <summary> 素材ごとの表記の一覧です。 </summary>
            public NoticeEntry[] notices;
        }

        /// <summary>
        ///     素材1件分の表記。
        /// </summary>
        [Serializable]
        private sealed class NoticeEntry
        {
            /// <summary> 素材名です。 </summary>
            public string name;

            /// <summary> 権利者・著作表記です。 </summary>
            public string holder;

            /// <summary> ライセンス名です。 </summary>
            public string license;

            /// <summary> 著作表記や許諾表示の本文です。 </summary>
            public string text;
        }
    }
}
