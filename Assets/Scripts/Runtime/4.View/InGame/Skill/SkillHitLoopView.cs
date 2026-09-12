using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.View.InGame.Sequence;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     連撃スキルのヒット適用を定期更新するViewクラス。
    /// </summary>
    public sealed class SkillHitLoopView : MonoBehaviour, IGameplayControllable
    {
        /// <summary>
        ///     初期化処理を行います。
        /// </summary>
        /// <param name="skillHitController"> 連撃の進行を扱うControllerです。 </param>
        public void Initialize(SkillHitController skillHitController)
        {
            _skillHitController = skillHitController;
        }

        /// <summary> ゲームプレイを開始します。 </summary>
        public void StartGameplay() => _isPlaying = true;

        /// <summary> ゲームプレイを停止します。 </summary>
        public void StopGameplay() => _isPlaying = false;

        /// <summary>
        ///     経過時間をControllerへ伝えます。
        /// </summary>
        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            _skillHitController?.Tick(Time.deltaTime);
        }

        private SkillHitController _skillHitController;
        private bool _isPlaying;
    }
}
