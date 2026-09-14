using KillChord.Runtime.Domain.OutGame.Resource;
using SymphonyFrameWork.System.SaveSystem;
using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     プレイヤーのセーブデータを表すクラス。
    ///     各種セーブデータクラスをメンバー変数として保持している。
    /// </summary>
    [Serializable]
    public sealed class SaveData : SaveDataContent
    {
        /// <summary>
        ///     SaveData クラスの新しいインスタンスを初期化する。
        /// </summary>
        public SaveData()
        {
            _skillUnlock = new();
            _skillBuild = new();
            _stageProgress = new();
            _tutorial = new();
            _audioSettings = new();
            _resourceInventory = new();
        }

        // 基底クラスにデシリアライズ後フックがないため、各プロパティの公開時に欠損データを補完する。

        /// <summary> プレイヤーのスキル解放情報のセーブデータを表すプロパティ。 </summary>
        public SkillUnlockData SkillUnlock => _skillUnlock ??= new();

        /// <summary> プレイヤーの装備スキル構成のセーブデータを表すプロパティ。 </summary>
        public SkillBuildData SkillBuild => _skillBuild ??= new();

        /// <summary> プレイヤーのステージ進行状況のセーブデータを表すプロパティ。 </summary>
        public StageProgressData StageProgress => _stageProgress ??= new();

        /// <summary> プレイヤーのチュートリアル進行状況のセーブデータを表すプロパティ。 </summary>
        public TutorialData Tutorial => _tutorial ??= new();

        /// <summary> プレイヤーの音量設定を表すプロパティ。 </summary>
        public AudioSettingsData AudioSettings => _audioSettings ??= new();

        /// <summary>
        ///     プレイヤーが所持するゲーム内リソースのセーブデータを表すプロパティ。
        ///     <para> 旧形式のポイントが残っている場合は、取得時に所持数へ移行する。 </para>
        /// </summary>
        public ResourceInventoryData ResourceInventory
        {
            get
            {
                _resourceInventory ??= new();
                MigrateLegacyPointsIfNeeded();
                return _resourceInventory;
            }
        }

        // セーブデータの各種データを保持するメンバー変数
        [SerializeField, Tooltip("プレイヤーのスキル解放情報のセーブデータ")]
        private SkillUnlockData _skillUnlock;
        [SerializeField, Tooltip("プレイヤーの装備スキル構成のセーブデータ")]
        private SkillBuildData _skillBuild;
        [SerializeField, Tooltip("プレイヤーのステージ進行状況のセーブデータ")]
        private StageProgressData _stageProgress;
        [SerializeField, Tooltip("プレイヤーのチュートリアル進行状況のセーブデータ")]
        private TutorialData _tutorial;
        [SerializeField, Tooltip("プレイヤーの音量設定")]
        private AudioSettingsData _audioSettings;
        [SerializeField, Tooltip("プレイヤーが所持するゲーム内リソースのセーブデータ")]
        private ResourceInventoryData _resourceInventory;
        [SerializeField, Tooltip("旧形式のポイントをリソース所持数へ移行済みの場合はtrue")]
        private bool _isLegacyPointMigrated;

        /// <summary>
        ///     SkillBuild と SkillUnlock に保存されていた旧形式のポイントを、リソースの所持数へ移行する。
        ///     <para>
        ///         移行済みフラグはコンストラクタで立てない。
        ///         旧セーブのデシリアライズ時にコンストラクタの値が残り、移行が飛ばされることを防ぐため。
        ///     </para>
        /// </summary>
        private void MigrateLegacyPointsIfNeeded()
        {
            if (_isLegacyPointMigrated)
            {
                return;
            }

            _resourceInventory.Add(
                GameResourceIds.SkillLevelupPoint,
                Math.Max(0, SkillBuild.SkillLevelupPoint));
            _resourceInventory.Add(
                GameResourceIds.ResearchPoint,
                // 研究ポイントのsetterは負値を拒否しないため、破損したセーブで移行が例外にならないよう0に丸める。
                Math.Max(0, SkillUnlock.ResearchPoint));
            SkillBuild.SetSkillLevelupPoint(0);
            SkillUnlock.SetResearchPoint(0);
            _isLegacyPointMigrated = true;
        }
    }
}
