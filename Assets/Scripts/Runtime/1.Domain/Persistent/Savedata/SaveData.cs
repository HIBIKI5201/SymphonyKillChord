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
        /// <summary> プレイヤーのスキル解放情報のセーブデータを表すプロパティ。 </summary>
        public SkillUnlockData SkillUnlock { get; private set; } = new();

        /// <summary> プレイヤーの装備スキル構成のセーブデータを表すプロパティ。 </summary>
        public SkillBuildData SkillBuild { get; private set; } = new();

        /// <summary> プレイヤーのチュートリアル進行状況のセーブデータを表すプロパティ。 </summary>
        public TutorialData Tutorial
        {
            get => _tutorial;
            set => _tutorial = value ?? throw new ArgumentNullException(nameof(value), "TutorialData は null にできません。");
        }

        [SerializeField, Tooltip("プレイヤーのチュートリアル進行状況のセーブデータ")]
        private TutorialData _tutorial;

        /// <summary>
        ///     セーブデータを読み込んだ後に null チェックを行い、必要に応じて初期化する。
        /// </summary>
        protected override void OnAfterDeserialize()
        {
            SkillUnlock ??= new();
            SkillBuild ??= new();
            _tutorial ??= new();
        }
    }
}
