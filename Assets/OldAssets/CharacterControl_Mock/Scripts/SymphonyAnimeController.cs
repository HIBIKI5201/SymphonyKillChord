using UnityEngine;

namespace Mock.CharacterControl
{
    /// <summary>
    ///     Symphonyのアニメーションを管理するクラス
    /// </summary>
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
