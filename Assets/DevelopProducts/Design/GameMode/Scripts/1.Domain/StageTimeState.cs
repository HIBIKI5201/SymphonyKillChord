using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     ステージの経過時間を管理するクラス。
    /// </summary>
    public class StageTimeState
    {
        public float ElapsedTime => _elapsedTime;

        /// <summary> 経過時間を更新するメソッド。 </summary>
        public void UpdateTimer(float deltaTime)
        {
            _elapsedTime += deltaTime;
        }

        private float _elapsedTime;
    }
}
