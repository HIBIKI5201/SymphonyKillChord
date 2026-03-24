using System.Collections.Generic;
namespace Research.SaveSystem
{
    /// <summary>
    ///     アウトゲーム情報のDTO。
    /// </summary>
    public class OutGameDataDto
    {
        /// <summary>ステージ解放状況</summary>
        public HashSet<int> StageUnlock { get; set; } = new();
        /// <summary>スキル解放状況</summary>
        public HashSet<int> SkillUnlock { get; set; } = new();
    }
}