using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵のHP表示を常にカメラの方に向けるためのViewクラス。
    /// </summary>
    public class EnemyHealthBillboardView : MonoBehaviour
    {
        /// <summary>
        ///     HP 表示を常にカメラの方へ向ける。
        /// </summary>
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

            transform.forward = _targetCamera.transform.forward;
        }

        [SerializeField, Tooltip("ターゲットとなるカメラ")] 
        private UnityEngine.Camera _targetCamera;
    }
}
