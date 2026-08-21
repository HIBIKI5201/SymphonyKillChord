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

        // セーブデータの各種データを保持するメンバー変数
        [SerializeField, Tooltip("プレイヤーのスキル解放情報のセーブデータ")]
        private SkillUnlockData _skillUnlock;
        [SerializeField, Tooltip("プレイヤーの装備スキル構成のセーブデータ")]
        private SkillBuildData _skillBuild;
        [SerializeField, Tooltip("プレイヤーのステージ進行状況のセーブデータ")]
        private StageProgressData _stageProgress;
        [SerializeField, Tooltip("プレイヤーのチュートリアル進行状況のセーブデータ")]
        private TutorialData _tutorial;
    }
}
