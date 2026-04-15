using DevelopProducts.BehaviorGraph.Runtime.Application.InGame;

namespace DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame
{
    public sealed class TargetManagerController
    {
        public TargetManagerController(TargetManager targetManager)
        {
            _manager = targetManager;
        }

        public void Register(LockOnTargetGateway gateway)
        {
            _manager.Register(gateway);
        }
        public void Unregister(LockOnTargetGateway gateway)
        {
            _manager.Unregister(gateway);
        }

        private readonly TargetManager _manager;
    }
}
