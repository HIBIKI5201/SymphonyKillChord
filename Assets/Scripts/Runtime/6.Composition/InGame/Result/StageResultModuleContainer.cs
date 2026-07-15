using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.View.InGame.Result;

namespace KillChord.Runtime.Composition.InGame.Result
{
    /// <summary>
    ///     ステージリザルトモジュールの公開物を保持するContainerです。
    /// </summary>
    public sealed class StageResultModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="view"> リザルトViewです。 </param>
        public StageResultModuleContainer(StageResultView view)
        {
            View = view;
        }

        /// <summary> リザルトViewです。 </summary>
        public StageResultView View { get; }

        /// <summary> リザルトPresenterです。 </summary>
        public StageResultPresenter Presenter { get; set; }

        /// <summary> リザルトControllerです。 </summary>
        public StageResultController Controller { get; set; }
    }
}
