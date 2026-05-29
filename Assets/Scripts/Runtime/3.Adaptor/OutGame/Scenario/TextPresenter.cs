using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// Text の表示要求を View 側へ橋渡しする。
    /// </summary>
    public sealed class TextPresenter : ITextOutputPort
    {
        /// <summary>
        /// テキスト表示の出力先を受け取る。
        /// </summary>
        public TextPresenter(ITextViewSink viewSink)
        {
            _viewSink = viewSink ?? throw new System.ArgumentNullException(nameof(viewSink));
        }

        /// <summary>
        /// テキスト表示要求をビューへ通知する。
        /// </summary>
        public ValueTask ShowTextAsync(string message, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _viewSink.SetText(message);
            return default;
        }

        private readonly ITextViewSink _viewSink;
    }
}