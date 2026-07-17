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
        /// <param name="baseDurationSeconds"> 基準時間上の再生時間です。 </param>
        /// <param name="enterBlendDurationSeconds"> 基準時間上の開始ブレンド時間です。 </param>
        /// <param name="exitBlendDurationSeconds"> 基準時間上の終了ブレンド時間です。 </param>
        /// <param name="shouldNotifyDodgeEnded"> 回避終了通知が必要ならtrue。 </param>
        /// <param name="canCancelByMovement"> 移動による再生キャンセルを許可するならtrue。 </param>
        public CharacterAnimationRequest(
            int index,
            float baseDurationSeconds,
            float enterBlendDurationSeconds,
            float exitBlendDurationSeconds,
            bool shouldNotifyDodgeEnded,
            bool canCancelByMovement)
        {
            Index = index;
            BaseDurationSeconds = baseDurationSeconds;
            EnterBlendDurationSeconds = enterBlendDurationSeconds;
            ExitBlendDurationSeconds = exitBlendDurationSeconds;
            ShouldNotifyDodgeEnded = shouldNotifyDodgeEnded;
            CanCancelByMovement = canCancelByMovement;
        }

        /// <summary> 再生インデックスです。 </summary>
        public int Index { get; }

        /// <summary> 基準時間上の再生時間です。 </summary>
        public float BaseDurationSeconds { get; }

        /// <summary> 基準時間上の開始ブレンド時間です。 </summary>
        public float EnterBlendDurationSeconds { get; }

        /// <summary> 基準時間上の終了ブレンド時間です。 </summary>
        public float ExitBlendDurationSeconds { get; }

        /// <summary> 回避終了通知が必要かどうかです。 </summary>
        public bool ShouldNotifyDodgeEnded { get; }

        /// <summary> 移動による再生キャンセルが可能かどうかです。 </summary>
        public bool CanCancelByMovement { get; }
    }
}
