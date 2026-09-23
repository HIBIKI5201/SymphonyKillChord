using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.View.InGame.Character;
using KillChord.Runtime.View.InGame.Enemy;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵のインスタンスプーリングを行うクラス。
    /// </summary>
    public class EnemyPools : MonoBehaviour, IShellPool
    {
        /// <summary>
        ///     プールが参照するプレハブ群の Addressables アセットを先行ロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public async Task<bool> LoadAddressableAssetsAsync(CancellationToken cancellationToken)
        {
            bool shellLoaded = _shellPrefab != null && await _shellPrefab.LoadAddressableAssetsAsync(cancellationToken);
            return shellLoaded;
        }

        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="repository"> 個別の敵定義リポジトリです。 </param>
        public void Initialize(EnemyDefinitionRepository repository)
        {
            _enemyPools.Clear();
            IReadOnlyList<EnemyDefinitionAsset> definitions = repository.Definitions;
            for (int i = 0; i < definitions.Count; i++)
            {
                EnemyDefinitionAsset definition = definitions[i];
                if (definition == null
                    || definition.Id.Value == 0
                    || definition.ViewPrefab == null
                    || _enemyPools.ContainsKey(definition.Id))
                {
                    continue;
                }

                EnemyDefinitionId enemyDefinitionId = definition.Id;
                _enemyPools[enemyDefinitionId] = new ObjectPool<EnemyLifeCycle>(
                    createFunc: () => InstantiateEnemy(definition),
                    collectionCheck: true,
                    defaultCapacity: Mathf.Max(1, definition.DefaultPoolSize),
                    maxSize: Mathf.Max(definition.DefaultPoolSize, definition.MaxPoolSize));
            }

            InitializeShellPool();
        }

        /// <summary>
        ///     ボス単体検証など、通常敵を生成しない環境向けに砲弾Poolだけを初期化します。
        /// </summary>
        public void InitializeShellOnly()
        {
            InitializeShellPool();
        }

        /// <summary>
        ///     個別の敵定義に対応するObject Poolから敵を取り出します。
        /// </summary>
        /// <param name="enemyDefinitionId"> 取得する個別の敵定義IDです。 </param>
        /// <returns> 取得した敵ライフサイクルです。 </returns>
        public EnemyLifeCycle GetEnemy(EnemyDefinitionId enemyDefinitionId)
        {
            if (_enemyPools.TryGetValue(enemyDefinitionId, out IObjectPool<EnemyLifeCycle> pool))
            {
                return pool.Get();
            }

            throw new InvalidOperationException(
                $"{nameof(EnemyDefinitionId)}に対応するObjectPoolがありません。 Id: {enemyDefinitionId.Value}");
        }
        /// <summary>
        ///     個別の敵定義に対応するObject Poolを事前にウォームアップします。
        /// </summary>
        /// <param name="enemyDefinitionId"> ウォームアップする個別の敵定義IDです。 </param>
        /// <param name="count"> 事前生成する数です。 </param>
        public void PrewarmEnemy(EnemyDefinitionId enemyDefinitionId, int count)
        {
            if (count <= 0 || !_enemyPools.ContainsKey(enemyDefinitionId))
            {
                return;
            }

            EnemyLifeCycle[] warmedInstances = new EnemyLifeCycle[count];
            for (int i = 0; i < count; i++)
            {
                EnemyLifeCycle instance = GetEnemy(enemyDefinitionId);
                warmedInstances[i] = instance;

                RenderForShaderWarmup(instance);
                instance.gameObject.SetActive(false);
            }

            for (int i = 0; i < count; i++)
            {
                ReleaseEnemy(enemyDefinitionId, warmedInstances[i]);
            }
        }

        /// <summary>
        ///     指定した敵インスタンスを画面外の専用カメラで1回描画します。
        /// </summary>
        /// <param name="instance"> 描画ウォームアップ対象の敵インスタンスです。 </param>
        private void RenderForShaderWarmup(EnemyLifeCycle instance)
        {
            if (!_renderWarmupEnabled)
            {
                return;
            }

            UnityEngine.Camera warmupCamera = GetOrCreateWarmupCamera();
            instance.transform.SetPositionAndRotation(WARMUP_POSITION, Quaternion.identity);
            instance.gameObject.SetActive(true);
            warmupCamera.Render();
        }

        /// <summary>
        ///     シェーダーウォームアップ専用の非表示カメラを取得します。未生成なら作成します。
        /// </summary>
        /// <returns> ウォームアップ用カメラです。 </returns>
        private UnityEngine.Camera GetOrCreateWarmupCamera()
        {
            if (_warmupCamera != null)
            {
                return _warmupCamera;
            }

            GameObject cameraObject = new GameObject(nameof(EnemyPools) + "ShaderWarmupCamera");
            cameraObject.hideFlags = HideFlags.HideAndDontSave;
            cameraObject.transform.SetPositionAndRotation(
                WARMUP_POSITION + new Vector3(0f, 0f, -WARMUP_CAMERA_DISTANCE),
                Quaternion.identity);

            _warmupCamera = cameraObject.AddComponent<UnityEngine.Camera>();
            _warmupCamera.enabled = false;
            _warmupCamera.clearFlags = CameraClearFlags.SolidColor;
            _warmupCamera.backgroundColor = Color.clear;
            _warmupCamera.nearClipPlane = 0.1f;
            _warmupCamera.farClipPlane = WARMUP_CAMERA_DISTANCE * 4f;

            _warmupRenderTexture = new RenderTexture(4, 4, 16);
            _warmupCamera.targetTexture = _warmupRenderTexture;

            return _warmupCamera;
        }

        /// <summary>
        ///     ウォームアップ用カメラと RenderTexture を破棄します。
        /// </summary>
        private void OnDestroy()
        {
            if (_warmupCamera != null)
            {
                Destroy(_warmupCamera.gameObject);
                _warmupCamera = null;
            }

            if (_warmupRenderTexture != null)
            {
                _warmupRenderTexture.Release();
                Destroy(_warmupRenderTexture);
                _warmupRenderTexture = null;
            }
        }

        /// <summary>
        ///     砲弾用のObject Poolを初期化する。
        /// </summary>
        public void InitializeShellPool()
        {
            _shellPool = new ObjectPool<ShellLifeCycle>(
                createFunc: InstantiateShell,
                collectionCheck: true,
                defaultCapacity: _defaultShellPoolSize,
                maxSize: _maxShellPoolSize);
        }

        /// <summary>
        ///     個別の敵定義が指定するGameObjectを生成し、初期化します。
        /// </summary>
        /// <param name="definition"> 適用する個別の敵定義です。 </param>
        /// <returns> 初期化した敵ライフサイクルです。 </returns>
        public EnemyLifeCycle InstantiateEnemy(EnemyDefinitionAsset definition)
        {
            GameObject instance = Instantiate(definition.ViewPrefab);
            if (!instance.TryGetComponent(out EnemyLifeCycle lifeCycle))
            {
                Destroy(instance);
                throw new InvalidOperationException(
                    $"敵プレハブに{nameof(EnemyLifeCycle)}がありません。 Prefab: {definition.ViewPrefab.name}");
            }

            if (!lifeCycle.Configure(definition))
            {
                Destroy(instance);
                throw new InvalidOperationException(
                    $"敵定義の必須データが不足しています。 Definition: {definition.name}");
            }

            _initializer.InitializeEnemy(
                lifeCycle,
                definition.EnemyType,
                element => ReleaseEnemy(definition.Id, element));
            return lifeCycle;
        }

        /// <summary>
        ///     砲弾のGameObjectを生成し、初期化する。
        /// </summary>
        /// <returns></returns>
        public ShellLifeCycle InstantiateShell()
        {
            ShellLifeCycle shell = Instantiate(_shellPrefab);
            shell.CopyLoadedAssetsFrom(_shellPrefab);
            shell.Initialize(ReleaseShell, _shellExplosionEffectView);
            return shell;
        }

        /// <summary>
        ///     敵を対応する個別定義のObject Poolへ戻します。
        /// </summary>
        /// <param name="enemyDefinitionId"> 敵定義IDです。 </param>
        /// <param name="element"> 回収する敵です。 </param>
        public void ReleaseEnemy(EnemyDefinitionId enemyDefinitionId, EnemyLifeCycle element)
        {
            if (_enemyPools.TryGetValue(enemyDefinitionId, out IObjectPool<EnemyLifeCycle> pool))
            {
                pool.Release(element);
                return;
            }

            Destroy(element.gameObject);
        }

        /// <summary>
        ///     Object Poolから砲弾のGameObjectを取り出す。
        /// </summary>
        /// <returns></returns>
        public IShellLifeCycle GetShell()
        {
            return _shellPool.Get();
        }

        /// <summary>
        ///     砲弾のGameObjectをリリースし、Object Poolに戻す。
        /// </summary>
        /// <param name="element"></param>
        public void ReleaseShell(ShellLifeCycle element)
        {
            _shellPool.Release(element);
        }

        [SerializeField, Tooltip("敵インスタンスへランタイム依存を注入するInitializerです。")]
        private EnemyInitializer _initializer;

        [SerializeField, Tooltip("砲兵が使用する砲弾プレハブです。")]
        private ShellLifeCycle _shellPrefab;
        [SerializeField, Tooltip("初期Poolサイズ")] private int _defaultShellPoolSize;
        [SerializeField, Tooltip("最大Poolサイズ")] private int _maxShellPoolSize;
        [SerializeField, Tooltip("砲弾着弾時の爆発エフェクトです。")]
        private ReusableParticleSystemView _shellExplosionEffectView;

        [SerializeField, Tooltip("プール事前生成時に、画面外の専用カメラで1回描画してシェーダーコンパイルを" +
            "前払いするかどうかです。レンダーパイプラインの都合で問題が起きる場合はfalseにしてください。")]
        private bool _renderWarmupEnabled = true;

        private static readonly Vector3 WARMUP_POSITION = new(0f, -5000f, 0f);
        private const float WARMUP_CAMERA_DISTANCE = 3f;

        private readonly Dictionary<EnemyDefinitionId, IObjectPool<EnemyLifeCycle>> _enemyPools = new();
        private IObjectPool<ShellLifeCycle> _shellPool;
        private UnityEngine.Camera _warmupCamera;
        private RenderTexture _warmupRenderTexture;
    }
}
