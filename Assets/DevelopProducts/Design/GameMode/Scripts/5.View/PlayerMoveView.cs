using UnityEngine;
namespace DevelopProducts.Design.GameMode.View
{
    /// <summary>
    ///     プレイヤーの移動を管理するクラス。
    ///     プレイヤーの入力に基づいて、プレイヤーの位置を更新する役割を持つ。
    /// </summary>
    public class PlayerMoveView : MonoBehaviour
    {
        private void Update()
        {
            float move = 0f;

            if (Input.GetKey(KeyCode.A))
            {
                move = -1f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                move = 1f;
            }

            transform.position += Vector3.right * (move * _moveSpeed * Time.deltaTime);
        }

        [SerializeField] private float _moveSpeed;
    }
}
