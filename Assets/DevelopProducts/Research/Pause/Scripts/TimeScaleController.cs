using System.Collections.Generic;
using UnityEngine;

namespace DevelopProducts.Pause
{
    public class TimeScaleController : MonoBehaviour
    {
        #region All
        public void PauseAll()
        {
            _pauseAllToken ??= TimeScaleSystem.TimeScaleManager.PauseAll();
        }
        public void ModifyScaleAll(float scale)
        {
            TimeScaleSystem.TimeScaleManager.SelectScaleAll(scale: scale);
        }
        public void ResumeAll()
        {
            _pauseAllToken?.Dispose();
            _pauseAllToken = null;
        }
        #endregion

        #region Type
        public void PauseByType(TimeScaleType type)
        {
            if (_typeTokens.ContainsKey(type))
                return;   // すでにポーズ済みなら何もしない

            _typeTokens[type] = TimeScaleSystem.TimeScaleManager.PauseByType(type);
        }
        public void ModifyScaleByType(TimeScaleType type, float scale)
        {
            TimeScaleSystem.TimeScaleManager.SetScaleByType(type, scale);
        }

        public void ResumeScaleByType(TimeScaleType type)
        {
            if (_typeTokens.TryGetValue(type, out var token))
            {
                token.Dispose();
                _typeTokens.Remove(type);
            }
        }
        #endregion

        #region ID
        public void PauseById(int instanceId)
        {
            if (_idTokens.ContainsKey(instanceId))
                return;

            _idTokens[instanceId] = TimeScaleSystem.TimeScaleManager.PauseById(instanceId);
        }
        public void ModifyScaleById(int instanceId, float scale)
        {
            TimeScaleSystem.TimeScaleManager.SetScaleById(instanceId: instanceId, scale: scale);
        }
        public void ResumeScaleById(int instanceId)
        {
            if (_idTokens.TryGetValue(instanceId, out var pauseToken))
            {
                pauseToken.Dispose();
                _idTokens.Remove(instanceId);
            }
        }
        #endregion

        private void OnDestroy()
        {
            _pauseAllToken?.Dispose();
            foreach (var token in _typeTokens.Values)
                token.Dispose();
            foreach (var token in _idTokens.Values)
                token.Dispose();
        }
        private PauseToken _pauseAllToken = null;
        private readonly Dictionary<TimeScaleType, PauseToken> _typeTokens = new();
        private readonly Dictionary<int, PauseToken> _idTokens = new();
    }
}
