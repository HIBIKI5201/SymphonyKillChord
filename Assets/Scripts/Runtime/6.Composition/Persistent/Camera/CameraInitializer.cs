using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class CameraInitializer : MonoBehaviour, ICameraTransform
    {
        private void Awake()
        {
            ServiceLocator.RegisterInstance<ICameraTransform>(this);
        }
    }

    public interface ICameraTransform
    {
        Transform transform { get; }
    }
}