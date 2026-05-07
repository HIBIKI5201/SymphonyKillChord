using System.Threading;
using System.Threading.Tasks;
namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    /// IPortraitOutputPort の契約を定義します。
    /// </summary>
    public interface IPortraitOutputPort
    {
        /// <summary>
        /// 遶九■邨ｵ繧呈欠螳壹せ繝ｭ繝・ヨ縺ｸ陦ｨ遉ｺ繝ｻ譖ｴ譁ｰ縺励∪縺吶・        /// </summary>
        ValueTask ShowPortraitAsync(
            string slot,
            string assetKey,
            float positionX,
            float positionY,
            float scale,
            bool visible,
            CancellationToken ct);
    }
}
