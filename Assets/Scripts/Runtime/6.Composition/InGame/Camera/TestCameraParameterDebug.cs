using KillChord.Runtime.Domain;
using UnityEngine;

#if UNITY_EDITOR

namespace KillChord.Runtime.Composition
{
    public sealed class TestCameraParameterDebug : MonoBehaviour
    {
        [SerializeField] private CameraMovementParameter _parameter;

        public void SetCameraParameter(CameraMovementParameter parameter)
        {
            _parameter = parameter;
        }
    }
}

#endif