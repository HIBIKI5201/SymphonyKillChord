using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.View.OutGame.Screen;

namespace KillChord.Runtime.Composition.OutGame.Sortie
{
    /// <summary>
    ///     出撃ユースケースの出力ポートの実装クラス。
    ///     Application と View に依存しているため Composition に配置している。
    /// </summary>
    public sealed class OutGameSortieOutputPort : IOutGameSortieOutputPort
    {
        /// <summary>
        ///    OutGameSortieOutputPort を初期化します。
        /// </summary>
        /// <param name="outGameUIEvent"> OutGameUIEvent のインスタンス。 </param>
        public OutGameSortieOutputPort(OutGameUIEvent outGameUIEvent)
        {
            _outGameUIEvent = outGameUIEvent;
        }

        public void ShowBattlePreparationScreen(string targetSceneName)
        {
            _outGameUIEvent.OnShownBattlePreparationScreen?.Invoke(targetSceneName);
        }

        private readonly OutGameUIEvent _outGameUIEvent;
    }
}
