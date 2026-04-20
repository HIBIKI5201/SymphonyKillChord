using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Adaptor;
namespace KillChord.Runtime.View
{
    public class ViewModel : IOutPutPort
    {
        public ValueTask ShowTextAsync(string message, CancellationToken ct)
        {
            OnChat?.Invoke(message);
            return default;
        }

        public ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct)
        {
            OnFade?.Invoke(start, end, duration);
            return default;
        }

        public ValueTask ShowBackgroundAsync(string backgroundId, CancellationToken ct)
        {
            OnBackground?.Invoke(backgroundId);
            return default;
        }

        public Action<string> OnChat;
        public Action<float, float, float> OnFade;
        public Action<string> OnBackground;

    }
}
