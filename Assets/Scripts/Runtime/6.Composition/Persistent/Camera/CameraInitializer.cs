using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Camera
{
    /// <summary>
    ///     カメラの初期化を担当するクラス。
    ///     ServiceLocator に <see cref="ICameraTransform"/> を登録する。
    /// </summary>
    public class CameraInitializer : MonoBehaviour, ICameraTransform
    {
        /// <summary>
        ///     ICameraTransform の Transform プロパティ。
        ///     MonoBehaviour の transform を返す。  
        /// </summary>
        public Transform Transform => transform;

        /// <summary>
        ///     起動時にServiceLocatorへインスタンスを登録する。
        /// </summary>
        private void Awake()
        {
            ServiceLocator.RegisterInstance<ICameraTransform>(this);
        }
    }

    /// <summary>
    ///     カメラのTransformを提供するインターフェース。
    /// </summary>
    public interface ICameraTransform
    {
        /// <summary> カメラのTransform。 </summary>
        Transform Transform { get; }
    }
}