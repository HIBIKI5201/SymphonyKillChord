namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     体力の現在値や最大値を管理するエンティティクラス。
    /// </summary>
    public class HealthEntity
    {
        /// <summary>
        ///     体力を初期化するコンストラクタ。
        /// </summary>
        /// <param name="health"></param>
        public HealthEntity(float health)
        {
            CurrentHealth = new(health);
            MaxHealth = new(health);
        }

        /// <summary> 現在のHPを取得する。 </summary>
        public Health CurrentHealth { get; private set; }

        /// <summary> 最大HPを取得する。 </summary>
        public readonly Health MaxHealth;

        /// <summary>
        ///     HPを変更する。
        /// </summary>
        /// <param name="value"></param>
        public void ChangeHealth(Health value)
        {
            if ((float)value > (float)MaxHealth) { value = MaxHealth; }
            CurrentHealth = value;
        }
    }
}