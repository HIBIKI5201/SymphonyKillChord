using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Adaptor.InGame.Target
{
    /// <summary>
    ///     範囲クエリで検出した対象1件を表す構造体。
    /// </summary>
    public readonly struct TargetAreaHit
    {
        /// <summary>
        ///     検出結果を生成します。
        /// </summary>
        /// <param name="target"> 検出したターゲットViewModelです。 </param>
        /// <param name="entity"> 対応するCharacterEntityです。 </param>
        /// <param name="distance"> 原点からの水平距離です。 </param>
        public TargetAreaHit(ITargetableViewModel target, CharacterEntity entity, float distance)
        {
            Target = target;
            Entity = entity;
            Distance = distance;
        }

        /// <summary> 検出したターゲットViewModel。 </summary>
        public ITargetableViewModel Target { get; }

        /// <summary> 対応するCharacterEntity。 </summary>
        public CharacterEntity Entity { get; }

        /// <summary> 原点からの水平距離。 </summary>
        public float Distance { get; }
    }
}
