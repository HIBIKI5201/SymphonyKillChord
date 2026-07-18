using KillChord.Runtime.View.InGame.Character;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Stage
{
    /// <summary>
    ///     GameObjectの有効状態を切り替えるワンショット演出です。
    /// </summary>
    [Serializable]
    public sealed class GameObjectActivationOneShotVisualEffect : IOneShotVisualEffect
    {
        /// <summary>
        ///     GameObjectを指定状態へ切り替えます。
        /// </summary>
        /// <param name="cueName"> 使用しません。 </param>
        public void Play(string cueName)
        {
            _target?.SetActive(_isActiveOnPlay);
        }

        /// <summary>
        ///     GameObjectを再生時と逆の状態へ戻します。
        /// </summary>
        public void Stop()
        {
            _target?.SetActive(!_isActiveOnPlay);
        }

        [SerializeField, Tooltip("有効状態を切り替えるGameObjectです。")]
        private GameObject _target;

        [SerializeField, Tooltip("再生時に設定する有効状態です。")]
        private bool _isActiveOnPlay = true;
    }
}
