using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Serialization;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     SourceDataProviderで参照するSourceAsset設定とcollection設定を保持します。
    /// </summary>
    [FilePath("ProjectSettings/SourceDataProviderSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class SourceDataProviderSettings : ScriptableSingleton<SourceDataProviderSettings>
    {
        /// <summary> 登録済みのSourceAsset設定一覧です。 </summary>
        public IReadOnlyList<SourceAssetMapping> SourceAssetMappings
        {
            get
            {
                EnsureInitialized();
                return _sourceAssetMappings;
            }
        }

        /// <summary> 登録済みのcollection設定一覧です。 </summary>
        public IReadOnlyList<SourceCollectionMapping> SourceCollectionMappings
        {
            get
            {
                EnsureInitialized();
                return _sourceCollectionMappings;
            }
        }

        /// <summary> 互換維持用の旧設定一覧です。 </summary>
        public IReadOnlyList<RepositoryMapping> RepositoryMappings
        {
            get
            {
                EnsureInitialized();
                return _repositoryMappings;
            }
        }

        /// <summary>
        ///     指定CollectionKeyがcollection設定に登録済みか判定します。
        /// </summary>
        /// <param name="collectionKey"> 確認するCollectionKeyです。 </param>
        /// <returns> 登録済みの場合はtrueです。 </returns>
        public bool ContainsCollectionKey(string collectionKey)
        {
            return TryGetCollectionMapping(collectionKey, out _);
        }

        /// <summary>
        ///     指定CollectionKeyのcollection設定を取得します。
        /// </summary>
        /// <param name="collectionKey"> 取得するCollectionKeyです。 </param>
        /// <param name="mapping"> 見つかった設定です。 </param>
        /// <returns> 対応が存在する場合はtrueです。 </returns>
        public bool TryGetCollectionMapping(string collectionKey, out SourceCollectionMapping mapping)
        {
            EnsureInitialized();
            for (int i = 0; i < _sourceCollectionMappings.Count; i++)
            {
                SourceCollectionMapping candidate = _sourceCollectionMappings[i];
                if (candidate != null
                    && string.Equals(candidate.CollectionKey, collectionKey, StringComparison.Ordinal))
                {
                    mapping = candidate;
                    return true;
                }
            }

            mapping = null;
            return false;
        }

        /// <summary>
        ///     指定AddressableキーのSourceAsset設定を取得します。
        /// </summary>
        /// <param name="addressableKey"> 取得するAddressableキーです。 </param>
        /// <param name="mapping"> 見つかった設定です。 </param>
        /// <returns> 対応が存在する場合はtrueです。 </returns>
        public bool TryGetSourceAssetMapping(string addressableKey, out SourceAssetMapping mapping)
        {
            EnsureInitialized();
            for (int i = 0; i < _sourceAssetMappings.Count; i++)
            {
                SourceAssetMapping candidate = _sourceAssetMappings[i];
                if (candidate != null
                    && string.Equals(candidate.AddressableKey, addressableKey, StringComparison.Ordinal))
                {
                    mapping = candidate;
                    return true;
                }
            }

            mapping = null;
            return false;
        }

        /// <summary>
        ///     指定Addressableキーに紐づくcollection設定一覧を取得します。
        /// </summary>
        /// <param name="addressableKey"> 取得対象のAddressableキーです。 </param>
        /// <returns> 対応するcollection設定一覧です。 </returns>
        public IReadOnlyList<SourceCollectionMapping> GetCollectionMappingsByAddressableKey(string addressableKey)
        {
            EnsureInitialized();
            List<SourceCollectionMapping> results = new();
            for (int i = 0; i < _sourceCollectionMappings.Count; i++)
            {
                SourceCollectionMapping candidate = _sourceCollectionMappings[i];
                if (candidate != null
                    && string.Equals(candidate.SourceAssetAddressableKey, addressableKey, StringComparison.Ordinal))
                {
                    results.Add(candidate);
                }
            }

            return results;
        }

        /// <summary>
        ///     現在の設定をProjectSettingsへ保存します。
        /// </summary>
        public void SaveSettings()
        {
            EnsureInitialized();
            Save(true);
        }

        /// <summary>
        ///     Addressablesへ登録済みのScriptableObject一覧を明示的に同期します。
        /// </summary>
        public void RefreshSourceAssetsFromAddressables()
        {
            if (!_isInitialized)
            {
                EnsureInitialized();
                return;
            }

            if (SynchronizeSourceAssetsFromAddressables())
            {
                Save(true);
            }
        }

        [SerializeField, Tooltip("Addressable ScriptableObject設定一覧です。")]
        private List<SourceAssetMapping> _sourceAssetMappings = new();

        [SerializeField, Tooltip("SourceAsset内のcollection設定一覧です。")]
        private List<SourceCollectionMapping> _sourceCollectionMappings = new();

        [SerializeField, HideInInspector]
        private List<RepositoryMapping> _repositoryMappings = CreateDefaultMappings();

        [SerializeField, HideInInspector]
        private bool _isInitialized;

        /// <summary>
        ///     旧設定の移行、必須設定の補完、Addressables同期を行います。
        /// </summary>
        private void EnsureInitialized()
        {
            _sourceAssetMappings ??= new List<SourceAssetMapping>();
            _sourceCollectionMappings ??= new List<SourceCollectionMapping>();
            _repositoryMappings ??= new List<RepositoryMapping>();

            bool changed = false;
            if (!_isInitialized)
            {
                if (_sourceAssetMappings.Count == 0)
                {
                    for (int i = 0; i < _repositoryMappings.Count; i++)
                    {
                        RepositoryMapping legacy = _repositoryMappings[i];
                        if (legacy == null || string.IsNullOrWhiteSpace(legacy.AddressableKey))
                        {
                            continue;
                        }

                        AppendSourceAssetMapping(legacy.AddressableKey);
                        changed = true;
                    }
                }

                if (_sourceCollectionMappings.Count == 0)
                {
                    for (int i = 0; i < _repositoryMappings.Count; i++)
                    {
                        RepositoryMapping legacy = _repositoryMappings[i];
                        if (legacy == null
                            || string.IsNullOrWhiteSpace(legacy.CollectionKey)
                            || string.IsNullOrWhiteSpace(legacy.AddressableKey))
                        {
                            continue;
                        }

                        _sourceCollectionMappings.Add(new SourceCollectionMapping(
                            legacy.CollectionKey,
                            legacy.AddressableKey,
                            legacy.ArrayPropertyPath,
                            GetDefaultCreationDirectory(legacy.CollectionKey)));
                        changed = true;
                    }
                }

                _isInitialized = true;
                changed = SynchronizeSourceAssetsFromAddressables() || changed;
                changed = true;
            }

            changed = EnsureDefaultMappings() || changed;
            if (changed)
            {
                // 新しい既定設定は、既存設定が初期化済みでも不足分だけ永続化する。
                Save(true);
            }
        }

        /// <summary>
        ///     バージョン更新で追加された全ての既定設定を不足時だけ補完します。
        /// </summary>
        /// <returns> 設定を追加した場合はtrueです。 </returns>
        private bool EnsureDefaultMappings()
        {
            _sourceAssetMappings ??= new List<SourceAssetMapping>();
            _sourceCollectionMappings ??= new List<SourceCollectionMapping>();
            _repositoryMappings ??= new List<RepositoryMapping>();

            bool changed = false;
            List<RepositoryMapping> defaults = CreateDefaultMappings();
            for (int i = 0; i < defaults.Count; i++)
            {
                RepositoryMapping mapping = defaults[i];
                if (!ContainsRepositoryMapping(mapping.CollectionKey))
                {
                    _repositoryMappings.Add(mapping);
                    changed = true;
                }

                if (!ContainsSourceAssetAddress(mapping.AddressableKey))
                {
                    _sourceAssetMappings.Add(new SourceAssetMapping(mapping.AddressableKey));
                    changed = true;
                }

                if (!ContainsCollectionMapping(mapping.CollectionKey))
                {
                    _sourceCollectionMappings.Add(new SourceCollectionMapping(
                        mapping.CollectionKey,
                        mapping.AddressableKey,
                        mapping.ArrayPropertyPath,
                        GetDefaultCreationDirectory(mapping.CollectionKey)));
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>
        ///     Addressablesへ登録済みのScriptableObjectをSourceAsset一覧へ補完します。
        /// </summary>
        /// <returns> SourceAsset設定を追加した場合はtrueです。 </returns>
        private bool SynchronizeSourceAssetsFromAddressables()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                return false;
            }

            bool changed = false;
            for (int i = 0; i < settings.groups.Count; i++)
            {
                AddressableAssetGroup group = settings.groups[i];
                if (group == null)
                {
                    continue;
                }

                foreach (AddressableAssetEntry entry in group.entries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.address))
                    {
                        continue;
                    }

                    Type mainAssetType = AssetDatabase.GetMainAssetTypeAtPath(entry.AssetPath);
                    if (mainAssetType == null || !typeof(ScriptableObject).IsAssignableFrom(mainAssetType))
                    {
                        continue;
                    }

                    changed = AppendSourceAssetMapping(entry.address) || changed;
                }
            }

            return changed;
        }

        /// <summary>
        ///     SourceAsset設定を未登録の場合のみ追加します。
        /// </summary>
        /// <param name="addressableKey"> 追加対象のAddressableキーです。 </param>
        /// <returns> SourceAsset設定を追加した場合はtrueです。 </returns>
        private bool AppendSourceAssetMapping(string addressableKey)
        {
            if (string.IsNullOrWhiteSpace(addressableKey) || ContainsSourceAssetAddress(addressableKey))
            {
                return false;
            }

            _sourceAssetMappings.Add(new SourceAssetMapping(addressableKey));
            return true;
        }

        /// <summary>
        ///     既に同じAddressableキーのSourceAssetが存在するか判定します。
        /// </summary>
        /// <param name="addressableKey"> 確認するAddressableキーです。 </param>
        /// <returns> 存在する場合はtrueです。 </returns>
        private bool ContainsSourceAssetAddress(string addressableKey)
        {
            for (int i = 0; i < _sourceAssetMappings.Count; i++)
            {
                SourceAssetMapping mapping = _sourceAssetMappings[i];
                if (mapping != null
                    && string.Equals(mapping.AddressableKey, addressableKey, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     CollectionKeyが現在のCollection設定に含まれているか判定します。
        /// </summary>
        /// <param name="collectionKey"> 確認するCollectionKeyです。 </param>
        /// <returns> 登録済みの場合はtrueです。 </returns>
        private bool ContainsCollectionMapping(string collectionKey)
        {
            for (int i = 0; i < _sourceCollectionMappings.Count; i++)
            {
                SourceCollectionMapping mapping = _sourceCollectionMappings[i];
                if (mapping != null
                    && string.Equals(mapping.CollectionKey, collectionKey, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     CollectionKeyが互換維持用の旧設定に含まれているか判定します。
        /// </summary>
        /// <param name="collectionKey"> 確認するCollectionKeyです。 </param>
        /// <returns> 登録済みの場合はtrueです。 </returns>
        private bool ContainsRepositoryMapping(string collectionKey)
        {
            for (int i = 0; i < _repositoryMappings.Count; i++)
            {
                RepositoryMapping mapping = _repositoryMappings[i];
                if (mapping != null
                    && string.Equals(mapping.CollectionKey, collectionKey, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     全データに共通して適用する既定マッピングを生成します。
        /// </summary>
        /// <returns> 初期設定です。 </returns>
        private static List<RepositoryMapping> CreateDefaultMappings()
        {
            return new List<RepositoryMapping>
            {
                new("StageAsset", "StageTreeAsset", "_stageAssets"),
                new("StageBind", "StageTreeAsset", "_bindAssets"),
                new("PlayerAttack", "Player", "_attackDifinitions"),
                new("Skill", "OutGameSkillRepository", "_skillDataAssets"),
                new("SkillNode", "SkillNodeDataRepo", "SkillNodes"),
                new("SkillNodeBind", "SkillNodeBindRepo", "SkillNodeBinds"),
                new("SkillNodePhaseBind", "SkillNodePhaseBindDataRepo", "PhaseBindData"),
                new("ScenarioAnimation", "AnimationCatalogAsset", "_entries"),
                new("ScenarioPortrait", "PortraitCatalogAsset", "_entries"),
                new("ScenarioBackground", "BackgroundCatalogAsset", "_entries"),
                new("Wave", "EnemyWaveDefinitionRepository", "_waveDefinitionAssets"),
                new("EnemyData", "EnemyDefinitionRepository", "_enemyDefinitionAssets"),
                new("BossAttackEntry", "BossAttackEntryRepo", "_attackEntries"),
                new("EnemyMissionKey", "EnemyMissionKeyRepository", "_missionKeyAssets"),
                new("Character", "CharacterDefinitionRepository", "_characterAssets"),
                new("TitleScreenRule", "TitleScreenRuleData", "_entries"),
                new("OutGameScreenRule", "OutGameScreenRuleData", "_entries"),
                new("Mission", "MissionDefinitionRepository", "_missionDefinitionAssets")
            };
        }

        /// <summary>
        ///     既定CollectionのScriptableObject生成先を取得します。
        /// </summary>
        /// <param name="collectionKey"> CollectionKeyです。 </param>
        /// <returns> 既定生成先です。未定義の場合は空文字です。 </returns>
        private static string GetDefaultCreationDirectory(string collectionKey)
        {
            return collectionKey switch
            {
                "StageAsset" => "Assets/Level/Data/Master/OutGame/StageSelect/Stages",
                "StageBind" => "Assets/Level/Data/Master/OutGame/StageSelect/StageBinds",
                "PlayerAttack" => "Assets/Level/Data/Master/InGame/Battle",
                "Skill" => "Assets/Level/Data/Master/Skill/Templates",
                "Wave" => "Assets/Level/Data/Master/InGame/Enemy",
                "EnemyData" => "Assets/Level/Data/Master/InGame/Enemy/Definitions",
                "BossAttackEntry" => "Assets/Level/Data/Develop/Boss",
                "EnemyMissionKey" => "Assets/Level/Data/Master/OutGame/StageSelect/Mission",
                "Character" => "Assets/Level/Data/Master/Character",
                "Mission" => "Assets/Level/Data/Master/OutGame/StageSelect/Mission",
                _ => string.Empty,
            };
        }

        /// <summary>
        ///     Addressable ScriptableObject設定を保持します。
        /// </summary>
        [Serializable]
        internal sealed class SourceAssetMapping
        {
            /// <summary>
            ///     設定情報を初期化します。
            /// </summary>
            /// <param name="addressableKey"> Addressableキーです。 </param>
            public SourceAssetMapping(string addressableKey)
            {
                _addressableKey = addressableKey;
            }

            /// <summary> Addressableキーです。 </summary>
            public string AddressableKey => _addressableKey;

            [SerializeField, Tooltip("SourceAssetのAddressableキーです。")]
            private string _addressableKey;
        }

        /// <summary>
        ///     SourceAsset内のcollection設定を保持します。
        /// </summary>
        [Serializable]
        internal sealed class SourceCollectionMapping
        {
            /// <summary>
            ///     設定情報を初期化します。
            /// </summary>
            /// <param name="collectionKey"> CollectionKeyです。 </param>
            /// <param name="sourceAssetAddressableKey"> SourceAssetのAddressableキーです。 </param>
            /// <param name="propertyPath"> collectionのプロパティパスです。 </param>
            /// <param name="assetCreationDirectory"> 新規アセットの生成先ディレクトリです。 </param>
            public SourceCollectionMapping(
                string collectionKey,
                string sourceAssetAddressableKey,
                string propertyPath,
                string assetCreationDirectory = "")
            {
                _collectionKey = collectionKey;
                _sourceAssetAddressableKey = sourceAssetAddressableKey;
                _propertyPath = propertyPath;
                _assetCreationDirectory = assetCreationDirectory;
            }

            /// <summary> CollectionKeyです。 </summary>
            public string CollectionKey => _collectionKey;

            /// <summary> SourceAssetのAddressableキーです。 </summary>
            public string SourceAssetAddressableKey => _sourceAssetAddressableKey;

            /// <summary> collectionのプロパティパスです。 </summary>
            public string PropertyPath => _propertyPath;

            /// <summary> 新規ScriptableObjectの生成先ディレクトリです。 </summary>
            public string AssetCreationDirectory => _assetCreationDirectory;

            [FormerlySerializedAs("_category")]
            [SerializeField, Tooltip("DataIDフィールドへ指定するCollectionKeyです。")]
            private string _collectionKey;

            [SerializeField, Tooltip("collectionを持つSourceAssetのAddressableキーです。")]
            private string _sourceAssetAddressableKey;

            [SerializeField, Tooltip("collectionとして扱う配列またはListのSerializedPropertyパスです。")]
            private string _propertyPath;

            [SerializeField, Tooltip("collectionへ追加するScriptableObjectの生成先Assetsディレクトリです。")]
            private string _assetCreationDirectory;
        }

        /// <summary>
        ///     旧互換のカテゴリとAddressableリポジトリ対応を保持します。
        /// </summary>
        [Serializable]
        internal sealed class RepositoryMapping
        {
            /// <summary>
            ///     設定情報を初期化します。
            /// </summary>
            /// <param name="category"> 旧カテゴリ名です。 </param>
            /// <param name="addressableKey"> Addressableキーです。 </param>
            /// <param name="arrayPropertyPath"> 個別データ配列のプロパティパスです。 </param>
            public RepositoryMapping(string category, string addressableKey, string arrayPropertyPath)
            {
                _category = category;
                _addressableKey = addressableKey;
                _arrayPropertyPath = arrayPropertyPath;
            }

            /// <summary> カテゴリ名です。 </summary>
            public string CollectionKey => _category;

            /// <summary> Addressableキーです。 </summary>
            public string AddressableKey => _addressableKey;

            /// <summary> 個別データ配列のプロパティパスです。 </summary>
            public string ArrayPropertyPath => _arrayPropertyPath;

            [SerializeField]
            private string _category;

            [SerializeField]
            private string _addressableKey;

            [SerializeField]
            private string _arrayPropertyPath;
        }
    }
}
