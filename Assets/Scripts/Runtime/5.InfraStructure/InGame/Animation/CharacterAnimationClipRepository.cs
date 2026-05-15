using KillChord.Runtime.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     CharacterAnimationStateをキーにしてAnimationClipを検索するリポジトリ。
    ///     ScriptableObjectのカタログからDictionaryを構築する。
    /// </summary>
    public class CharacterAnimationClipRepository : MonoBehaviour
    {
        /// <summary> カタログアセットからDictionaryを構築する。 </summary>
        /// <param name="catalog"> カタログアセット。 </param>
        public CharacterAnimationClipRepository(CharacterAnimationCatalogAsset catalog)
        {
            _map = new Dictionary<CharacterAnimationState, AnimationClip>();
            if (catalog == null)
            {
                return;
            }

            // カタログエントリをDictionaryに登録する
            foreach (var entry in catalog.Entries)
            {
                if (entry.Clip == null)
                {
                    Debug.LogWarning($"[CharacterAnimationClipRepository] {entry.State} のClipがnullです。スキップします。");
                    continue;
                }

                if (_map.ContainsKey(entry.State))
                {
                    Debug.LogWarning($"[CharacterAnimationClipRepository] {entry.State} が重複しています。上書きします。");
                }

                _map[entry.State] = entry.Clip;
            }
        }

        /// <summary> アニメーション状態からクリップを検索する。 </summary>
        /// <param name="state"> アニメーション状態。 </param>
        /// <param name="clip"> 取得したアニメーションクリップ。 </param>
        /// <returns> 取得成功かどうか。 </returns>
        public bool TryFindByState(CharacterAnimationState state, out AnimationClip clip)
            => _map.TryGetValue(state, out clip);

        /// <summary> アニメーション状態からクリップを検索する。 </summary>
        private readonly Dictionary<CharacterAnimationState, AnimationClip> _map;
    }
}
