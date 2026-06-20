using System.Collections.Generic;

namespace DevelopProducts.Pause
{
    /// <summary>
    ///     オブジェクト一つ分のスケール管理を行うクラス
    /// </summary>
    public class TimeScaleHandler
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="baseScale"></param>
        public TimeScaleHandler(float baseScale)
        {
            _baseScale = baseScale;
            _scaleStack.Push(baseScale);
        }
        /// <summary>
        ///     必ずポーズ時で呼び出してください
        ///     
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public PauseToken Push(float scale)
        {
            _scaleStack.Push(scale);
            return new PauseToken(() => Pop());
        }
        private void Pop()
        {
            if (_scaleStack.Count > 0)
            {
                _scaleStack.Pop();
            }
        }
        public float CurrentScale => _scaleStack.Count > 0 ? _scaleStack.Peek() : _baseScale;
        private float _baseScale;
        private readonly Stack<float> _scaleStack = new();
    }
}
