using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵のHP表示を常にカメラの方に向けるためのViewクラス。
    /// </summary>
    public class EnemyHealthBillboardView : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (_targetCamera == null)
            {
                _targetCamera = UnityEngine.Camera.main;

                if (_targetCamera == null)
                {
                    return;
                }
            }

            transform.LookAt(_targetCamera.transform);
        }

        [SerializeField, Tooltip("ターゲットとなるカメラ")] 
        private UnityEngine.Camera _targetCamera;
    }
}
