namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     デコレータで連結されたクリア条件チェーンを探索するヘルパーです。
    ///     1つのステップに複数のデコレータ(Popup/WaveStart/PlayerBuff等)が連結されている場合、
    ///     最外層だけでなくチェーン全体から目的の型を見つけるために使います。
    /// </summary>
    public static class ClearConditionChain
    {
        /// <summary>
        ///     チェーン中に指定した型の条件が含まれているかどうかを判定します。
        /// </summary>
        /// <typeparam name="TCondition"> 検索する条件の型です。 </typeparam>
        /// <param name="condition"> 探索を開始する条件です。 </param>
        /// <returns> チェーン中に見つかった場合はtrueです。 </returns>
        public static bool Contains<TCondition>(IMissionClearCondition condition) where TCondition : class, IMissionClearCondition
        {
            return Find<TCondition>(condition) != null;
        }

        /// <summary>
        ///     チェーンをたどり、指定した型の条件を探します。
        /// </summary>
        /// <typeparam name="TCondition"> 検索する条件の型です。 </typeparam>
        /// <param name="condition"> 探索を開始する条件です。 </param>
        /// <returns> 見つかった条件。見つからない場合はnullです。 </returns>
        public static TCondition Find<TCondition>(IMissionClearCondition condition) where TCondition : class, IMissionClearCondition
        {
            IMissionClearCondition current = condition;
            while (current != null)
            {
                if (current is TCondition target)
                {
                    return target;
                }

                current = (current as IDecoratorClearCondition)?.InnerCondition;
            }

            return null;
        }
    }
}
