namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     体力の現在値や最大値を管理するエンティティクラス。
    /// </summary>
    public class HealthEntity
    {
        public HealthEntity(float health)
        {
            CurrentHealth = new(health);
            MaxHealth = new(health);
        }

        public Health CurrentHealth { get; private set; }
        public readonly Health MaxHealth;

        public void ChangeHealth(Health value)
        {
            if ((float)value > (float)MaxHealth) { value = MaxHealth; }
            CurrentHealth = value;
        }
    }
}