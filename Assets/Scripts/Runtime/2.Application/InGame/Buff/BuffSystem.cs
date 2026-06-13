using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Skill;
using UnityEngine.Assertions.Must;

namespace KillChord.Runtime.Application.InGame.Buff
{
    public class BuffSystem : IBuffSystem
    {
        public BuffContext Execute(BuffContext context, BuffExecuteTiming state)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                IBuff buff = _list[i];

                if (buff.GetState().GetActivationType() != state)
                    continue;
                if (buff.GetState().GetTimingType() == BuffActivationType.Duration)
                {
                    _ = buff.ExecuteAsync(context); 
                    continue;
                }

                context = buff.Execute(context);
            }

            _list.Clear();
            return context;
        }

        public void Add(IBuff buff)
        {
            _list.Add(buff);
        }


        private List<IBuff> _list = new();
    }

}
