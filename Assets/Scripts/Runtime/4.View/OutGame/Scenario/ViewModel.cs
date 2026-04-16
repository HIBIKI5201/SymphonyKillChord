using System;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Adaptor;
namespace KillChord.Runtime.View
{
    public class ViewModel : IOutPutPort
    {
        public async ValueTask ShowTextAsync(CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(1f), ct);
            Debug.Log("ViewModel Complete");
        }

    }
}
