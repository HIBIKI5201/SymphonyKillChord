using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Camera
{
    public class CameraInitializer : MonoBehaviour, ICameraTransform
    {
        /// <summary>
        ///     ICameraTransform の Transform プロパティ。
        ///     MonoBehaviour の transform を返す。  
        /// </summary>
        public Transform Transform => transform;

        private void Awake()
        {
            ServiceLocator.RegisterInstance<ICameraTransform>(this);
        }
    }

    public interface ICameraTransform
    {
        Transform Transform { get; }
    }
}