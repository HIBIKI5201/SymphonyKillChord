using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキルの表示・入力チェックを仲介するコントローラクラス。
    /// </summary>
    public class SkillController
    {
        public SkillController(IMusicSyncService musicSyncService)
        {
            _musicSyncService = musicSyncService ?? throw new ArgumentNullException(nameof(musicSyncService));
        }

        /// <summary>スキルの発動に成功したとき、対応するアニメーションを再生するためのイベント。</summary>
        public event Action<string> OnSkillAnimationRequested;

        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="skillExecutionControllers"></param>
        public void Initialize(SkillExecutionController[] skillExecutionControllers)
        {
            _skillExecutionControllers = skillExecutionControllers;
        }

        /// <summary>
        ///   指定された行動と入力でスキルの発動判定を行い、発動した場合は実行する。
        /// </summary>
        public void TryExecuteSkill(BattleActionType actionType, BeatType beatType, float unscaledTime)
        {
            _musicSyncService.RegisterBattleActionHistory(actionType, beatType, unscaledTime);

            // 装備中のスキルごとに入力をチェックし、発動判定を行う
            for (int i = 0; i < _skillExecutionControllers.Length; i++)
            {
                SkillExecutionController exeController = _skillExecutionControllers[i];
                if(exeController.TryExecuteSkill(beatType, unscaledTime, actionType, out string animationKey))
                {
                    OnSkillAnimationRequested?.Invoke(animationKey);
                }
            }
        }

        // ...インスペクション用のフィールド（順序はコード規定に従う）
        private SkillExecutionController[] _skillExecutionControllers;
        private readonly IMusicSyncService _musicSyncService;
    }
}