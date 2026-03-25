namespace KillChord.Runtime.Adaptor
{
    public readonly ref struct IngameHudDTO
    {
        public IngameHudDTO(float healthRate)
        {
            HealthRate = healthRate;
        }

        public readonly float HealthRate;
    }
}