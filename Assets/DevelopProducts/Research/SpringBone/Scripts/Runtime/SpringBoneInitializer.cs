using System;
using Unity.Animations.SpringBones;
using UnityEngine;

namespace DevelopProducts.SpringBone
{
    [DefaultExecutionOrder(1000)]
    public class SpringBoneInitializer : MonoBehaviour
    {
        [SerializeField] private SpringManager springManager;

        private void Start()
        {
            for (int i = 0; i < 10; i++)
            {
                springManager.UpdateDynamics();
            }
        }
        
        private void Update()
        {
            springManager.UpdateDynamics();
        }
    }
}