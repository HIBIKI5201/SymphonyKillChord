using KillChord.Runtime.Domain.InGame.Camera;
using UnityEngine;

#if UNITY_EDITOR

namespace KillChord.Runtime.Composition.InGame.Debugger
{
    /// <summary>
    ///     カメラシステムのパラメータをデバッグ表示するためのクラス。
    /// </summary>
    public sealed class CameraSystemParameterDebug : MonoBehaviour
    {
        /// <summary>
        ///     デバッグ表示対象のカメラシステムパラメータを設定する。
        /// </summary>
        /// <param name="parameter"> 表示対象のカメラシステムパラメータ。</param>
        public void SetCameraParameter(CameraSystemParameter parameter)
        {
            _parameter = parameter;
        }

        [SerializeField, Tooltip("デバッグ表示対象のカメラシステムパラメータ。")]
        private CameraSystemParameter _parameter;
    }
}

#endif