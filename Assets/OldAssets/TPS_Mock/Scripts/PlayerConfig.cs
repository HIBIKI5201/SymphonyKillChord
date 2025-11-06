using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     プレイヤーの設定クラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(PlayerConfig), menuName = "Mock/TPS/" + nameof(PlayerConfig))]
    public class PlayerConfig :ScriptableObject
    {
        public bool IsCameraFlipX => _isCameraFlipX;

        [SerializeField, Tooltip("カメラのX回転を反転")]
        private bool _isCameraFlipX;
    }
}