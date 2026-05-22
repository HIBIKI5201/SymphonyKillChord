using KillChord.Runtime.View.InGame.Enemy;
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
        ///     初期化処理。
        /// </summary>
        public void Initialize()
        {
            InitializeInfantryPool();
            InitializeArtilleryPool();
            InitializeShellPool();
        }

        /// <summary>
        ///     歩兵用のObject Poolを初期化する。
        /// </summary>
        public void InitializeInfantryPool()
        {
            _infantryPool = new ObjectPool<EnemyLifeCycle>(
                createFunc: InstantiateInfantry,
                collectionCheck: true,
                defaultCapacity: _defaultInfantryPoolSize,
                maxSize: _maxInfantryPoolSize);
        }
        /// <summary>
        ///     砲兵用のObject Poolを初期化する。
        /// </summary>
        public void InitializeArtilleryPool()
        {
            _artilleryPool = new ObjectPool<EnemyLifeCycle>(
                createFunc: InstantiateArtillery,
                collectionCheck: true,
                defaultCapacity: _defaultArtilleryPoolSize,
                maxSize: _maxArtilleryPoolSize);
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
                maxSize: _defaultShellPoolSize);
        }

        /// <summary>
        ///     歩兵のGameObjectを生成し、初期化する。
        /// </summary>
        /// <returns></returns>
        public EnemyLifeCycle InstantiateInfantry()
        {
            EnemyLifeCycle lifeCycle = Instantiate(_infantryPrefab);
            _initializer.InitializeInfantry(lifeCycle, ReleaseInfantry);
            return lifeCycle;
        }

        /// <summary>
        ///     砲兵のGameObjectを生成し、初期化する。
        /// </summary>
        /// <returns></returns>
        public EnemyLifeCycle InstantiateArtillery()
        {
            EnemyLifeCycle lifeCycle = Instantiate(_artilleryPrefab);
            _initializer.InitializeArtillery(lifeCycle, ReleaseArtillery);
            return lifeCycle;
        }

        /// <summary>
        ///     砲弾のGameObjectを生成し、初期化する。
        /// </summary>
        /// <returns></returns>
        public ShellLifeCycle InstantiateShell()
        {
            ShellLifeCycle shell = Instantiate(_shellPrefab);
            shell.Initialize(ReleaseShell);
            return shell;
        }

        /// <summary>
        ///     Object Poolから歩兵のGameObjectを取り出す。
        /// </summary>
        /// <returns></returns>
        public EnemyLifeCycle GetInfantry()
        {
            return _infantryPool.Get();
        }

        /// <summary>
        ///     歩兵のGameObjectをリリースし、Object Poolに戻す。
        /// </summary>
        /// <param name="element"></param>
        public void ReleaseInfantry(EnemyLifeCycle element)
        {
            _infantryPool.Release(element);
        }

        /// <summary>
        ///     Object Poolから砲兵のGameObjectを取り出す。
        /// </summary>
        /// <returns></returns>
        public EnemyLifeCycle GetArtillery()
        {
            return _artilleryPool.Get();
        }

        /// <summary>
        ///     砲兵のGameObjectをリリースし、Object Poolに戻す。
        /// </summary>
        /// <param name="element"></param>
        public void ReleaseArtillery(EnemyLifeCycle element)
        {
            _artilleryPool.Release(element);
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

        [SerializeField] private EnemyInitializer _initializer;

        [Header("歩兵")]
        [SerializeField] private EnemyLifeCycle _infantryPrefab;
        [SerializeField, Tooltip("初期Poolサイズ")] private int _defaultInfantryPoolSize;
        [SerializeField, Tooltip("最大Poolサイズ")] private int _maxInfantryPoolSize;

        [Header("砲兵")]
        [SerializeField] private EnemyLifeCycle _artilleryPrefab;
        [SerializeField, Tooltip("初期Poolサイズ")] private int _defaultArtilleryPoolSize;
        [SerializeField, Tooltip("最大Poolサイズ")] private int _maxArtilleryPoolSize;
        [SerializeField] private ShellLifeCycle _shellPrefab;
        [SerializeField, Tooltip("初期Poolサイズ")] private int _defaultShellPoolSize;
        [SerializeField, Tooltip("最大Poolサイズ")] private int _maxShellPoolSize;

        private IObjectPool<EnemyLifeCycle> _infantryPool;
        private IObjectPool<EnemyLifeCycle> _artilleryPool;
        private IObjectPool<ShellLifeCycle> _shellPool;
    }
}
