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
        /// <summary>
        ///     コントローラーを初期化します。
        /// </summary>
        /// <param name="musicSyncService"> 音楽同期サービスです。 </param>
        public SkillController(IMusicSyncService musicSyncService)
        {
            _musicSyncService = musicSyncService ?? throw new ArgumentNullException(nameof(musicSyncService));
        }

        /// <summary> スキルの発動に成功したとき、対応するアニメーションを再生するためのイベント。 </summary>
        public event Action<string> OnSkillAnimationRequested;

        /// <summary> スキルの発動に成功したとき、対応するボイスを再生するためのイベント。 </summary>
        public event Action OnSkillVoiceRequested;

        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="skillExecutionControllers"> スキル実行Controller一覧です。 </param>
        public void Initialize(SkillExecutionController[] skillExecutionControllers)
        {
            _skillExecutionControllers = skillExecutionControllers;
        }

        /// <summary>
        ///   指定された行動と入力でスキルの発動判定を行い、発動した場合は実行する。
        /// </summary>
        /// <param name="actionType"> 行動種別です。 </param>
        /// <param name="beatType"> 現在ビートです。 </param>
        /// <param name="unscaledTime"> 現在時刻です。 </param>
        public void TryExecuteSkill(BattleActionType actionType, BeatType beatType, float unscaledTime)
        {
            _musicSyncService.RegisterBattleActionHistory(actionType, beatType);

            for (int i = 0; i < _skillExecutionControllers.Length; i++)
            {
                SkillExecutionResult result = _skillExecutionControllers[i].TryExecuteSkill(beatType, unscaledTime, actionType);
                if (result.ResultType == SkillExecutionResultType.Executed)
                {
                    OnSkillAnimationRequested?.Invoke(result.AnimationKey);
                    OnSkillVoiceRequested?.Invoke();
                }
            }
        }

        private SkillExecutionController[] _skillExecutionControllers;
        private readonly IMusicSyncService _musicSyncService;
    }
}
