using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;

namespace KillChord.Runtime.Composition.InGame.Music
{
    /// <summary>
    ///     装備BGMモジュールの公開物を保持するContainerです。
    /// </summary>
    public sealed class EquipmentBgmModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="equipmentBgmController"> 装備BGMControllerです。 </param>
        /// <param name="equipmentBgmService"> 装備BGMサービスです。 </param>
        public EquipmentBgmModuleContainer(
            EquipmentBgmController equipmentBgmController,
            EquipmentBgmService equipmentBgmService)
        {
            EquipmentBgmController = equipmentBgmController;
            EquipmentBgmService = equipmentBgmService;
        }

        /// <summary> 装備BGMControllerです。 </summary>
        public EquipmentBgmController EquipmentBgmController { get; }

        /// <summary> 装備BGMサービスです。 </summary>
        public EquipmentBgmService EquipmentBgmService { get; }
    }
}
