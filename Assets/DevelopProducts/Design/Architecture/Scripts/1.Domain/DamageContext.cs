using Codice.Client.BaseCommands;
using UnityEngine;
using UnityEngine.Playables;

namespace DevelopProducts.Architecture.Domain
{
    public readonly struct DamageContext
    {
        public DamageContext(float value)
        {
            _value = value;
        }

        public float Value => _value;

        public static DamageContext operator +(DamageContext left, float right)
        {
            return new DamageContext(left.Value + right);
        }
        public static DamageContext operator -(DamageContext left, float right)
        {
            return new DamageContext(left.Value - right);
        }
        public static DamageContext operator *(DamageContext context, in float multiplier)
        {
            return new DamageContext(context.Value * multiplier);
        }
        public static DamageContext operator /(DamageContext context, in float divisor)
        {
            return new DamageContext(context.Value / divisor);
        }

        private readonly float _value;
    }
}
