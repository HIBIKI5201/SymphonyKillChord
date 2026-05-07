namespace KillChord.Runtime.Domain.InGame.Enemy
{
    public readonly struct ShellAttackSpec
    {
        public ShellAttackSpec(float explosionRadius)
        {
            ExplosionRadius = explosionRadius;
        }
        public float ExplosionRadius { get; }
    }
}
