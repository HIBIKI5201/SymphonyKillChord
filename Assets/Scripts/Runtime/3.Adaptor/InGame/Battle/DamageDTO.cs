using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Adaptor.InGame.Battle
{
    /// <summary>
    ///     ダメージ情報（最大体力と現在体力）を転送するための構造体。
    /// </summary>
    public readonly ref struct DamageDTO
    {
        public DamageDTO(Health max, Health current)
        {
            MaxHealth = max;
            CurrentHealth = current;
        }

        public readonly Health MaxHealth;
        public readonly Health CurrentHealth;
    }
}
