using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     銃の発砲に合わせて薬莢モデルを排出するView。
    /// </summary>
    public sealed class CasingEjectorView : MonoBehaviour
    {
        /// <summary>
        ///     薬莢を1つ排出します。
        /// </summary>
        public void Eject()
        {
            if (_casingPrefab == null)
            {
                Debug.LogError($"[{nameof(CasingEjectorView)}] 薬莢のPrefabが未設定です。", this);
                return;
            }

            Transform origin = _ejectPoint != null ? _ejectPoint : transform;
            GameObject casing = Instantiate(_casingPrefab, origin.position, origin.rotation);

            if (casing.TryGetComponent(out Rigidbody rigidbody))
            {
                Vector3 direction = origin.right + origin.up * Random.Range(0.3f, 0.7f);
                rigidbody.AddForce(direction.normalized * Random.Range(_minEjectSpeed, _maxEjectSpeed), ForceMode.VelocityChange);
                rigidbody.AddTorque(Random.insideUnitSphere * _torqueStrength, ForceMode.VelocityChange);
            }

            Destroy(casing, _lifetimeSeconds);
        }

        [SerializeField, Tooltip("排出する薬莢モデルのPrefab。Rigidbodyが必要です。")]
        private GameObject _casingPrefab;

        [SerializeField, Tooltip("薬莢の排出位置。未設定の場合はこのTransformを使用します。")]
        private Transform _ejectPoint;

        [SerializeField, Min(0f), Tooltip("薬莢を排出する最小速度。")]
        private float _minEjectSpeed = 1.5f;

        [SerializeField, Min(0f), Tooltip("薬莢を排出する最大速度。")]
        private float _maxEjectSpeed = 2.5f;

        [SerializeField, Min(0f), Tooltip("薬莢の回転に加える角速度の強さ。")]
        private float _torqueStrength = 10f;

        [SerializeField, Min(0f), Tooltip("薬莢を破棄するまでの秒数。")]
        private float _lifetimeSeconds = 4f;
    }
}
