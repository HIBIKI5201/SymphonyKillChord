using System;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面の ViewModel インターフェース。
    /// </summary>
    public interface ISkillBuildViewModel
    {
        /// <summary> 保存要求イベント。 </summary>
        public event Action<ReadOnlyMemory<int>> OnSaveRequested;

        /// <summary>
        ///     DTO から表示状態を反映する。
        /// </summary>
        /// <param name="dto"> 表示更新 DTO。 </param>
        public void Apply(in SkillBuildViewDTO dto);
    }
}
