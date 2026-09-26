using Unity.Entities;

namespace KillChord.Runtime.View.InGame.Stage.Barrage
{
    /// <summary>
    ///     Timelineからタレットを指定するための数値IDです。
    /// </summary>
    /// <remarks> Baker時にDataIDのハッシュ値が焼き込まれます。 </remarks>
    public struct TurretId : IComponentData
    {
        /// <summary> タレットIDのCollectionKeyです。ハッシュ計算に使用します。 </summary>
        /// <remarks> シーン内で完結するため、SourceDataProviderへは登録しません。 </remarks>
        public const string COLLECTION_KEY = "BarrageTurret";

        /// <summary> DataIDから焼き込まれた数値IDです。 </summary>
        public int Value;
    }
}
