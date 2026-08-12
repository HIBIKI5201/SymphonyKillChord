using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.InfraStructure.InGame.Music;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.InGame.PostEffect;
using LitMotion;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     AC風リズムガイドを初期化するモジュールです。
    /// </summary>
    public class ACLikeRhythmGuideInitializer : InGameInitializationModuleBase
    { /// <summary>
      ///     リズムガイド機能を初期化する。
      /// </summary>
        public override string ModuleName => nameof(ACLikeRhythmGuideInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 800;

        /// <summary>
        ///     他モジュールへ結合してリズムガイドを初期化する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (_rhythmJudgmentDefinitionAsset == null || _rhythmGuideView == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] リズムガイド参照が不足しています。", this);
                return false;
            }

            if (ServiceLocator.GetInstance<MusicSyncModuleContainer>() == null
                || ServiceLocator.GetInstance<TargetSystemModuleContainer>() == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] 必要なContainerが見つかりません。", this);
                return false;
            }

            if (!Initialize())
            {
                return false;
            }

            RegisterGameplayControllable();
            return true;
        }

        /// <summary>
        ///     リズムガイド機能を初期化する。
        /// </summary>
        /// <returns> 初期化に成功した場合はtrueです。 </returns>
        public bool Initialize()
        {
            Debug.Assert(_rhythmJudgmentDefinitionAsset != null, "RhythmJudgmentDefinitionAsset の参照が未設定です。RhythmJudgmentDefinitionAsset を設定してください。");
            Debug.Assert(_rhythmGuideView != null, "RhythmGuideView の参照が未設定です。RhythmGuideView を設定してください。");

            IMusicSyncService musicSyncService = ServiceLocator.GetInstance<MusicSyncModuleContainer>()?.MusicSyncService;

            if (musicSyncService == null)
            {
                Debug.LogError($"{nameof(IMusicSyncService)} が見つかりません。MusicSyncInitializer が先に初期化されているか確認してください。");
                return false;
            }

            TargetSystemController targetingSystem = ServiceLocator.GetInstance<TargetSystemModuleContainer>()?.TargetSystemController;

            if (targetingSystem == null)
            {
                Debug.LogError($"{nameof(TargetSystemController)} が見つかりません。TargetSystemController が登録されているか確認してください。");
                return false;
            }


            RhythmGuidePresenter presenter = new RhythmGuidePresenter(
                musicSyncService,
                new RhythmGuideUsecase(_rhythmJudgmentDefinitionAsset.ToDefinition()),
                targetingSystem
            );

            new ACLikeRhythmGuideViewModel(_rhythmGuideView, presenter);

            // 攻撃入力の拍種に応じてVignetteの強さを変えるため、攻撃実行の通知へ購読する。
            PlayerAttackController playerAttackController =
                ServiceLocator.GetInstance<PlayerModuleContainer>()?.PlayerAttackController;

            if (playerAttackController == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] {nameof(PlayerAttackController)} が見つかりません。PlayerInitializer が先に初期化されているか確認してください。", this);
                return false;
            }

            _playerAttackController = playerAttackController;
            _playerAttackController.OnAttackBeatExecuted += HandleAttackBeatExecuted;

            return true;
        }

        /// <summary>
        ///     攻撃実行通知の購読を解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (_playerAttackController == null)
            {
                return;
            }

            _playerAttackController.OnAttackBeatExecuted -= HandleAttackBeatExecuted;
            _playerAttackController = null;
        }

        /// <summary>
        ///     攻撃入力時に拍種へ応じた強さでVignette演出を再生する。
        /// </summary>
        /// <param name="beatType"> 攻撃が成立した拍の種類。ジャスト判定には使用しない。 </param>
        private void HandleAttackBeatExecuted(BeatType beatType)
        {
            if (_rhythmGuidePostEffectView == null || _rhythmGuideView == null)
            {
                return;
            }

            // 演出設定が無効な場合やビート色を解決できない場合は再生しない。
            if (!_rhythmGuideView.TryGetVignetteParameter(out Color color, out Ease ease, out float duration))
            {
                return;
            }

            // ガイド上のJustTimingMarkerにカーソルが乗っている入力のみをジャストとして扱う。
            float startRatio = _rhythmGuideView.IsOnJustTiming ? JUST_HIT_START_RATIO : NORMAL_HIT_START_RATIO;
            duration = _rhythmGuideView.IsOnJustTiming ? duration * 2 : duration;

            _rhythmGuidePostEffectView.SetColor(color);
            _rhythmGuidePostEffectView.OneShotRatio(ease, duration, startRatio);
        }

        /// <summary>
        ///     リズムガイドViewをゲームプレイ開始対象へ登録します。
        /// </summary>
        private void RegisterGameplayControllable()
        {
            if (_isRegisteredToPlayDirector)
            {
                return;
            }

            InGamePlayDirector inGamePlayDirector = FindFirstObjectByType<InGamePlayDirector>();
            if (inGamePlayDirector == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] {nameof(InGamePlayDirector)} が見つかりません。", this);
                return;
            }

            inGamePlayDirector.AddGamePlayControllable(_rhythmGuideView);
            _isRegisteredToPlayDirector = true;
        }

        /// <summary> ジャストタイミング時のVignette開始強度。 </summary>
        private const float JUST_HIT_START_RATIO = 1f;
        /// <summary> ジャストタイミング以外のVignette開始強度。 </summary>
        private const float NORMAL_HIT_START_RATIO = 0.3f;

        [Tooltip("リズム判定定義アセット。")]
        [SerializeField] private RhythmJudgmentDefinitionAsset _rhythmJudgmentDefinitionAsset;
        [Tooltip("リズムガイドView。")]
        [SerializeField] private ACLikeRhythmGuideView _rhythmGuideView;
        [Tooltip("リズムガイドのフルスクリーン演出View。")]
        [SerializeField] private RhythmGuidePostEffectView _rhythmGuidePostEffectView;

        private bool _isRegisteredToPlayDirector;
        private PlayerAttackController _playerAttackController;
    }
}
