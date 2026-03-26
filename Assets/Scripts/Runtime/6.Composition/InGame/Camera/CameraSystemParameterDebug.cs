using KillChord.Runtime.Domain;
using UnityEngine;

#if UNITY_EDITOR

namespace KillChord.Runtime.Composition
{
    public sealed class CameraSystemParameterDebug : MonoBehaviour
    {
        [SerializeField] private CameraSystemParameter _parameter;

        public void SetCameraParameter(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }
    }
}

#endif