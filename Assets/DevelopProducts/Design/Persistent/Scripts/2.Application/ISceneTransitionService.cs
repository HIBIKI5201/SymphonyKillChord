using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface ISceneTransitionService
{
    Task<bool> ChangeSceneAsync(string fromSceneName, string toSceneName, CancellationTokenSource cancellationTokenSource);
}
