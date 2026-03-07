using UnityEngine;
using CharacterController = DevelopProducts.Architecture.Adaptor.CharacterController;

namespace DevelopProducts.Architecture.View
{
    /// <summary>
    ///     キャラクターの描画と物理挙動を担当するビュークラス。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterView : MonoBehaviour
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

        public void BindViewModel(CharacterViewModel vm)
        {
            vm.OnMove += Move;
            vm.OnUpdateHealth += UpdateHealth;
            _vm = vm;
        }

        [SerializeField, Tooltip("操作するRigidbody。")]
        private Rigidbody _rb;

        private CharacterViewModel _vm;
        private CharacterController _controller;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnDestroy()
        {
            if (_vm != null)
            {
                _vm.OnMove -= Move;
                _vm.OnUpdateHealth -= UpdateHealth;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out CharacterView view))
            {
                // 衝突した相手にダメージを与える
                _controller.AddDamage(view.Controller.Presenter);
            }
        }

        /// <summary>
        ///     キャラクターを移動させる。
        /// </summary>
        /// <param name="vel"> 速度ベクトル。 </param>
        void Move(Vector2 vel)
        {
            _rb.AddForce(new Vector3(vel.x, 0, vel.y), ForceMode.Impulse);
        }

        /// <summary>
        ///     体力の表示を更新する。
        /// </summary>
        /// <param name="currentHealth"> 現在の体力。 </param>
        /// <param name="maxHealth"> 最大体力。 </param>
        void UpdateHealth(float currentHealth, float maxHealth)
        {
            Debug.Log($"{name} Health: {currentHealth} / {maxHealth}");
        }
    }
}
