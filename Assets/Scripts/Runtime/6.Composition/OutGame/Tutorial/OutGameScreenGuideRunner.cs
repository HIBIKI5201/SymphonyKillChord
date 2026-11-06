using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.OutGame.Tutorial;
using SymphonyFrameWork.System.SaveSystem;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.Tutorial
{
    /// <summary>
    ///     研究・改造・作戦の各画面を初めて開いたときに、画面の使い方を案内します。
    ///     順番は決めず、開いた画面の案内だけを一度出します。研究画面では最初のノードの解放を促します。
    /// </summary>
    public sealed class OutGameScreenGuideRunner : IDisposable
    {
        /// <summary>
        ///     案内に使うセーブデータと、案内を出す画面を受け取ります。
        /// </summary>
        /// <param name="saveData"> 案内の進行を記録するセーブデータです。 </param>
        /// <param name="skillTreeScreenView"> 研究画面です。無い場合はnullです。 </param>
        /// <param name="skillBuildScreenView"> 改造画面です。無い場合はnullです。 </param>
        /// <param name="stageSelectScreenView"> 作戦画面です。無い場合はnullです。 </param>
        /// <param name="cancellationToken"> 破棄時に案内を止めるキャンセルトークンです。 </param>
        public OutGameScreenGuideRunner(
            SaveData saveData,
            SkillTreeScreenView skillTreeScreenView,
            SkillBuildScreenView skillBuildScreenView,
            StageSelectScreenView stageSelectScreenView,
            CancellationToken cancellationToken)
        {
            _saveData = saveData ?? throw new ArgumentNullException(nameof(saveData));
            _skillTreeScreenView = skillTreeScreenView;
            _skillBuildScreenView = skillBuildScreenView;
            _stageSelectScreenView = stageSelectScreenView;
            _cancellationToken = cancellationToken;

            // 各画面の表示完了を購読する。
            if (_skillTreeScreenView != null)
            {
                _skillTreeScreenView.OnShowCompleted += HandleSkillTreeShowCompleted;
            }

            if (_skillBuildScreenView != null)
            {
                _skillBuildScreenView.OnShowCompleted += HandleSkillBuildShowCompleted;
            }

            if (_stageSelectScreenView != null)
            {
                _stageSelectScreenView.OnShowCompleted += HandleStageSelectShowCompleted;
            }
        }

        /// <summary>
        ///     購読を解除し、表示中の案内を閉じます。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            if (_skillTreeScreenView != null)
            {
                _skillTreeScreenView.OnShowCompleted -= HandleSkillTreeShowCompleted;
            }

            if (_skillBuildScreenView != null)
            {
                _skillBuildScreenView.OnShowCompleted -= HandleSkillBuildShowCompleted;
            }

            if (_stageSelectScreenView != null)
            {
                _stageSelectScreenView.OnShowCompleted -= HandleStageSelectShowCompleted;
            }

            _activeOverlayView?.Dispose();
            _activeOverlayView = null;
        }

        /// <summary> 研究画面の案内です。最初のノード（キルコード1）の解放を促します。 </summary>
        private static readonly ScreenGuideStep[] SKILL_TREE_STEPS =
        {
            new ScreenGuideStep(
                "UnlockPointInfo",
                "ui.screen_guide.skill_tree.points",
                "解放ポイントを使って研究を進め、キルコードや強化を解放できます。解放ポイントはステージの初回クリアなどで手に入ります。"),
            new ScreenGuideStep(
                "N1",
                "ui.screen_guide.skill_tree.first_node",
                "まずはこのノードを解放して、新しいキルコードを手に入れましょう。"),
            new ScreenGuideStep(
                "BackButton",
                "ui.screen_guide.skill_tree.next",
                "解放したキルコードは、改造で装備できます。"),
        };

        /// <summary> 改造画面の案内です。付け替えは促すだけで、必須にはしません。 </summary>
        private static readonly ScreenGuideStep[] SKILL_BUILD_STEPS =
        {
            new ScreenGuideStep(
                "Formation",
                "ui.screen_guide.skill_build.formation",
                "ここに、戦闘に持ち込むキルコードが並びます。"),
            new ScreenGuideStep(
                "SkillListPanel",
                "ui.screen_guide.skill_build.list",
                "持っているキルコードから選んで、付け替えられます。"),
            new ScreenGuideStep(
                "SkillBuildSaveButton",
                "ui.screen_guide.skill_build.save",
                "付け替えたら、保存して確定しましょう。"),
        };

        /// <summary> 作戦画面の案内です。 </summary>
        private static readonly ScreenGuideStep[] STAGE_SELECT_STEPS =
        {
            new ScreenGuideStep(
                "StageNodeRoot",
                "ui.screen_guide.stage_select.unlock",
                "ステージをクリアすると、次のステージが解放されます。"),
            new ScreenGuideStep(
                "StageNodeRoot",
                "ui.screen_guide.stage_select.reward",
                "ステージを初めてクリアすると、解放ポイントなどの報酬がもらえます。選んだステージの報酬はここで確かめられます。"),
        };

        private readonly SaveData _saveData;
        private readonly SkillTreeScreenView _skillTreeScreenView;
        private readonly SkillBuildScreenView _skillBuildScreenView;
        private readonly StageSelectScreenView _stageSelectScreenView;
        private readonly CancellationToken _cancellationToken;
        /// <summary> 表示中の案内のオーバーレイです。 </summary>
        private TutorialOverlayView _activeOverlayView;
        private bool _isRunning;
        private bool _isDisposed;

        /// <summary>
        ///     研究画面の表示完了を受け、初回なら案内を出します。
        /// </summary>
        private void HandleSkillTreeShowCompleted()
        {
            TryRunGuide(_skillTreeScreenView, ScreenGuideKind.SkillTree, SKILL_TREE_STEPS);
        }

        /// <summary>
        ///     改造画面の表示完了を受け、初回なら案内を出します。
        /// </summary>
        private void HandleSkillBuildShowCompleted()
        {
            TryRunGuide(_skillBuildScreenView, ScreenGuideKind.SkillBuild, SKILL_BUILD_STEPS);
        }

        /// <summary>
        ///     作戦画面の表示完了を受け、初回なら案内を出します。
        /// </summary>
        private void HandleStageSelectShowCompleted()
        {
            TryRunGuide(_stageSelectScreenView, ScreenGuideKind.StageSelect, STAGE_SELECT_STEPS);
        }

        /// <summary>
        ///     ホームチュートリアルを終えていて、その画面の案内がまだなら案内を始めます。
        /// </summary>
        /// <param name="screenView"> 案内する画面です。 </param>
        /// <param name="kind"> 案内の種類です。 </param>
        /// <param name="steps"> 案内の手順です。 </param>
        private async void TryRunGuide(ScreenViewBase screenView, ScreenGuideKind kind, ScreenGuideStep[] steps)
        {
            TutorialData tutorialData = _saveData.Tutorial;
            if (_isDisposed
                || _isRunning
                || !tutorialData.IsTutorialCompleted
                || !tutorialData.HasPendingScreenGuide(kind))
            {
                return;
            }

            _isRunning = true;
            try
            {
                await RunGuideAsync(screenView, steps, _cancellationToken);

                // 最後まで出したときだけ、案内を出し終えたことを保存する。
                if (!_isDisposed && tutorialData.CompleteScreenGuide(kind))
                {
                    await SaveStore.SaveAsync<SaveData>(_cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                _isRunning = false;
            }
        }

        /// <summary>
        ///     画面の要素を順番にハイライトし、決定入力ごとに次へ進みます。
        /// </summary>
        /// <param name="screenView"> 案内する画面です。 </param>
        /// <param name="steps"> 案内の手順です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        private async Task RunGuideAsync(
            ScreenViewBase screenView, ScreenGuideStep[] steps, CancellationToken cancellationToken)
        {
            VisualElement overlayParent = screenView.OutGameRootElement;
            if (overlayParent == null)
            {
                Debug.LogWarning($"[{nameof(OutGameScreenGuideRunner)}] 案内を配置する要素が見つかりませんでした。");
                return;
            }

            TutorialOverlayView overlayView = new TutorialOverlayView(overlayParent);
            _activeOverlayView = overlayView;
            try
            {
                foreach (ScreenGuideStep step in steps)
                {
                    // 対象が見つからない手順は飛ばす。
                    VisualElement target = screenView.FindTutorialTarget(step.TargetElementName);
                    if (target == null)
                    {
                        Debug.LogWarning(
                            $"[{nameof(OutGameScreenGuideRunner)}] 案内の対象 {step.TargetElementName} が見つかりませんでした。");
                        continue;
                    }

                    await overlayView.ShowStepAsync(
                        target, step.MessageEntry, step.FallbackMessage, cancellationToken);
                }

                await overlayView.HideAsync(cancellationToken);
            }
            finally
            {
                if (_activeOverlayView == overlayView)
                {
                    _activeOverlayView = null;
                }

                overlayView.Dispose();
                screenView.RestoreFocus();
            }
        }

        /// <summary>
        ///     画面の案内の1手順を表します。
        /// </summary>
        private readonly struct ScreenGuideStep
        {
            /// <summary>
            ///     画面の案内の1手順を初期化します。
            /// </summary>
            /// <param name="targetElementName"> ハイライトする要素の名前です。 </param>
            /// <param name="messageEntry"> 表示する説明文のUICommonのキーです。 </param>
            /// <param name="fallbackMessage"> 翻訳を取得できない場合の説明文です。 </param>
            public ScreenGuideStep(string targetElementName, string messageEntry, string fallbackMessage)
            {
                TargetElementName = targetElementName;
                MessageEntry = messageEntry;
                FallbackMessage = fallbackMessage;
            }

            /// <summary> ハイライトする要素の名前を取得します。 </summary>
            public string TargetElementName { get; }

            /// <summary> 表示する説明文のUICommonのキーを取得します。 </summary>
            public string MessageEntry { get; }

            /// <summary> 翻訳を取得できない場合の説明文を取得します。 </summary>
            public string FallbackMessage { get; }
        }
    }
}
