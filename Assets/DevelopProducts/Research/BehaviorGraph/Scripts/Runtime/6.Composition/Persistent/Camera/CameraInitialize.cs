using System;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Composition
{
    public class CameraInitialize : MonoBehaviour, ICameraTransform
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