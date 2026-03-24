using DevelopProducts.Persistent.Adaptor;

namespace DevelopProducts.Persistent.Application
{
    /// <summary>
    ///     入力マップを切り替えるユースケース。
    ///     シーンが変わった時など異なる入力マップを使用する場合などに使用される。
    /// </summary>
    public class SwichInputMapUseCase
    {
        public SwichInputMapUseCase(IInputMapController inputMapController)
        {
            _inputMapController = inputMapController;
        }

        public void ToInGame()
        {
            _inputMapController.EnableCommonWith(InputMapNames.InGame);
        }

        public void ToOutGame()
        {
            _inputMapController.EnableCommonWith(InputMapNames.OutGame);
        }

        public void DisableAll()
        {
            _inputMapController.DisableAll();
        }

        private readonly IInputMapController _inputMapController;
    }
}
