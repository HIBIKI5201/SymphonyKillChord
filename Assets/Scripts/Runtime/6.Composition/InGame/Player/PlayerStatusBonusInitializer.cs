using KillChord.Runtime.Application.OutGame.SkillTree;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.OutGame.SkillTree;
using KillChord.Runtime.Utility.OutGame.Savedata;
using SymphonyFrameWork.System.ServiceLocate;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Player
{
    /// <summary>
    ///     InGameで使用するプレイヤーステータスボーナスを初期化するクラスです。
    /// </summary>
    public sealed class PlayerStatusBonusInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(PlayerStatusBonusInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 490;

        [SerializeField, Tooltip("スキルノード定義リポジトリです。")]
        private SkillNodeDataRepo _skillNodeDataRepo;

        private PlayerStatusBonus _playerStatusBonus = PlayerStatusBonus.None;
        private bool _isRegistered;

        /// <summary>
        ///     解放済みスキルノードからプレイヤーステータスボーナスを読み込みます。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 読み込みに成功した場合はtrueです。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            if (_skillNodeDataRepo == null)
            {
                Debug.LogError(
                    $"[{nameof(PlayerStatusBonusInitializer)}] {nameof(SkillNodeDataRepo)} が設定されていません。",
                    this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                Debug.LogError(
                    $"[{nameof(PlayerStatusBonusInitializer)}] {nameof(SavedataSystem)} が見つかりません。",
                    this);
                return false;
            }

            LoadPlayerStatusBonusUseCase useCase = new LoadPlayerStatusBonusUseCase(
                _skillNodeDataRepo,
                new SavedataSkillUnlockRepository(savedataSystem));
            _playerStatusBonus = await useCase.ExecuteAsync(cancellationToken);
            return true;
        }

        /// <summary>
        ///     集計済みプレイヤーステータスボーナスを公開します。
        /// </summary>
        /// <returns> 登録に成功した場合はtrueです。 </returns>
        public override bool Build()
        {
            PlayerStatusBonusModuleContainer moduleContainer =
                new PlayerStatusBonusModuleContainer(_playerStatusBonus);
            ServiceLocator.RegisterInstance(moduleContainer);
            _isRegistered = true;
            return true;
        }

        /// <summary>
        ///     登録済みContainerを解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (!_isRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<PlayerStatusBonusModuleContainer>();
            _isRegistered = false;
        }
    }
}
