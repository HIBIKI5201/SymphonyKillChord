using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Sequence;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    /// <summary>
    ///     ESC長押し入力からタイトルシーンへ復帰する機能を初期化するクラス。
    /// </summary>
    public sealed class ReturnToTitleInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(ReturnToTitleInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 450;

        [SerializeField, SceneNameSelector, Tooltip("復帰先となるタイトルシーン名です。")]
        private string _titleSceneName;

        private PlayerInputView _playerInputView;
        private ReturnToTitleController _controller;
        private bool _isTransitioning;

        /// <summary>
        ///     入力とシーン遷移を結合してタイトル復帰機能を構築します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (string.IsNullOrWhiteSpace(_titleSceneName))
            {
                Debug.LogError($"[{nameof(ReturnToTitleInitializer)}] タイトルシーン名が設定されていません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out PlayerInputView playerInputView))
            {
                Debug.LogError($"[{nameof(ReturnToTitleInitializer)}] {nameof(PlayerInputView)} が見つかりません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SceneTransitionUsecase sceneTransitionUsecase))
            {
                Debug.LogError($"[{nameof(ReturnToTitleInitializer)}] {nameof(SceneTransitionUsecase)} が見つかりません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SelectedBattleStageState selectedBattleStageState))
            {
                Debug.LogError($"[{nameof(ReturnToTitleInitializer)}] {nameof(SelectedBattleStageState)} が見つかりません。", this);
                return false;
            }

            // ミッション状態は存在すればクリア対象とするため、取得できない場合もnullで許容する。
            ServiceLocator.TryGetInstance(out SelectedMissionState selectedMissionState);

            _controller = new ReturnToTitleController(
                sceneTransitionUsecase,
                selectedBattleStageState,
                selectedMissionState,
                _titleSceneName);

            _playerInputView = playerInputView;
            _playerInputView.OnReturnToTitleInput += HandleReturnToTitleInput;
            return true;
        }

        /// <summary>
        ///     入力購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            Unsubscribe();
            _controller = null;
        }

        /// <summary>
        ///     破棄時に入力購読を解除します。
        /// </summary>
        private void OnDestroy()
        {
            Unsubscribe();
        }

        /// <summary>
        ///     タイトル復帰入力を受け取って遷移を開始します。
        /// </summary>
        /// <param name="input"> タイトル復帰入力です。 </param>
        private void HandleReturnToTitleInput(InputContext<float> input)
        {
            // Hold成立(Performed)時のみ実行する。
            if (input.Phase != InputActionPhase.Performed)
            {
                return;
            }

            if (_isTransitioning || _controller == null)
            {
                return;
            }

            _ = ReturnToTitleAsync();
        }

        /// <summary>
        ///     タイトルシーンへの遷移を非同期で実行します。
        /// </summary>
        private async Awaitable ReturnToTitleAsync()
        {
            _isTransitioning = true;

            try
            {
                bool success = await _controller.ReturnToTitleAsync(
                    gameObject.scene.name,
                    destroyCancellationToken);

                if (!success)
                {
                    Debug.LogError($"[{nameof(ReturnToTitleInitializer)}] タイトルシーンへの復帰に失敗しました。", this);
                    _isTransitioning = false;
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                _isTransitioning = false;
            }
        }

        /// <summary>
        ///     入力購読を解除します。
        /// </summary>
        private void Unsubscribe()
        {
            if (_playerInputView != null)
            {
                _playerInputView.OnReturnToTitleInput -= HandleReturnToTitleInput;
                _playerInputView = null;
            }
        }
    }
}
