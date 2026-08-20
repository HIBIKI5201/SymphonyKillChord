using KillChord.Runtime.Utility.Collections;
using LitMotion;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace KillChord.Runtime.View.InGame.UI
{
    using Camera = UnityEngine.Camera;

    /// <summary>
    ///     プレイヤーの任意の位置で画面外の敵方向を3Dマーカーとして描画する。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.HUD)]
    public sealed class EnemyDirectionIndicatorView : MonoBehaviour
    {
        /// <summary> 表示更新タイミングを通知する。 </summary>
        public event Action OnUpdate;

        /// <summary> 生成済みの表示スロット数。 </summary>
        public int Capacity => _slots?.Length ?? 0;

        /// <summary>
        ///     判定用Camera、表示数、位置、フェード時間を受け取って初期化する。
        /// </summary>
        /// <param name="camera"> 画面外判定に使用するCamera。 </param>
        /// <param name="capacity"> 生成する表示スロット数。 </param>
        /// <param name="positionOffset"> プレイヤー原点からのローカル位置。 </param>
        /// <param name="fadeEase"> 表示・非表示のフェードイージング。 </param>
        /// <param name="fadeDuration"> 表示・非表示のフェード時間。 </param>
        /// <returns> 初期化に成功した場合はtrue。 </returns>
        public bool Initialize(
            Camera camera,
            int capacity,
            in Vector3 positionOffset,
            Ease fadeEase,
            float fadeDuration)
        {
            if (_isInitialized)
            {
                Debug.LogWarning($"[{nameof(EnemyDirectionIndicatorView)}] 既に初期化されています。", this);
                return true;
            }

            if (!ValidateReferences(camera, capacity, fadeDuration))
            {
                return false;
            }

            _camera = camera;
            _fadeDuration = fadeDuration;
            _fadeEase = fadeEase;
            _slots = new EnemyDirectionIndicatorSlot[capacity];
            _frustumPlanes = new Plane[FRUSTUM_PLANE_COUNT];
            transform.SetLocalPositionAndRotation(positionOffset, Quaternion.identity);
            transform.localScale = Vector3.one;

            for (int i = 0; i < capacity; i++)
            {
                GameObject indicator = Instantiate(_indicatorPrefab, transform, false);
                indicator.name = $"EnemyDirectionIndicator_{i}";
                indicator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                indicator.transform.localScale = Vector3.one;

                Renderer[] renderers = indicator.GetComponentsInChildren<Renderer>(true);
                EnemyDirectionIndicatorSlot slot = new(indicator, renderers);
                slot.ApplyAlpha(0f);
                indicator.SetActive(false);
                _slots[i] = slot;
            }

            _isInitialized = true;
            return true;
        }

        /// <summary>
        ///     対象BoundsがCameraの表示範囲外か判定する。
        /// </summary>
        /// <param name="worldBounds"> 判定するワールド空間Bounds。 </param>
        /// <returns> 画面外の場合はtrue。 </returns>
        public bool IsOutsideViewport(Bounds worldBounds)
        {
            if (_camera == null || _frustumPlanes == null)
            {
                return false;
            }

            return !GeometryUtility.TestPlanesAABB(_frustumPlanes, worldBounds);
        }

        /// <summary>
        ///     指定スロットを敵方向へ回転する。
        /// </summary>
        /// <param name="slotIndex"> 更新する表示スロット番号。 </param>
        /// <param name="direction"> プレイヤーから敵への水平方向。 </param>
        public void SetDirection(int slotIndex, in Vector3 direction)
        {
            EnemyDirectionIndicatorSlot slot = GetSlot(slotIndex);
            if (direction.sqrMagnitude <= DIRECTION_SQR_EPSILON)
            {
                return;
            }

            slot.Transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        /// <summary>
        ///     指定スロットの表示状態をフェードで切り替える。
        /// </summary>
        /// <param name="slotIndex"> 更新する表示スロット番号。 </param>
        /// <param name="isVisible"> 表示する場合はtrue。 </param>
        public void SetVisibility(int slotIndex, bool isVisible)
        {
            EnemyDirectionIndicatorSlot slot = GetSlot(slotIndex);
            if (slot.IsVisible == isVisible)
            {
                return;
            }

            slot.RecordVisibility(isVisible);
            slot.CancelMotion();

            if (isVisible)
            {
                slot.GameObject.SetActive(true);
            }

            float targetAlpha = isVisible ? 1f : 0f;
            if (_fadeDuration <= 0f)
            {
                slot.CompleteVisibility(targetAlpha);
                return;
            }

            MotionHandle motionHandle = LMotion.Create(slot.CurrentAlpha, targetAlpha, _fadeDuration)
                .WithEase(_fadeEase)
                .Bind(slot, static (alpha, state) => state.ApplyFadeAlpha(alpha));
            slot.RecordMotion(motionHandle);
        }

        /// <summary> 方向判定でゼロベクトルを除外する二乗長の閾値。 </summary>
        private const float DIRECTION_SQR_EPSILON = 0.000001f;

        /// <summary> カメラフラスタムを構成する平面数。 </summary>
        private const int FRUSTUM_PLANE_COUNT = 6;

        [SerializeField, Tooltip("1つの敵方向表示に使用する3DマーカーPrefab。")]
        private GameObject _indicatorPrefab;

        private Camera _camera;
        private EnemyDirectionIndicatorSlot[] _slots;
        private Plane[] _frustumPlanes;
        private float _fadeDuration;
        private Ease _fadeEase;
        private bool _isInitialized;

        /// <summary>
        ///     Camera更新後に表示更新を通知する。
        /// </summary>
        private void LateUpdate()
        {
            if (_isInitialized && _camera != null)
            {
                GeometryUtility.CalculateFrustumPlanes(_camera, _frustumPlanes);
                OnUpdate?.Invoke();
            }
        }

        /// <summary>
        ///     イベントと再生中のMotionを解放する。
        /// </summary>
        private void OnDestroy()
        {
            OnUpdate = null;

            if (_slots == null)
            {
                return;
            }

            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i]?.CancelMotion();
            }
        }

        /// <summary>
        ///     初期化に必要な参照と設定を検証する。
        /// </summary>
        /// <param name="camera"> 判定用Camera。 </param>
        /// <param name="capacity"> 生成する表示スロット数。 </param>
        /// <param name="fadeDuration"> 表示・非表示のフェード時間。 </param>
        /// <returns> 初期化可能な場合はtrue。 </returns>
        private bool ValidateReferences(
            Camera camera,
            int capacity,
            float fadeDuration)
        {
            if (camera == null
                || _indicatorPrefab == null
                || capacity <= 0
                || fadeDuration < 0f
                || float.IsNaN(fadeDuration)
                || float.IsInfinity(fadeDuration))
            {
                Debug.LogError($"[{nameof(EnemyDirectionIndicatorView)}] 初期化参照または設定値が不正です。", this);
                return false;
            }

            Renderer[] prefabRenderers = _indicatorPrefab.GetComponentsInChildren<Renderer>(true);
            if (prefabRenderers.Length == 0)
            {
                Debug.LogError($"[{nameof(EnemyDirectionIndicatorView)}] マーカーPrefabにRendererがありません。", this);
                return false;
            }

            for (int rendererIndex = 0; rendererIndex < prefabRenderers.Length; rendererIndex++)
            {
                Material[] materials = prefabRenderers[rendererIndex].sharedMaterials;
                if (materials.Length == 0)
                {
                    Debug.LogError(
                        $"[{nameof(EnemyDirectionIndicatorView)}] マーカーRendererにMaterialがありません。",
                        this);
                    return false;
                }

                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    Material material = materials[materialIndex];
                    if (material == null || material.shader == null)
                    {
                        Debug.LogError(
                            $"[{nameof(EnemyDirectionIndicatorView)}] マーカーMaterialまたはShaderがありません。",
                            this);
                        return false;
                    }

                    if (material.renderQueue < (int)RenderQueue.Transparent)
                    {
                        Debug.LogError(
                            $"[{nameof(EnemyDirectionIndicatorView)}] マーカーMaterialは透明描画キューに設定してください。",
                            this);
                        return false;
                    }

                    if (EnemyDirectionIndicatorSlot.TryGetColorPropertyId(material, out _))
                    {
                        continue;
                    }

                    Debug.LogError(
                        $"[{nameof(EnemyDirectionIndicatorView)}] マーカーMaterialに透明度を制御できるメインカラーがありません。",
                        this);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     指定番号の表示スロットを取得する。
        /// </summary>
        /// <param name="slotIndex"> 取得する表示スロット番号。 </param>
        /// <returns> 表示スロット。 </returns>
        private EnemyDirectionIndicatorSlot GetSlot(int slotIndex)
        {
            if (_slots == null || slotIndex < 0 || slotIndex >= _slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(slotIndex));
            }

            return _slots[slotIndex];
        }

    }
}
