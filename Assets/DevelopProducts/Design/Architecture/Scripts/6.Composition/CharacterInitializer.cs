using DevelopProducts.Architecture.Adaptor;
using DevelopProducts.Architecture.Application;
using DevelopProducts.Architecture.Domain;
using DevelopProducts.Architecture.InfraStructure;
using DevelopProducts.Architecture.View;
using UnityEngine;
using CharacterController = DevelopProducts.Architecture.Adaptor.CharacterController;

namespace DevelopProducts.Architecture.Composition
{
    /// <summary>
    ///     キャラクター固有の依存関係を構築し、初期化を行うクラス。
    /// </summary>
    [RequireComponent(typeof(CharacterView))]
    public class CharacterInitializer : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize(AttackPipelineAsset attackPipeline)
        {
            CharacterView view = GetComponent<CharacterView>();

            CharacterViewModel vm = new();
            CharacterEntity entity = new(_characterStatus.Name, _characterStatus.Health, _characterStatus.Speed, _characterStatus.AttackPower);
            CharacterAttack characterAttack = new(entity, attackPipeline.Create());
            CharacterPresenter presenter = new(entity, vm);
            CharacterController controller = new(characterAttack, presenter);
            view.SetController(controller);
            view.BindViewModel(vm);

            _controller = controller;
        }

        /// <summary>
        ///     入力バッファをバインドする。
        /// </summary>
        /// <param name="buffer"> バインドする入力バッファ。 </param>
        public void BindInputBuffer(InputBuffer buffer)
        {
            buffer.SetController(_controller);
        }

        [SerializeField, Tooltip("キャラクターのステータスデータ。")]
        private CharacterStatusAsset _characterStatus;

        private CharacterController _controller;
    }
}
