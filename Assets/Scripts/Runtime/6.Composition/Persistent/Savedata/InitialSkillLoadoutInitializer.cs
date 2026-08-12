using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.SkillBuild;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.System.SaveSystem;
using SymphonyFrameWork.System.ServiceLocate;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Savedata
{
    /// <summary>
    ///     セーブデータへ初期解放・初期装備スキルを補完する初期化モジュール。
    ///     <para>
    ///         Persistentシーンで実行することで、OutGameシーンを経由しない起動経路
    ///         （ステージ単体の直接再生など）でも初期スキル状態を保証する。
    ///     </para>
    /// </summary>
    public sealed class InitialSkillLoadoutInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(InitialSkillLoadoutInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 20;

        [SerializeField, SourceDataAddress]
        [Tooltip("ゲーム開始時の初期解放・初期装備スキルを定義するアセットの Addressables キーです。")]
        private string _initialSkillLoadoutKey;

        private InitialSkillLoadoutAsset _loadedInitialSkillLoadout;
        private InitialSkillLoadoutService _initialSkillLoadoutService;
        private bool _isServiceRegistered;

        /// <summary>
        ///     セーブデータが未設定の場合に、初期解放・初期装備スキルを補完する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            _loadedInitialSkillLoadout = await _initialSkillLoadoutKey.LoadAssetAsync<InitialSkillLoadoutAsset>(this, cancellationToken);

            _initialSkillLoadoutService = new InitialSkillLoadoutService(
                _loadedInitialSkillLoadout.GetUnlockedSkillIds(),
                _loadedInitialSkillLoadout.GetEquippedSkillIds());

            cancellationToken.ThrowIfCancellationRequested();
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();
            cancellationToken.ThrowIfCancellationRequested();

            if (_initialSkillLoadoutService.TryApply(saveData))
            {
                await SaveStore.SaveAsync<SaveData>();
            }

            return true;
        }

        /// <summary>
        ///     初期スキル補完サービスを登録します。
        ///     セーブデータリセット後の再補完で使用するため、起動後も参照できるようにする。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            _isServiceRegistered = ServiceLocator.RegisterInstance(_initialSkillLoadoutService);
            return true;
        }

        /// <summary>
        ///     ロード済みアセットを解放し、登録済みサービスを解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (_isServiceRegistered)
            {
                ServiceLocator.UnregisterInstance<InitialSkillLoadoutService>();
                _isServiceRegistered = false;
            }

            _initialSkillLoadoutService = null;
            _loadedInitialSkillLoadout = null;
            _initialSkillLoadoutKey.ReleaseLoadedAsset(this);
        }
    }
}
