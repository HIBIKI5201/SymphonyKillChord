namespace KillChord.Runtime.View.InGame.Result
{
    public class StageResultMissionItemViewModel
    {
        public StageResultMissionItemViewModel(
            string descpription,
            bool isCompleted)
        {
            Description = descpription ?? string.Empty;
            IsCompleted = isCompleted;
        }

        public string Description { get; }

        public bool IsCompleted { get; }
    }
}
