using KillChord.Runtime.Adaptor.OutGame.Sortie;
using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Sortie
{
    /// <summary>
    ///     アウトゲームの出撃機能の初期化を行うクラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class OutGameSortieInitializer : MonoBehaviour
    {
        private void Awake()
        {
            if (!ServiceLocator.TryGetInstance<OutGameUIEvent>(out var outGameUIEvent))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(OutGameSortieInitializer)}] OutGameUIEvent が取得できませんでした。", this);
#endif
                return;
            }

            if (!ServiceLocator.TryGetInstance(out InputComposition inputComposition))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(OutGameSortieInitializer)}] InputComposition が取得できませんでした。", this);
#endif
                return;
            }

            if (!ServiceLocator.TryGetInstance(
                out SceneTransitionUsecase sceneTransitionUseCase))
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(OutGameSortieInitializer)}] " +
                    $"{nameof(SceneTransitionUsecase)}が取得できませんでした。",
                    this);
#endif
                return;
            }

            IOutGameSortieOutputPort outputPort =
                new OutGameSortieOutputPort(
                    outGameUIEvent,
                    inputComposition);
            OutGameSortieUseCase useCase =
                new OutGameSortieUseCase(sceneTransitionUseCase, outputPort);

            OutGameSortieController controller = new OutGameSortieController(useCase);

            // ステージ詳細画面の出撃ボタンを押した時の処理を外部で呼び出せるようにするため、ServiceLocatorに登録しておく。
            ServiceLocator.RegisterInstance(controller);
        }

        private void OnDestroy()
        {
            // 登録したコントローラーをServiceLocatorから登録解除する。
            ServiceLocator.UnregisterInstance<OutGameSortieController>();
        }
    }
}
