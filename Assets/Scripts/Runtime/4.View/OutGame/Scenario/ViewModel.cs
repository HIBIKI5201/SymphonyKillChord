using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Adaptor;
namespace KillChord.Runtime.View
{
    public class ViewModel : IOutPutPort
    {
        public async ValueTask ShowTextAsync(string message, CancellationToken ct)
        {
            OnChat?.Invoke(message);
        }

        public async ValueTask FadeAsync(float start, float end, float duration, CancellationToken ct)
        {
            OnFade?.Invoke(start, end, duration);
        }

        public Action<string> OnChat;
        public Action<float, float, float> OnFade;

    }
}
