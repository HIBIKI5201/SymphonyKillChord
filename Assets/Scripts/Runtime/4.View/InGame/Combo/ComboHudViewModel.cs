using KillChord.Runtime.Adaptor;
using R3;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///    コンボHUDの表示内容を更新するためのViewModel。
    /// </summary>
    public class ComboHudViewModel : IComboHudViewModel
    {
        /// <summary> コンボ数を保持するプロパティ。 </summary>
        public ReactiveProperty<int> ComboCount { get; } = new(0);
        /// <summary>
        ///   DTOを受け取って、コンボHUDの表示内容を更新します。
        /// </summary>
        /// <param name="dto"> 反映するコンボ表示用のDTOです。 </param>
        public void Apply(in ComboDTO dto)
        {
            ComboCount.Value = dto.ComboCount;
        }
    }
}
