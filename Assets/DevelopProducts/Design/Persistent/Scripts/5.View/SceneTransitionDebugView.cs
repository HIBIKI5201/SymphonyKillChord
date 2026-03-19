using DevelopProducts.Persistent.Adaptor;
using UnityEngine;

namespace DevelopProducts.Persistent.View
{
    public class SceneTransitionDebugView : MonoBehaviour
    {
        public async void ChangeScene()
        {
            bool success = await _service.ChangeSceneAsync(
                _fromSceneName,
                _toSceneName,
                default);

            if (!success)
            {
                Debug.LogError($"シーン遷移失敗: {_fromSceneName} -> {_toSceneName}");
            }
            else
            {
                Debug.Log($"シーン遷移成功: {_fromSceneName} -> {_toSceneName}");
            }
        }

        [SerializeField] private string _fromSceneName;
        [SerializeField] private string _toSceneName;

        private ISceneTransitionService _service;

        private void Awake()
        {
            _service = new SceneTransitionService();
        }
    }
}
