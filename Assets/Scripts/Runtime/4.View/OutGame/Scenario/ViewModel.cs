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
            ChangeChat?.Invoke(message);
        }

        public Action<string> ChangeChat;

    }
}
