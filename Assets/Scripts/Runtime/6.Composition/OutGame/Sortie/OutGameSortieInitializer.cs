using KillChord.Runtime.Adaptor.OutGame.Sortie;
using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.InfraStructure.Persistent.SceneManagement;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Sortie
{
    public sealed class OutGameSortieInitializer : MonoBehaviour
    {
        private void Awake()
        {
            if (!ServiceLocator.TryGetInstance<OutGameUIEvent> (out var outGameUIEvent))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(OutGameSortieInitializer)}] OutGameUIEvent が取得できませんでした。", this);
                return;
#endif
            }

            ISceneTransitionService sceneTransitionService = new SceneTransitionService();

            IOutGameSortieOutputPort outGameOutputPort = 
                new OutGameSortieOutputPort(outGameUIEvent);
             OutGameSortieUseCase useCase = 
                new OutGameSortieUseCase(sceneTransitionService, outGameOutputPort);

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
