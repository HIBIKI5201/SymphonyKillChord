using KillChord.Runtime.View.InGame.Sequence;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    public class InGamePlayDirector : MonoBehaviour, IGameplayControllable
    {
        public void StartGameplay()
        {
            CacheControllables();

            foreach (var controllable in _gameplayControllables)
            {
                controllable?.StartGameplay();
            }
        }

        public void StopGameplay()
        {
            CacheControllables();

            foreach (var controllable in _gameplayControllables)
            {
                controllable?.StopGameplay();
            }
        }

        [SerializeField, Header("ゲーム開始と終了の演出に関わるIGameplayControllableを実装したMonoBehaviourリスト")]
        private MonoBehaviour[] _gamePlayControllableObjects;

        private readonly List<IGameplayControllable> _gameplayControllables = new();
        private bool _cached;

        /// <summary>
        ///     シリアライズされた配列から
        ///     IGameplayControllableを実装しているものを抽出してリストに追加する。
        /// </summary>
        private void CacheControllables()
        {
            if (_cached)
            {
                return;
            }

            _gameplayControllables.Clear();

            // _gamePlayControllableObjectsから
            // IGameplayControllableを実装しているものを抽出して_gameplayControllablesに追加する。
            foreach (var mono in _gamePlayControllableObjects)
            {
                if (mono is IGameplayControllable controllable)
                {
                    _gameplayControllables.Add(controllable);
                }
                else
                {
                    Debug.LogWarning($"[InGamePlayDirector] {mono.name} は IGameplayControllable を実装していません。");
                }
            }
            _cached = true;
        }
    }
}
