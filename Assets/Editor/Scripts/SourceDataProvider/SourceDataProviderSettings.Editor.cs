using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SourceDataProviderで参照するカテゴリとリポジトリの対応を保持します。
    /// </summary>
    [FilePath("ProjectSettings/SourceDataProviderSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class SourceDataProviderSettings : ScriptableSingleton<SourceDataProviderSettings>
    {
        /// <summary> 登録済みのリポジトリ対応一覧です。 </summary>
        public IReadOnlyList<RepositoryMapping> RepositoryMappings => _repositoryMappings;

        /// <summary>
        ///     指定カテゴリが登録済みか判定します。
        /// </summary>
        /// <param name="category"> 確認するカテゴリ名です。 </param>
        /// <returns> 登録済みの場合はtrueです。 </returns>
        public bool ContainsCategory(string category)
        {
            return TryGetMapping(category, out _);
        }

        /// <summary>
        ///     指定カテゴリのリポジトリ対応を取得します。
        /// </summary>
        /// <param name="category"> 取得するカテゴリ名です。 </param>
        /// <param name="mapping"> 見つかったリポジトリ対応です。 </param>
        /// <returns> 対応が存在する場合はtrueです。 </returns>
        public bool TryGetMapping(string category, out RepositoryMapping mapping)
        {
            for (int i = 0; i < _repositoryMappings.Count; i++)
            {
                RepositoryMapping candidate = _repositoryMappings[i];
                if (candidate != null
                    && string.Equals(candidate.Category, category, StringComparison.Ordinal))
                {
                    mapping = candidate;
                    return true;
                }
            }

            mapping = null;
            return false;
        }

        /// <summary>
        ///     現在の設定をProjectSettingsへ保存します。
        /// </summary>
        public void SaveSettings()
        {
            Save(true);
        }

        [SerializeField, Tooltip("カテゴリとAddressableリポジトリの対応一覧です。")]
        private List<RepositoryMapping> _repositoryMappings = CreateDefaultMappings();

        /// <summary>
        ///     初期カテゴリ設定を生成します。
        /// </summary>
        /// <returns> 初期カテゴリ設定です。 </returns>
        private static List<RepositoryMapping> CreateDefaultMappings()
        {
            return new List<RepositoryMapping>
            {
                new("Stage", "StageTreeAsset", "_nodeAssets"),
                new("EnemyMissionKey", "EnemyMissionKeyAsset", string.Empty),
                new("Skill", "OutGameSkillRepository", "_skillDataAssets"),
                new("SkillNode", "SkillNodeDataRepo", "SkillNodes"),
                new("ScenarioAnimation", "AnimationCatalogAsset", "_entries"),
                new("ScenarioPortrait", "PortraitCatalogAsset", "_entries"),
                new("ScenarioBackground", "BackgroundCatalogAsset", "_entries"),
                new("StageEffect", "EnemyWaveDefinitionAsset", "_waves")
            };
        }

        /// <summary>
        ///     カテゴリとAddressableリポジトリの対応を保持します。
        /// </summary>
        [Serializable]
        internal sealed class RepositoryMapping
        {
            /// <summary>
            ///     対応情報を初期化します。
            /// </summary>
            /// <param name="category"> カテゴリ名です。 </param>
            /// <param name="addressableKey"> リポジトリのAddressableキーです。 </param>
            /// <param name="arrayPropertyPath"> 個別データ配列のプロパティパスです。 </param>
            public RepositoryMapping(string category, string addressableKey, string arrayPropertyPath)
            {
                _category = category;
                _addressableKey = addressableKey;
                _arrayPropertyPath = arrayPropertyPath;
            }

            /// <summary> カテゴリ名です。 </summary>
            public string Category => _category;

            /// <summary> リポジトリのAddressableキーです。 </summary>
            public string AddressableKey => _addressableKey;

            /// <summary> 個別データ配列のプロパティパスです。 </summary>
            public string ArrayPropertyPath => _arrayPropertyPath;

            [SerializeField, Tooltip("DataIDフィールドへ指定するカテゴリ名です。")]
            private string _category;

            [SerializeField, Tooltip("リポジトリアセットのAddressableキーです。")]
            private string _addressableKey;

            [SerializeField, Tooltip("個別データ配列のSerializedPropertyパスです。")]
            private string _arrayPropertyPath;
        }
    }
}
