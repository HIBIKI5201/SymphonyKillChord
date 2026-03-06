using DevelopProducts.Architecture.View;
using UnityEngine;

namespace DevelopProducts.Architecture.Composition
{
    public class Initializer : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Awake()
        {
            foreach (CharacterInitializer chara in FindObjectsByType<CharacterInitializer>(FindObjectsSortMode.None))
            {
                chara.Initialize();
            }

            _controlCharacter.BindInputBuffer(_buffer);
        }

        [SerializeField, Tooltip("操作するキャラクターの初期化クラス。")]
        private CharacterInitializer _controlCharacter;
        [SerializeField, Tooltip("入力バッファ。")]
        private InputBuffer _buffer;
    }
}
