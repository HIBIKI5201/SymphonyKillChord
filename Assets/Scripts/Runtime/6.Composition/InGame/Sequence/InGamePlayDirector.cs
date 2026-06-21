using KillChord.Runtime.View.InGame.Sequence;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    /// <summary>
    ///    ゲームプレイの開始と終了の演出を制御するクラス。
    /// </summary>
    public class InGamePlayDirector : MonoBehaviour, IGameplayControllable
    {
        /// <summary>
        ///     ゲームプレイの開始演出を開始する。
        /// </summary>
        public void StartGameplay()
        {
            CacheControllables();

            foreach (var controllable in _gameplayControllables)
            {
                controllable?.StartGameplay();
            }
        }

        /// <summary>
        ///     ゲームプレイの終了演出を開始する。
        /// </summary>
        public void StopGameplay()
        {
            CacheControllables();

            foreach (var controllable in _gameplayControllables)
            {
                controllable?.StopGameplay();
            }
        }

        /// <summary>
        ///     ゲームプレイの開始と終了の演出に関わるIGameplayControllableを実装したMonoBehaviourを追加する。
        ///     これはステージシーンなどに配置されたIGameplayControllableを実装したMonoBehaviourをInGamePlayDirectorに紐づけるためのメソッドです。
        /// </summary>
        /// <param name="controllable">追加するIGameplayControllableを実装したMonoBehaviour</param>
        public void AddGamePlayControllable(IGameplayControllable controllable)
        {
            if (controllable == null)
            {
                Debug.LogWarning("[InGamePlayDirector] 追加しようとした IGameplayControllable が null です。");
                return;
            }

            CacheControllables();

            if (!_gameplayControllables.Contains(controllable))
            {
                _gameplayControllables.Add(controllable);
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
                if (mono == null)
                {
                    Debug.LogWarning("[InGamePlayDirector] 制御対象が未設定です。", this);
                    continue;
                }

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
