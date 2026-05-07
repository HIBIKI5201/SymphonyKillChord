using KillChord.Runtime.Application.InGame.Camera.Target;

namespace KillChord.Runtime.Adaptor.InGame.Camera.Target
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
