using UnityEngine;

namespace KillChord.Runtime.View
{
    public class EnemyHealthBillboardView : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;

                if (_targetCamera == null)
                {
                    return;
                }
            }

            transform.LookAt(_targetCamera.transform);
            transform.Rotate(0f, 0f, 0f);
        }

        [SerializeField] private Camera _targetCamera;
    }
}
