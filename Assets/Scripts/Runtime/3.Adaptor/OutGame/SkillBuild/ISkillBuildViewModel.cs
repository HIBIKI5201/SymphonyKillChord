using System;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     改造画面の ViewModel インターフェース。
    /// </summary>
    public interface ISkillBuildViewModel
    {
        /// <summary> 保存要求イベント。true: 保存成功 / false: 保存失敗。 </summary>
        public event Func<ReadOnlyMemory<int>, Task<bool>> OnSaveRequested;

        /// <summary>
        ///     DTO から表示状態を反映する。
        /// </summary>
        /// <param name="dto"> 表示更新 DTO。 </param>
        public void Apply(in SkillBuildViewDTO dto);
    }
}
