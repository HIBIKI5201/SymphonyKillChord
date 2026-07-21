using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Camera
{
    /// <summary>
    ///     カメラのTransformを提供するインターフェース。
    /// </summary>
    public interface ICameraTransform
    {
        /// <summary> カメラのTransform。 </summary>
        Transform Transform { get; }
    }
}
