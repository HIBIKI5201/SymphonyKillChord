using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     ホーム画面のトップバーに表示するポイントを取得するユースケース。
    /// </summary>
    public sealed class GetHomePointsUseCase
    {
        /// <summary>
        ///     現在の改造ポイント・解放ポイントを取得します。
        /// </summary>
        public async ValueTask<HomePoints> ExecuteAsync()
        {
            SaveData saveData = SaveStore.IsLoaded<SaveData>()
                ? SaveStore.Get<SaveData>()
                : await SaveStore.LoadAsync<SaveData>();

            return new HomePoints(
                saveData.SkillBuild.SkillLevelupPoint,
                saveData.SkillUnlock.ResearchPoint);
        }
    }

    /// <summary>
    ///     ホーム画面のトップバーに表示するポイントの組。
    /// </summary>
    public readonly struct HomePoints
    {
        /// <summary>
        ///     ポイントの組を初期化します。
        /// </summary>
        /// <param name="rebuildPoints"> 改造ポイント。 </param>
        /// <param name="unlockPoints"> 解放ポイント。 </param>
        public HomePoints(int rebuildPoints, int unlockPoints)
        {
            RebuildPoints = rebuildPoints;
            UnlockPoints = unlockPoints;
        }

        /// <summary> 改造ポイント。 </summary>
        public int RebuildPoints { get; }

        /// <summary> 解放ポイント。 </summary>
        public int UnlockPoints { get; }
    }
}
