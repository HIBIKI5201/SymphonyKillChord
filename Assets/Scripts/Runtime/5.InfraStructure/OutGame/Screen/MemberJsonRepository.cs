using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace KillChord.Runtime.InfraStructure.OutGame.Screen
{
    /// <summary>
    ///     StreamingAssets のクレジット JSON から制作メンバー情報を取得するリポジトリ。
    /// </summary>
    /// <remarks>
    ///     <c>Assets/StreamingAssets/Credits/credits.json</c> はホームページ（<c>HomePage/src/pages/credit.astro</c>）と共有する名簿の正本です。
    ///     構成は カテゴリ → 役職 → 名前 の入れ子で、サイトはカテゴリ・役職・名前・リードを表示します。
    ///     ゲームは名前ごとの読み仮名・役職表記（<c>roleDetail</c>）・所属を表示し、並び順は JSON の記載順です。
    /// </remarks>
    public sealed class MemberJsonRepository : IMemberRepository
    {
        /// <summary>
        ///     クレジット JSON を解析してリポジトリを初期化します。
        /// </summary>
        /// <param name="jsonText"> クレジット JSON の全文です。 </param>
        /// <exception cref="ArgumentNullException"> JSON の全文が null の場合に発生します。 </exception>
        /// <exception cref="ArgumentException"> JSON の書式が不正な場合に発生します。 </exception>
        public MemberJsonRepository(string jsonText)
        {
            if (jsonText == null)
            {
                throw new ArgumentNullException(nameof(jsonText));
            }

            _members = Parse(jsonText);
        }

        /// <summary>
        ///     StreamingAssets のクレジット JSON を読み込んでリポジトリを生成します。
        /// </summary>
        /// <param name="ct"> キャンセルトークンです。 </param>
        /// <returns> 生成したリポジトリです。 </returns>
        /// <exception cref="IOException"> ファイルを読み込めなかった場合に発生します。 </exception>
        public static async ValueTask<MemberJsonRepository> LoadAsync(CancellationToken ct)
        {
            string jsonText = await ReadCreditJsonAsync(ct);
            return new MemberJsonRepository(jsonText);
        }

        /// <summary>
        ///     全ての制作メンバー情報を取得します。
        /// </summary>
        /// <returns> 制作メンバー情報の一覧です。 </returns>
        public IReadOnlyList<MemberData> GetAllMembers()
        {
            return _members;
        }

        /// <summary> StreamingAssets 内のクレジット JSON のフォルダ名です。 </summary>
        private const string CREDIT_DIRECTORY_NAME = "Credits";

        /// <summary> クレジット JSON のファイル名です。 </summary>
        private const string CREDIT_FILE_NAME = "credits.json";

        /// <summary> StreamingAssets のパスが URL 形式かどうかを判別する区切り文字です。 </summary>
        private const string URL_SCHEME_SEPARATOR = "://";

        private readonly IReadOnlyList<MemberData> _members;

        /// <summary>
        ///     StreamingAssets からクレジット JSON の全文を読み込みます。
        /// </summary>
        /// <param name="ct"> キャンセルトークンです。 </param>
        /// <returns> クレジット JSON の全文です。 </returns>
        private static async ValueTask<string> ReadCreditJsonAsync(CancellationToken ct)
        {
            string root = UnityEngine.Application.streamingAssetsPath;

            // Android では StreamingAssets が APK 内にあり、File API では読めないため UnityWebRequest で読む。
            if (!root.Contains(URL_SCHEME_SEPARATOR, StringComparison.Ordinal))
            {
                string path = Path.Combine(root, CREDIT_DIRECTORY_NAME, CREDIT_FILE_NAME);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"クレジット JSON が見つかりません。path={path}", path);
                }

                return await File.ReadAllTextAsync(path, Encoding.UTF8, ct);
            }

            string url = $"{root.TrimEnd('/')}/{CREDIT_DIRECTORY_NAME}/{CREDIT_FILE_NAME}";
            using UnityWebRequest request = UnityWebRequest.Get(url);
            using CancellationTokenRegistration registration = ct.Register(s => ((UnityWebRequest)s).Abort(), request);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            ct.ThrowIfCancellationRequested();
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new IOException($"クレジット JSON を読み込めませんでした。url={url}, error={request.error}");
            }

            return request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
        }

        /// <summary>
        ///     クレジット JSON を制作メンバー情報の一覧へ解析します。
        /// </summary>
        /// <param name="jsonText"> クレジット JSON の全文です。 </param>
        /// <returns> 解析した制作メンバー情報の一覧です。 </returns>
        private static IReadOnlyList<MemberData> Parse(string jsonText)
        {
            var members = new List<MemberData>();
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                return members;
            }

            CreditDocument document = JsonUtility.FromJson<CreditDocument>(jsonText);
            if (document?.categories == null)
            {
                return members;
            }

            // カテゴリ → 役職 → 名前 の記載順をそのまま表示順にする。
            foreach (CreditCategory category in document.categories)
            {
                if (category?.roles == null)
                {
                    continue;
                }

                foreach (CreditRole role in category.roles)
                {
                    if (role?.names == null)
                    {
                        continue;
                    }

                    foreach (CreditName entry in role.names)
                    {
                        if (TryCreateMemberData(role, entry, out MemberData memberData))
                        {
                            members.Add(memberData);
                        }
                    }
                }
            }

            return members;
        }

        /// <summary>
        ///     名前1件分の記載から制作メンバー情報の生成を試みます。
        /// </summary>
        /// <param name="role"> 名前が属する役職です。 </param>
        /// <param name="entry"> 名前1件分の記載です。 </param>
        /// <param name="memberData"> 生成した制作メンバー情報です。 </param>
        /// <returns> 生成に成功した場合はtrue。 </returns>
        private static bool TryCreateMemberData(CreditRole role, CreditName entry, out MemberData memberData)
        {
            memberData = default;
            if (entry == null)
            {
                return false;
            }

            string name = FormatName(entry);

            // 役職表記が無い場合は、サイトと同じまとめた役職名を使う。
            string className = string.IsNullOrWhiteSpace(entry.roleDetail) ? role.role : entry.roleDetail;
            string affiliationName = entry.affiliation;

            // 1 件の不備で一覧全体が表示できなくなることを避けるため、不正な記載は警告して読み飛ばす。
            if (string.IsNullOrWhiteSpace(name)
                || string.IsNullOrWhiteSpace(className)
                || string.IsNullOrWhiteSpace(affiliationName))
            {
                Debug.LogWarning(
                    $"[{nameof(MemberJsonRepository)}] 空の項目があるため読み飛ばします。名前={name} 役職={className} 所属={affiliationName}");
                return false;
            }

            memberData = new MemberData(
                new MemberName(name),
                new MemberClassName(className.Trim()),
                new MemberAffiliationName(affiliationName.Trim()));
            return true;
        }

        /// <summary>
        ///     読み仮名があれば「名前（読み仮名）」の形に整えた表示名を返します。
        /// </summary>
        /// <param name="entry"> 名前1件分の記載です。 </param>
        /// <returns> 表示名です。名前が空の場合は空文字列です。 </returns>
        private static string FormatName(CreditName entry)
        {
            string name = entry.name?.Trim() ?? string.Empty;
            if (name.Length == 0 || string.IsNullOrWhiteSpace(entry.reading))
            {
                return name;
            }

            return $"{name}（{entry.reading.Trim()}）";
        }

        // 以下は JsonUtility で読むための型で、フィールド名を JSON のキー名と一致させる必要がある。

        /// <summary>
        ///     クレジット JSON のルート。
        /// </summary>
        [Serializable]
        private sealed class CreditDocument
        {
            /// <summary> カテゴリの一覧です。 </summary>
            public CreditCategory[] categories;
        }

        /// <summary>
        ///     クレジットのカテゴリ（プログラム、デザインなど）。
        /// </summary>
        [Serializable]
        private sealed class CreditCategory
        {
            /// <summary> カテゴリ名です。 </summary>
            public string category;

            /// <summary> カテゴリ内の役職の一覧です。 </summary>
            public CreditRole[] roles;
        }

        /// <summary>
        ///     サイトで見出しにするまとめた役職。
        /// </summary>
        [Serializable]
        private sealed class CreditRole
        {
            /// <summary> まとめた役職名です。 </summary>
            public string role;

            /// <summary> 役職に属する名前の一覧です。 </summary>
            public CreditName[] names;
        }

        /// <summary>
        ///     名前1件分の記載。
        /// </summary>
        [Serializable]
        private sealed class CreditName
        {
            /// <summary> 名前です。 </summary>
            public string name;

            /// <summary> 読み仮名です。 </summary>
            public string reading;

            /// <summary> ゲーム内で表示する役職表記です。 </summary>
            public string roleDetail;

            /// <summary> 所属です。 </summary>
            public string affiliation;
        }
    }
}
