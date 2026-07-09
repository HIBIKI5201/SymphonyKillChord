using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;

namespace KillChord.Runtime.Composition.InGame.Bootstrap
{
    /// <summary>
    ///     シーン依存オブジェクトを公開するContainerです。
    /// </summary>
    public sealed class SceneDependencyModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="stageSceneInstance"> ステージシーン参照です。 </param>
        /// <param name="musicPlayer"> 音楽プレイヤーです。 </param>
        /// <param name="inputComposition"> 入力Compositionです。 </param>
        /// <param name="playerInitializer"> プレイヤー初期化クラスです。 </param>
        /// <param name="mainCamera"> メインカメラです。 </param>
        public SceneDependencyModuleContainer(
            IStageSceneInstance stageSceneInstance,
            MusicPlayer musicPlayer,
            InputComposition inputComposition,
            PlayerInitializer playerInitializer,
            UnityEngine.Camera mainCamera)
        {
            StageSceneInstance = stageSceneInstance;
            MusicPlayer = musicPlayer;
            InputComposition = inputComposition;
            PlayerInitializer = playerInitializer;
            MainCamera = mainCamera;
        }

        /// <summary> ステージシーン参照です。 </summary>
        public IStageSceneInstance StageSceneInstance { get; }

        /// <summary> 音楽プレイヤーです。 </summary>
        public MusicPlayer MusicPlayer { get; }

        /// <summary> 入力Compositionです。 </summary>
        public InputComposition InputComposition { get; }

        /// <summary> プレイヤー初期化クラスです。 </summary>
        public PlayerInitializer PlayerInitializer { get; }

        /// <summary> メインカメラです。 </summary>
        public UnityEngine.Camera MainCamera { get; }
    }
}
