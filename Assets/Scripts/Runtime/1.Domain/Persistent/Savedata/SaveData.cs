using KillChord.Runtime.Utility.OutGame.Savedata;
using System;

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

        /// <summary> プレイヤーのステージ進行状況のセーブデータを表すプロパティ。 </summary>
        public StageProgressData StageProgress { get; private set; } = new();

        /// <summary>
        ///     セーブデータを読み込んだ後に null チェックを行い、必要に応じて初期化する。
        /// </summary>
        protected override void OnAfterDeserialize()
        {
            SkillUnlock ??= new();
            SkillBuild ??= new();
            StageProgress ??= new();
        }
    }
}
