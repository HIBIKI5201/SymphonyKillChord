using KillChord.Editor.Utility;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.SourceDataProvider
{
    /// <summary>
    ///     プランナー向けマスターデータ画面のページ定義を保持します。
    /// </summary>
    [FilePath(
        ProviderConst.PROJECT_SETTINGS_PATH + "PlannerMasterDataEditorSettings.asset",
        FilePathAttribute.Location.ProjectFolder)]
    internal sealed class PlannerMasterDataEditorSettings : ScriptableSingleton<PlannerMasterDataEditorSettings>
    {
        /// <summary> 登録済みのページ一覧です。 </summary>
        public IReadOnlyList<PageDefinition> Pages
        {
            get
            {
                EnsureDefaults();
                return _pages;
            }
        }

        /// <summary>
        ///     設定をProjectSettingsへ保存します。
        /// </summary>
        public void SaveSettings()
        {
            EnsureDefaults();
            Save(true);
        }

        [SerializeField, Tooltip("プランナー向け画面のページ定義一覧です。")]
        private List<PageDefinition> _pages = CreateDefaultPages();

        [SerializeField, HideInInspector]
        private bool _isInitialized;

        /// <summary>
        ///     初期ページ定義が未設定の場合に既定値を補完します。
        /// </summary>
        private void EnsureDefaults()
        {
            if (_isInitialized)
            {
                return;
            }

            if (_pages == null || _pages.Count == 0)
            {
                _pages = CreateDefaultPages();
            }

            _isInitialized = true;
        }

        /// <summary>
        ///     初期ページ定義を生成します。
        /// </summary>
        /// <returns> 初期ページ定義一覧です。 </returns>
        private static List<PageDefinition> CreateDefaultPages()
        {
            return new List<PageDefinition>
            {
                new(
                    "Stage Select",
                    new List<string>
                    {
                        "StageTreeAsset",
                        "EnemyWaveDefinitionAsset",
                        "EnemyMissionKeyAsset"
                    },
                    new List<string>
                    {
                        "StageAsset",
                        "StageBind",
                        "Wave"
                    }),
                new(
                    "Player",
                    new List<string>
                    {
                        "Player",
                        "PlayerConfig",
                        "OutGameSkillRepository"
                    },
                    new List<string>
                    {
                        "PlayerAttack",
                        "Skill"
                    }),
                new(
                    "UI",
                    new List<string>
                    {
                        "CameraConfig",
                        "SkillInputProgressViewConfigAsset",
                        "RhythmJudgmentDefinitionAsset"
                    },
                    new List<string>()),
                new(
                    "Enemy",
                    new List<string>
                    {
                        "Enemy",
                        "ExampleEnemyMoveData",
                        "ShellAttackData",
                        "BossCharacterData",
                        "BossAttackEntryRepo"
                    },
                    new List<string>
                    {
                        "EnemyAttack",
                        "BossAttackEntry"
                    }),
                new(
                    "Skill Tree",
                    new List<string>
                    {
                        "SkillNodeDataRepo",
                        "SkillNodeBindRepo",
                        "OutGameSkillRepository"
                    },
                    new List<string>
                    {
                        "SkillNode",
                        "SkillNodeBind",
                        "Skill"
                    }),
                new(
                    "Scenario",
                    new List<string>
                    {
                        "BackgroundCatalogAsset",
                        "AnimationCatalogAsset",
                        "PortraitCatalogAsset"
                    },
                    new List<string>
                    {
                        "ScenarioBackground",
                        "ScenarioAnimation",
                        "ScenarioPortrait"
                    })
            };
        }

        /// <summary>
        ///     1ページ分の定義を保持します。
        /// </summary>
        [Serializable]
        internal sealed class PageDefinition
        {
            /// <summary>
            ///     ページ定義を初期化します。
            /// </summary>
            /// <param name="displayName"> 表示名です。 </param>
            /// <param name="sourceAssetAddressableKeys"> 紐づくSourceAssetキー一覧です。 </param>
            /// <param name="collectionCategories"> 紐づくcollectionカテゴリ一覧です。 </param>
            public PageDefinition(
                string displayName,
                List<string> sourceAssetAddressableKeys,
                List<string> collectionCategories)
            {
                _displayName = displayName;
                _sourceAssetAddressableKeys = sourceAssetAddressableKeys ?? new List<string>();
                _collectionCategories = collectionCategories ?? new List<string>();
            }

            /// <summary> ページの表示名です。 </summary>
            public string DisplayName => _displayName;

            /// <summary> ページで扱うSourceAssetキー一覧です。 </summary>
            public IReadOnlyList<string> SourceAssetAddressableKeys => _sourceAssetAddressableKeys;

            /// <summary> ページで扱うcollectionカテゴリ一覧です。 </summary>
            public IReadOnlyList<string> CollectionCategories => _collectionCategories;

            [SerializeField, Tooltip("サイドバーへ表示するページ名です。")]
            private string _displayName;

            [SerializeField, Tooltip("このページで扱うSourceAssetのAddressableキー一覧です。")]
            private List<string> _sourceAssetAddressableKeys = new();

            [SerializeField, Tooltip("このページで扱うcollectionカテゴリ一覧です。")]
            private List<string> _collectionCategories = new();
        }
    }
}
