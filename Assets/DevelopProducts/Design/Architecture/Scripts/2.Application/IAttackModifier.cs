using DevelopProducts.Architecture.Domain;

namespace DevelopProducts.Architecture.Application
{
    public interface IAttackModifier
    {
        public DamageContext Modify(DamageContext damage);
    }
}
