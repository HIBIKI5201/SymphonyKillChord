using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;

namespace KillChord.Editor.SourceDataProvider.Core
{
    /// <summary>
    ///     収集したDataIDの重複・ハッシュ衝突・焼き込み忘れ・参照切れを、アセットを変更せずに検出します。
    /// </summary>
    public sealed class DataIDIntegrityChecker
    {
        /// <summary> 検出した不整合の件数です。 </summary>
        public int IssueCount => _issues.Count;

        /// <summary> 検査したDataIDの件数です。 </summary>
        public int CheckedCount => _checkedCount;

        /// <summary>
        ///     collectionへ登録されたID（生成側）を記録し、焼き込み済みハッシュを検査します。
        /// </summary>
        /// <param name="scope"> 重複を判定する範囲名です（ゲームデータ種別など）。 </param>
        /// <param name="collectionKey"> IDのCollectionKeyです。 </param>
        /// <param name="id"> 人間可読な文字列IDです。 </param>
        /// <param name="hashId"> 焼き込み済みの数値IDです。 </param>
        /// <param name="location"> エラー表示用の保持元です。 </param>
        public void AddRegisteredId(string scope, string collectionKey, string id, int hashId, string location)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            CheckHash(collectionKey, id, hashId, location);

            string scopedKey = scope + SCOPE_SEPARATOR + collectionKey;
            if (!_registrationsByScope.TryGetValue(scopedKey, out List<Registration> registrations))
            {
                registrations = new List<Registration>();
                _registrationsByScope.Add(scopedKey, registrations);
            }

            registrations.Add(new Registration(collectionKey, id, location));

            if (!_registeredIds.TryGetValue(collectionKey, out HashSet<string> ids))
            {
                ids = new HashSet<string>(StringComparer.Ordinal);
                _registeredIds.Add(collectionKey, ids);
            }

            ids.Add(id);
        }

