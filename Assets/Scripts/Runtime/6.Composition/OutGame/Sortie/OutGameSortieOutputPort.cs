using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Input;

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
        /// <param name="inputComposition"> InputComposition のインスタンス。 </param>
        public OutGameSortieOutputPort(
            OutGameUIEvent outGameUIEvent,
            InputComposition inputComposition)
        {
            _outGameUIEvent = outGameUIEvent;
            _inputComposition = inputComposition;
        }

        /// <summary>
        ///     戦闘準備画面の表示を要求します。
        /// </summary>
        public void ShowBattlePreparationScreen()
        {
            _outGameUIEvent.OnShownBattlePreparationScreen?.Invoke();
        }

        /// <summary>
        ///     バトル開始イベントを通知します。
        /// </summary>
        public void StartBattle()
        {
            _outGameUIEvent.OnStartGame?.Invoke();
        }

        /// <summary>
        ///     ホーム画面の表示を要求します。
        /// </summary>
        public void ShowHomeScreen()
        {
            _outGameUIEvent.OnShownHomeScreen?.Invoke();
        }

        /// <summary>
        ///     シナリオ再生状態に合わせてOutGameの表示と入力を切り替えます。
        /// </summary>
        /// <param name="isActive"> OutGameを有効にする場合はtrueです。 </param>
        public void SetOutGameActiveForScenario(bool isActive)
        {
            _outGameUIEvent.OnOutGameUiVisibilityChanged?.Invoke(isActive);

            if (_inputComposition == null)
            {
                return;
            }

            if (isActive)
            {
                _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.OutGame);
                return;
            }

            _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.Scenario);
        }

        private readonly OutGameUIEvent _outGameUIEvent;
        private readonly InputComposition _inputComposition;
    }
}
