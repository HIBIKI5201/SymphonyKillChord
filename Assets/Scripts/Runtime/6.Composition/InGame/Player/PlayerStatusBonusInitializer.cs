using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.OutGame.SkillTree;
using KillChord.Runtime.Utility.Identity;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Player
{
    /// <summary>
    ///     InGame で使用するプレイヤーステータスボーナスを初期化するクラスです。
    /// </summary>
    public sealed class PlayerStatusBonusInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(PlayerStatusBonusInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 490;

        [SerializeField, SourceDataAddress]
        [Tooltip("スキルノード定義リポジトリの Addressables キーです。")]
        private string _skillNodeDataRepoKey;

        private SkillNodeDataRepo _loadedSkillNodeDataRepo;
        private PlayerStatusBonus _playerStatusBonus = PlayerStatusBonus.None;
        private PlayerStatusBonusModuleContainer _moduleContainer;

        /// <summary>
        ///     解放済みスキルノードからプレイヤーステータスボーナスを読み込みます。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 読み込みに成功した場合は true です。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            _playerStatusBonus = PlayerStatusBonus.None;

            try
            {
                _loadedSkillNodeDataRepo =
                    await _skillNodeDataRepoKey.LoadAssetAsync<SkillNodeDataRepo>(this, cancellationToken);
                if (_loadedSkillNodeDataRepo == null)
                {
#if UNITY_EDITOR
                    Debug.LogError(
                        $"[{nameof(PlayerStatusBonusInitializer)}] {nameof(SkillNodeDataRepo)} の読み込み結果が null です。",
                        this);
#endif
                    ReleaseLoadedSkillNodeDataRepo();
                    return false;
                }

                SavedataSkillUnlockRepository savedataSkillUnlockRepository =
                    new SavedataSkillUnlockRepository();
                PlayerStatusBonusCalculator calculator =
                    new PlayerStatusBonusCalculator(_loadedSkillNodeDataRepo.GetAll());

                LoadPlayerStatusBonusUseCase useCase = new LoadPlayerStatusBonusUseCase(
                    savedataSkillUnlockRepository,
                    calculator);
                _playerStatusBonus = await useCase.ExecuteAsync(cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                ReleaseLoadedSkillNodeDataRepo();
                throw;
            }
            catch (Exception exception)
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(PlayerStatusBonusInitializer)}] プレイヤーステータスボーナスの読み込みに失敗しました: {exception}",
                    this);
#endif
                ReleaseLoadedSkillNodeDataRepo();
                _playerStatusBonus = PlayerStatusBonus.None;
                return false;
            }
        }

        /// <summary>
        ///     集計済みプレイヤーステータスボーナスを公開します。
        /// </summary>
        /// <returns> 登録に成功した場合は true です。 </returns>
        public override bool Build()
        {
            if (_loadedSkillNodeDataRepo == null)
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(PlayerStatusBonusInitializer)}] {nameof(SkillNodeDataRepo)} が読み込まれていません。",
                    this);
#endif
                return false;
            }

            if (_moduleContainer != null)
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(PlayerStatusBonusInitializer)}] {nameof(PlayerStatusBonusModuleContainer)} は登録済みです。",
                    this);
#endif
                return false;
            }

            if (ServiceLocator.TryGetInstance<PlayerStatusBonusModuleContainer>(out _))
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(PlayerStatusBonusInitializer)}] 別の {nameof(PlayerStatusBonusModuleContainer)} が登録されています。",
                    this);
#endif
                return false;
            }

            _moduleContainer = new PlayerStatusBonusModuleContainer(_playerStatusBonus);
            ServiceLocator.RegisterInstance(_moduleContainer);
            return true;
        }

        /// <summary>
        ///     登録済み Container を解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (_moduleContainer != null
                && ServiceLocator.TryGetInstance<PlayerStatusBonusModuleContainer>(out var registeredContainer)
                && ReferenceEquals(registeredContainer, _moduleContainer))
            {
#if UNITY_EDITOR
                Debug.Log(
                    $"[{nameof(PlayerStatusBonusInitializer)}] {nameof(PlayerStatusBonusModuleContainer)} の登録を解除します。",
                    this);
#endif
                ServiceLocator.UnregisterInstance<PlayerStatusBonusModuleContainer>();
            }

            ReleaseLoadedSkillNodeDataRepo();
            _playerStatusBonus = PlayerStatusBonus.None;
            _moduleContainer = null;
        }

        /// <summary>
        ///     読み込み済みのスキルノード定義リポジトリを解放します。
        /// </summary>
        private void ReleaseLoadedSkillNodeDataRepo()
        {
            _skillNodeDataRepoKey.ReleaseLoadedAsset(this);
            _loadedSkillNodeDataRepo = null;
        }
    }
}
