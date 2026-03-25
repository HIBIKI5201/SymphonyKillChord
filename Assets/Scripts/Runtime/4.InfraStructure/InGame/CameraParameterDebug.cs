using KillChord.Runtime.Domain;
using UnityEngine;

#if UNITY_EDITOR

namespace KillChord.Structure
{
    public sealed class CameraParameterDebug : MonoBehaviour
    {
        [SerializeField] private CameraParameter _cameraParameter;

        public void SetCameraParameter(CameraParameter parameter)
        {
            _cameraParameter = parameter;
        }
    }
}

#endif