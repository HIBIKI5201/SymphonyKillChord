
using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Buff;


namespace KillChord.Runtime.Application.InGame.Buff
{
    public class BuffBase : IBuff
    {
        public BuffBase(BuffMetaData buffMetaData)
        {
            _buffMetaData =  buffMetaData;
        }
        public virtual BuffContext ExecuteInstance(BuffContext context)
        {
            return context;
        }

        public virtual async ValueTask ExecuteAsync(BuffContext context,CancellationToken token)
        {
        }

        public virtual BuffMetaData GetState()
        {
           return _buffMetaData;
        }

        private readonly BuffMetaData _buffMetaData;
    }
}
