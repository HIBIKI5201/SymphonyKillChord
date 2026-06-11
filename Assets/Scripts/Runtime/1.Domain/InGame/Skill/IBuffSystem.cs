using System.Threading.Tasks;

namespace KillChord.Runtime.Domain.InGame.Skill
{
   public interface IBuffSystem
    {
        BuffContext Execute(BuffContext context, BuffExecuteType state);
        void Add(IBuff buff);
    }
}
