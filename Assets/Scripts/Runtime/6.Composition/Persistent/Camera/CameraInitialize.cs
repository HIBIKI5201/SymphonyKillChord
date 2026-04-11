using System;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class CameraInitialize : MonoBehaviour, CameraTransform
    {
        private void Awake()
        {
            ServiceLocator.RegisterInstance<CameraTransform>(this);
        }
    }

    public interface CameraTransform
    {
        Transform transform { get; }
    }
}