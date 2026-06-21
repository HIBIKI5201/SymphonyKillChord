using System;
namespace KillChord.Runtime.Domain.InGame.Buff
{
    public interface IBuffSystem:IDisposable
    {
        BuffContext Execute(BuffContext context, BuffExecuteTiming state);
        void Add(IBuff buff);
    }
}
