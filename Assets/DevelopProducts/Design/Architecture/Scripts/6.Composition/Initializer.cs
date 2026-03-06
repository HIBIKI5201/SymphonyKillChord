using DevelopProducts.Architecture.View;
using UnityEngine;

namespace DevelopProducts.Architecture.Composition
{
    public class Initializer : MonoBehaviour
    {
        public void Awake()
        {
            foreach (CharacterInitializer chara in FindObjectsByType<CharacterInitializer>(FindObjectsSortMode.None))
            {
                chara.Initialize();
            }

            _controlCharacter.BindInputBuffer(_buffer);
        }

        [SerializeField]
        private CharacterInitializer _controlCharacter;
        [SerializeField]
        private InputBuffer _buffer;
    }
}
