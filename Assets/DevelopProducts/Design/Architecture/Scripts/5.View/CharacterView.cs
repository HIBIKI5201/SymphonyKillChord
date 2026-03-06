using UnityEngine;
using DevelopProducts.Architecture.Adaptor;
using CharacterController = DevelopProducts.Architecture.Adaptor.CharacterController;
using DevelopProducts.Architecture.Domain;

namespace DevelopProducts.Architecture.View
{
    /// <summary>
    ///     キャラクターの描画と物理挙動を担当するビュークラス。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterView : MonoBehaviour, ICharacterView
    {
        /// <summary> 管理しているコントローラー。 </summary>
        public CharacterController Controller => _controller;

        /// <summary>
        ///     コントローラーをセットする。
        /// </summary>
        /// <param name="controller"> セットするコントローラー。 </param>
        public void SetController(CharacterController controller)
        {
            _controller = controller;
        }

        /// <summary>
        ///     キャラクターを移動させる。
        /// </summary>
        /// <param name="vel"> 速度ベクトル。 </param>
        void ICharacterView.Move(Vector2 vel)
        {
            _rb.AddForce(new Vector3(vel.x, 0, vel.y), ForceMode.Impulse);
        }

        /// <summary>
        ///     体力の表示を更新する。
        /// </summary>
        /// <param name="currentHealth"> 現在の体力。 </param>
        /// <param name="maxHealth"> 最大体力。 </param>
        void ICharacterView.UpdateHealth(float currentHealth, float maxHealth)
        {
            Debug.Log($"{name} Health: {currentHealth} / {maxHealth}");
        }

        [SerializeField, Tooltip("操作するRigidbody。")]
        private Rigidbody _rb;

        private CharacterController _controller;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out CharacterView view))
            {
                // 衝突した相手にダメージを与える
                _controller.AddDamage(view.Controller.Presenter);
            }
        }
    }
}
