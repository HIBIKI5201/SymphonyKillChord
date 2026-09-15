using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Common
{
    /// <summary>
    ///     ボタンの演出を要素へ付与するための拡張メソッド群です。
    /// </summary>
    public static class ButtonAnimationExtensions
    {
        /// <summary>
        ///     要素へ hover/focus 中の連続パルス演出を付与します。
        ///     <para>
        ///         USS クラス <c>btn-scale-feedback</c> と組み合わせて使うことを想定しています。
        ///     </para>
        /// </summary>
        /// <param name="element"> 対象の要素です。 </param>
        /// <returns> 呼び出し側で保持し、不要になったら <see cref="VisualElement.RemoveManipulator"/> へ渡すためのマニピュレータです。 </returns>
        /// <exception cref="ArgumentNullException"> elementがnullの場合にスローされます。 </exception>
        public static IManipulator EnableButtonPulseAnimation(this VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var manipulator = new ButtonPulseAnimationManipulator();
            element.AddManipulator(manipulator);
            return manipulator;
        }
    }
}
