using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public class CameraBoneFreeLookRotationApplication
    {
        public CameraBoneFreeLookRotationApplication(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        private readonly CameraSystemParameter _parameter;
    }
}
