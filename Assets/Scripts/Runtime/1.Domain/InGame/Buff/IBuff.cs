using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Domain.InGame.Buff
{
    public interface IBuff
    {  
        /// <summary>
        ///     即時発動バフ
        /// </summary>
        /// <param name="context"> バフ対象 </param>
        /// <returns></returns>
        BuffContext ExecuteInstance(BuffContext context);
        /// <summary>
        ///     継続発動バフ
        /// </summary>
        /// <param name="context"> バフ対象 </param>
        /// <returns></returns>
        ValueTask ExecuteAsync(BuffContext context,CancellationToken token);
        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        BuffMetaData GetState();
    }

}
