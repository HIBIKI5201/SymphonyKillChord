using System.Collections.Generic;
namespace Research.SaveSystem
{
    /// <summary>
    ///     プレイヤー情報
    /// </summary>
    public class PlayerDataDto
    {
        public PlayerDataDto()
        {
        }
        public List<int> EquippedSkills { get; set; }
    }
}