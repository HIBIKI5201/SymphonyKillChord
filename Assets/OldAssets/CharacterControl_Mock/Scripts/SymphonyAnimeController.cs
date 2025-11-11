using UnityEngine;

namespace Mock.CharacterControl
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class SymphonyAnimeController : MonoBehaviour
    {
        

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
    }
}
