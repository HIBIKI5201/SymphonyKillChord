using KillChord.Runtime.Utility.OutGame.Savedata;
using System;
using UnityEngine;

namespace KillChord.Runtime.Domain.Persistent.Savedata
{
    /// <summary>
    ///     プレイヤーのセーブデータを表すクラス。
    ///     各種セーブデータクラスをメンバー変数として保持している。
    /// </summary>
    [Serializable]
    public sealed class SaveData : SaveBase
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

        /// <summary> プレイヤーのスキル解放情報のセーブデータを表すプロパティ。 </summary>
        public SkillUnlockData SkillUnlock => _skillUnlock;

        /// <summary> プレイヤーの装備スキル構成のセーブデータを表すプロパティ。 </summary>
        public SkillBuildData SkillBuild => _skillBuild;

        /// <summary> プレイヤーのステージ進行状況のセーブデータを表すプロパティ。 </summary>
        public StageProgressData StageProgress => _stageProgress;

        /// <summary> プレイヤーのチュートリアル進行状況のセーブデータを表すプロパティ。 </summary>
        public TutorialData Tutorial => _tutorial;

        // セーブデータの各種データを保持するメンバー変数
        [SerializeField, Tooltip("プレイヤーのスキル解放情報のセーブデータ")]
        private SkillUnlockData _skillUnlock;
        [SerializeField, Tooltip("プレイヤーの装備スキル構成のセーブデータ")]
        private SkillBuildData _skillBuild;
        [SerializeField, Tooltip("プレイヤーのステージ進行状況のセーブデータ")]
        private StageProgressData _stageProgress;
        [SerializeField, Tooltip("プレイヤーのチュートリアル進行状況のセーブデータ")]
        private TutorialData _tutorial;


        /// <summary>
        ///     セーブデータを読み込んだ後に null チェックを行い、必要に応じて初期化する。
        /// </summary>
        protected override void OnAfterDeserialize()
        {
            _skillUnlock ??= new();
            _skillBuild ??= new();
            _stageProgress ??= new();
            _tutorial ??= new();
        }
    }
}
