namespace KillChord.Runtime.View
{
    /// <summary>
    ///     キャラクターアニメーションの内部再生要求を表す。
    /// </summary>
    internal readonly struct CharacterAnimationRequest
    {
        /// <summary>
        ///     内部再生要求を初期化する。
        /// </summary>
        /// <param name="index"> 再生インデックス。 </param>
        /// <param name="shouldNotifyDodgeEnded"> 回避終了通知が必要ならtrue。 </param>
        public CharacterAnimationRequest(int index, bool shouldNotifyDodgeEnded)
        {
            Index = index;
            ShouldNotifyDodgeEnded = shouldNotifyDodgeEnded;
        }

        /// <summary> 再生インデックスです。 </summary>
        public int Index { get; }

        /// <summary> 回避終了通知が必要かどうかです。 </summary>
        public bool ShouldNotifyDodgeEnded { get; }
    }
}
