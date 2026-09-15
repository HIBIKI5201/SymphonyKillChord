using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Common
{
    /// <summary>
    ///     ボタンの hover/focus 中に、拡大と縮小を途切れなく繰り返すパルス演出を行う Manipulator です。
    ///     <para>
    ///         USS の transition は「値が変わった瞬間から遷移し、そこで静止する」仕組みしか持たないため、
    ///         折り返し地点で止まらない連続ループは表現できない。そのため毎フレーム sin 波の計算結果を
    ///         直接 <see cref="VisualElement.style"/> に書き込むことで、途切れないパルスを作る。
    ///     </para>
    ///     <para>
    ///         「アニメーションの実装」に属するクラスで、見た目(画像等)には関与しない。
    ///         見た目は <c>Button.uss</c> の <c>btn-skin</c> クラスが担当する。
    ///     </para>
    /// </summary>
    public sealed class ButtonPulseAnimationManipulator : Manipulator
    {
        /// <summary> パルス1往復(縮小→拡大)にかかる時間(秒)です。 </summary>
        private const float PERIOD_SECONDS = 1.5f;

        /// <summary> パルスの最小スケールです。 </summary>
        private const float MIN_SCALE = 1.0f;

        /// <summary> パルスの最大スケールです。hover/focus に入った瞬間の初期値でもあります。 </summary>
        private const float MAX_SCALE = 1.1f;

        /// <summary>
        ///     パルス演出に必要なポインタ・フォーカスイベントのコールバックをターゲットへ登録します。
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<FocusInEvent>(OnFocusIn);
            target.RegisterCallback<FocusOutEvent>(OnFocusOut);
        }

        /// <summary>
        ///     登録したコールバックを解除し、パルスループも停止します。
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<FocusInEvent>(OnFocusIn);
            target.UnregisterCallback<FocusOutEvent>(OnFocusOut);

            StopPulse();
        }

        private IVisualElementScheduledItem _pulseSchedule;
        private float _pulseStartTimeSeconds;
        private bool _isHovered;
        private bool _isFocused;
        private bool _isPressed;

        /// <summary>
        ///     ポインタがボタンに乗った際に呼び出され、パルスを開始します。
        /// </summary>
        /// <param name="pointerEnterEvent"> ポインタ侵入イベントです。 </param>
        private void OnPointerEnter(PointerEnterEvent pointerEnterEvent)
        {
            _isHovered = true;
            StartPulseIfNeeded();
        }

        /// <summary>
        ///     ポインタがボタンから離れた際に呼び出され、hover/focus のどちらも無ければパルスを止めます。
        /// </summary>
        /// <param name="pointerLeaveEvent"> ポインタ離脱イベントです。 </param>
        private void OnPointerLeave(PointerLeaveEvent pointerLeaveEvent)
        {
            _isHovered = false;
            _isPressed = false;
            StopPulseIfNotNeeded();
        }

        /// <summary>
        ///     クリック(押下)を開始した際に呼び出され、パルスによる書き換えを止めて USS の :active 演出に委ねます。
        /// </summary>
        /// <param name="pointerDownEvent"> ポインタ押下イベントです。 </param>
        private void OnPointerDown(PointerDownEvent pointerDownEvent)
        {
            _isPressed = true;
            target.style.scale = StyleKeyword.Null;
        }

        /// <summary>
        ///     クリック(押下)を終えた際に呼び出され、hover/focus が続いていればパルスを再開します。
        /// </summary>
        /// <param name="pointerUpEvent"> ポインタ解放イベントです。 </param>
        private void OnPointerUp(PointerUpEvent pointerUpEvent)
        {
            _isPressed = false;
            StartPulseIfNeeded();
        }

        /// <summary>
        ///     フォーカスを得た際に呼び出され、パルスを開始します。
        /// </summary>
        /// <param name="focusInEvent"> フォーカス取得イベントです。 </param>
        private void OnFocusIn(FocusInEvent focusInEvent)
        {
            _isFocused = true;
            StartPulseIfNeeded();
        }

        /// <summary>
        ///     フォーカスを失った際に呼び出され、hover のどちらも無ければパルスを止めます。
        /// </summary>
        /// <param name="focusOutEvent"> フォーカス喪失イベントです。 </param>
        private void OnFocusOut(FocusOutEvent focusOutEvent)
        {
            _isFocused = false;
            StopPulseIfNotNeeded();
        }

        /// <summary>
        ///     hover または focus が有効で、かつ未開始であればパルスループを開始します。
        /// </summary>
        private void StartPulseIfNeeded()
        {
            if (_isPressed || (!_isHovered && !_isFocused) || _pulseSchedule != null)
            {
                return;
            }

            _pulseStartTimeSeconds = Time.unscaledTime;
            _pulseSchedule = target.schedule.Execute(UpdatePulse).Every(0);
        }

        /// <summary>
        ///     hover と focus のどちらも無効であればパルスループを停止します。
        /// </summary>
        private void StopPulseIfNotNeeded()
        {
            if (_isHovered || _isFocused)
            {
                return;
            }

            StopPulse();
        }

        /// <summary>
        ///     パルスループを停止し、scale を USS 側の値に戻します。
        /// </summary>
        private void StopPulse()
        {
            _pulseSchedule?.Pause();
            _pulseSchedule = null;
            target.style.scale = StyleKeyword.Null;
        }

        /// <summary>
        ///     経過時間から sin 波でスケール値を算出し、毎フレーム書き込みます。
        /// </summary>
        /// <param name="timerState"> スケジューラから渡されるタイマー情報です。 </param>
        private void UpdatePulse(TimerState timerState)
        {
            if (_isPressed)
            {
                return;
            }

            // cosを使うことで t=0(hover/focus開始直後) の値が MAX になり、そこから MIN まで縮小して再び MAX へ戻るループになる。
            float elapsedSeconds = Time.unscaledTime - _pulseStartTimeSeconds;
            float t = (elapsedSeconds % PERIOD_SECONDS) / PERIOD_SECONDS;
            float scale = MIN_SCALE + (MAX_SCALE - MIN_SCALE) * (0.5f + 0.5f * Mathf.Cos(2f * Mathf.PI * t));

            target.style.scale = new StyleScale(new Scale(new Vector3(scale, scale, 1f)));
        }
    }
}
