using System.Collections.Generic;
namespace Research.SaveSystem
{
    /// <summary>
    ///     プレイヤー情報のDTO。
    /// </summary>
    public class PlayerDataDto
    {
        /// <summary>装備済みのスキル</summary>
        public List<int> EquippedSkills { get; set; } = new();
    }
}