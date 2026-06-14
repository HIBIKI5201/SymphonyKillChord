
using System.Collections.Generic;

using System.Threading;

using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Skill;


namespace KillChord.Runtime.Application.InGame.Buff
{
    public class BuffSystem : IBuffSystem
    {
        public BuffContext Execute(BuffContext context, BuffExecuteTiming state)
        {
            for (int i = _list.Count - 1; i >= 0; i--)
            {
                IBuff buff = _list[i];
                if (buff.GetState().GetActivationType() != state)
                    continue;

                buff.ExecuteAsync(context, _source.Token);
                context = buff.ExecuteInstance(context);
                _list.RemoveAt(i);
            }

            return context;
        }
        public void Add(IBuff buff)
        {
            _list.Add(buff);
        }

        public void Dispose()
        {
            _source.Cancel();
            _source = new();
        }

        private CancellationTokenSource _source = new();
        private List<IBuff> _list = new();
    }

}
