using KillChord.Runtime.Utility.OutGame.Savedata;
using System;
using System.Collections.Generic;
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
        private const int SKILL_00_ID = -876453005;
        private const int SKILL_13_ID = 1271923592;

        private static readonly int[] INITIAL_UNLOCKED_SKILL_IDS = { SKILL_00_ID, SKILL_13_ID };
        private static readonly int[] INITIAL_EQUIPPED_SKILL_IDS = { SKILL_00_ID, SKILL_13_ID };

        /// <summary>
        ///     SaveData クラスの新しいインスタンスを初期化する。
        /// </summary>
        public SaveData()
        {
            _skillUnlock = new();
            _skillBuild = new();
            _stageProgress = new();
            _tutorial = new();
            EnsureInitialSkillState();
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
            EnsureInitialSkillState();
        }

        /// <summary>
        ///     初期スキル状態が未設定の場合に、既定の解放・装備状態を補完する。
        /// </summary>
        private void EnsureInitialSkillState()
        {
            if (_skillUnlock.UnlockedSkillIds.Length == 0)
            {
                _skillUnlock.SetUnlockedSkillIds((int[])INITIAL_UNLOCKED_SKILL_IDS.Clone());
            }

            if (_skillBuild.EquipmentSkillIDs.Count == 0)
            {
                _skillBuild.SetEquipmentSkillIDs(new List<int>(INITIAL_EQUIPPED_SKILL_IDS));
            }
        }
    }
}
