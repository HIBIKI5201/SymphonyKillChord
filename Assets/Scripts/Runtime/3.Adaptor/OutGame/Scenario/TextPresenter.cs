using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
{
    public sealed class TextPresenter : ITextOutputPort
    {
        public TextPresenter(ITextViewSink viewSink)
        {
            _viewSink = viewSink;
        }

        public ValueTask ShowTextAsync(string message, CancellationToken ct)
        {
            _viewSink.SetText(message);
            return default;
        }

        private readonly ITextViewSink _viewSink;
    }
}
