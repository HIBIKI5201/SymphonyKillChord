using KillChord.Runtime.Utility.Rendering;
using LitMotion;
using LitMotion.Extensions;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    public sealed class EnemyDestroyEffectView : MonoBehaviour
    {
        /// <summary>
        ///     死亡演出として、対象RendererのMaterialプロパティをLMotionで変化させる。
        /// </summary>
        public async ValueTask PlayDeathMaterialEffectAsync()
        {
            if (_deathEffectRenderers == null || _deathEffectRenderers.Length == 0)
            {
                return;
            }
            if (_deathSwampGameObject == null)
            {
                return;
            }

            _deathSwampGameObject.SetActive(true);
            // DestroyFadeシェーダーは_DeathEffectAmountのデフォルトが1(通常表示)で、0に近づくほど消滅する。
            _handle.TryComplete();
            _handle = LSequence.Create()
                    .Join(LMotion.Create(1f, 0f, _flashDuration)
                        .WithEase(Ease.Linear)
                        .BindToMaterialPropertyBlockFloat(_deathEffectRenderers, FlashEffectPropertyId))

                    .Join(LMotion.Punch.Create(0f, 1f, _punchDuration)
                        .WithFrequency(Random.Range(9, 18))
                        .WithEase(Ease.OutQuad)
                        .BindToLocalPositionX(_charaTransform))
                    .Join(LMotion.Punch.Create(0f, 1f, _punchDuration)
                        .WithFrequency(Random.Range(9, 18))
                        .WithEase(Ease.OutQuad)
                        .BindToLocalPositionZ(_charaTransform))

                    .Join(LMotion.Create(1f, 0f, _deathEffectDuration)
                        .WithEase(Ease.Linear)
                        .BindToMaterialPropertyBlockFloat(_deathEffectRenderers, DeathEffectPropertyId))

                    .Join(LMotion.Create(Vector3.up * -0.5f, Vector3.up * 0.1f, _deathEffectDuration)
                        .WithEase(Ease.OutQuad)
                        .BindToLocalPosition(_deathSwampGameObject.transform))
                    .Join(LMotion.Create(Vector3.up * 0.1f, Vector3.up * -0.5f, _deathSwampSinkDuration)
                        .WithDelay(_deathEffectDuration)
                        .BindToLocalPosition(_deathSwampGameObject.transform))
                    .Run();

            await _handle.ToValueTask(destroyCancellationToken);
        }
        /// <summary>
        ///     プールから再利用した際に、前回の死亡演出で変化したMaterialPropertyBlockを既定値へ戻す。
        /// </summary>
        public void ResetDeathEffect()
        {
            if (_deathEffectRenderers == null || _deathEffectRenderers.Length == 0)
            {
                return;
            }

            foreach (Renderer renderer in _deathEffectRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.SetPropertyBlock(null);
            }
            if (_deathSwampGameObject != null)
            {
                _deathSwampGameObject.SetActive(false);
            }
        }


        [SerializeField, Tooltip("死亡演出でMaterialプロパティを変化させる対象のRendererです。未設定の場合は何もしません。")]
        private Renderer[] _deathEffectRenderers;
        [SerializeField, Tooltip("死亡演出用の沼のGameObjectです。")]
        private GameObject _deathSwampGameObject;
        [SerializeField, Tooltip("シェイク用のキャラTransform")]
        private Transform _charaTransform;

        [SerializeField, Min(0f), Tooltip("死亡演出のMaterialプロパティが変化する時間（秒）です。")]
        private float _deathEffectDuration = 1f;
        [SerializeField, Min(0f), Tooltip("死亡演出後、沼が沈み込むまでの時間（秒）です。")]
        private float _deathSwampSinkDuration = 3f;
        [SerializeField, Min(0f), Tooltip("死亡時フラッシュ時間（秒）")]
        private float _flashDuration = 0.5f;
        [SerializeField, Min(0f), Tooltip("死亡時パンチシェイク時間（秒）")]
        private float _punchDuration = 0.5f;

        private void Awake()
        {
            if (_deathEffectRenderers == null || _deathEffectRenderers.Length == 0)
            {
                Debug.LogError($"{nameof(EnemyDestroyEffectView)}: _deathEffectRenderersが未アタッチです. 死亡エフェクトが再生できません", this);
            }
            if (_deathSwampGameObject == null)
            {
                Debug.LogError($"{nameof(EnemyDestroyEffectView)}: _deathSwampGameObjectが未アタッチです. 死亡エフェクトが再生できません", this);
            }
        }
        private void OnDestroy()
        {
            _handle.TryCancel();
        }

        private MotionHandle _handle;
        private static readonly int DeathEffectPropertyId = Shader.PropertyToID("_DeathEffectAmount");
        private static readonly int FlashEffectPropertyId = Shader.PropertyToID("_FlashEffect");
    }
}
