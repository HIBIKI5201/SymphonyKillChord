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
        public SkillUnlockData SkillUnlock = new();

        /// <summary> プレイヤーの装備スキル構成のセーブデータを表すプロパティ。 </summary>
        public SkillBuildData SkillBuild = new();
    }
}
