using KillChord.Runtime.Domain;
using System.Collections.Generic;

namespace KillChord.Runtime.Composition
{
    /// <summary> CharacterAnimationStateをインデックスに変換するクラス。 </summary>    
    public sealed class CharacterAnimationPlaybackResolver
    {
        /// <summary> CharacterAnimationStateをインデックスに変換するための辞書を受け取って初期化する。 </summary>
        public CharacterAnimationPlaybackResolver(Dictionary<CharacterAnimationState, int> indices)
        {
            _indices = indices;
        }

        /// <summary> CharacterAnimationStateをインデックスに変換する。 </summary>
        public int GetIndex(CharacterAnimationState state)
        {
            return _indices[state];
        }

        private readonly Dictionary<CharacterAnimationState, int> _indices;
    }
}