        /// <summary>
        ///     登録済みIDを参照するDataIDを記録し、焼き込み済みハッシュを検査します。
        ///     参照先の存在は <see cref="Validate"/> で全登録を集めた後に検査します。
        /// </summary>
        /// <param name="collectionKey"> IDのCollectionKeyです。 </param>
        /// <param name="id"> 人間可読な文字列IDです。未設定の場合は任意項目として扱い検査しません。 </param>
        /// <param name="hashId"> 焼き込み済みの数値IDです。 </param>
        /// <param name="location"> エラー表示用の保持元です。 </param>
        public void AddReference(string collectionKey, string id, int hashId, string location)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            CheckHash(collectionKey, id, hashId, location);
            _references.Add(new Registration(collectionKey, id, location));
        }

        /// <summary>
        ///     焼き込み済みハッシュだけを検査します。登録・参照の判定ができないDataIDに使います。
        /// </summary>
        /// <param name="collectionKey"> IDのCollectionKeyです。 </param>
        /// <param name="id"> 人間可読な文字列IDです。 </param>
        /// <param name="hashId"> 焼き込み済みの数値IDです。 </param>
        /// <param name="location"> エラー表示用の保持元です。 </param>
        public void AddHashOnly(string collectionKey, string id, int hashId, string location)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            CheckHash(collectionKey, id, hashId, location);
        }

        /// <summary>
        ///     収集時に見つかった不整合を追加します。
        /// </summary>
        /// <param name="message"> 不整合の内容です。 </param>
        public void AddIssue(string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && _issueSet.Add(message))
            {
                _issues.Add(message);
            }
        }

        /// <summary>
        ///     記録した全DataIDを照合し、不整合の一覧を返します。
        /// </summary>
        /// <returns> 検出した不整合の一覧です。問題が無い場合は空です。 </returns>
        public IReadOnlyList<string> Validate()
        {
            foreach (KeyValuePair<string, List<Registration>> pair in _registrationsByScope)
            {
                ValidateRegistrations(pair.Key, pair.Value);
            }

            for (int i = 0; i < _references.Count; i++)
            {
                Registration reference = _references[i];
                if (!_registeredIds.TryGetValue(reference.CollectionKey, out HashSet<string> ids)
                    || !ids.Contains(reference.Id))
                {
                    AddIssue(
                        $"参照切れ: {reference.CollectionKey} の「{reference.Id}」はcollectionへ登録されていません。"
                        + $" ({reference.Location})");
                }
            }

            return _issues;
        }

        private const string SCOPE_SEPARATOR = "/";

        private readonly Dictionary<string, List<Registration>> _registrationsByScope = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _registeredIds = new(StringComparer.Ordinal);
        private readonly List<Registration> _references = new();
        private readonly List<string> _issues = new();
        private readonly HashSet<string> _issueSet = new(StringComparer.Ordinal);
        private int _checkedCount;

        /// <summary>
        ///     焼き込み済みハッシュがCollectionKeyとIDから計算した値と一致するか検査します。
        /// </summary>
        /// <param name="collectionKey"> IDのCollectionKeyです。 </param>
        /// <param name="id"> 人間可読な文字列IDです。 </param>
        /// <param name="hashId"> 焼き込み済みの数値IDです。 </param>
        /// <param name="location"> エラー表示用の保持元です。 </param>
        private void CheckHash(string collectionKey, string id, int hashId, string location)
        {
            _checkedCount++;
            int expectedHash = DataIDHasher.Compute(collectionKey, id);
            if (hashId != expectedHash)
            {
                AddIssue(
                    $"焼き込み忘れ: {collectionKey} の「{id}」のハッシュが {hashId} です（正: {expectedHash}）。"
                    + $" ({location})");
            }
        }

        /// <summary>
        ///     同じ範囲・CollectionKeyに登録されたIDの重複とハッシュ衝突を検査します。
        /// </summary>
        /// <param name="scopedKey"> 範囲名とCollectionKeyを連結したキーです。 </param>
        /// <param name="registrations"> 登録済みIDの一覧です。 </param>
        private void ValidateRegistrations(string scopedKey, List<Registration> registrations)
        {
            Dictionary<string, Registration> byId = new(StringComparer.Ordinal);
            Dictionary<int, Registration> byHash = new();

            for (int i = 0; i < registrations.Count; i++)
            {
                Registration registration = registrations[i];
                if (byId.TryGetValue(registration.Id, out Registration first))
                {
                    AddIssue(
                        $"重複: {scopedKey} に「{registration.Id}」が複数登録されています。"
                        + $" ({first.Location} / {registration.Location})");
                    continue;
                }

                byId.Add(registration.Id, registration);

                // 文字列IDが異なるのに数値IDが同じ場合は、実行時に区別できない。
                int hash = DataIDHasher.Compute(registration.CollectionKey, registration.Id);
                if (byHash.TryGetValue(hash, out Registration other))
                {
                    AddIssue(
                        $"ハッシュ衝突: {scopedKey} の「{other.Id}」と「{registration.Id}」が同じ数値ID {hash} になります。"
                        + $" ({other.Location} / {registration.Location})");
                    continue;
                }

                byHash.Add(hash, registration);
            }
        }

        /// <summary>
        ///     記録したDataID1件分の情報です。
        /// </summary>
        private readonly struct Registration
        {
            /// <summary>
            ///     記録を初期化します。
            /// </summary>
            /// <param name="collectionKey"> IDのCollectionKeyです。 </param>
            /// <param name="id"> 人間可読な文字列IDです。 </param>
            /// <param name="location"> エラー表示用の保持元です。 </param>
            public Registration(string collectionKey, string id, string location)
            {
                CollectionKey = collectionKey;
                Id = id;
                Location = location;
            }

            /// <summary> IDのCollectionKeyです。 </summary>
            public string CollectionKey { get; }

            /// <summary> 人間可読な文字列IDです。 </summary>
            public string Id { get; }

            /// <summary> エラー表示用の保持元です。 </summary>
            public string Location { get; }
        }
    }
}
