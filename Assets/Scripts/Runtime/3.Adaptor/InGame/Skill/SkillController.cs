using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキルの表示・入力チェックを仲介するコントローラクラス。
    /// </summary>
    public class SkillController : IDisposable
    {
        /// <summary>
        ///     コントローラーを初期化します。
        /// </summary>
        /// <param name="musicSyncService"> 音楽同期サービスです。 </param>
        /// <param name="getUnscaledTime"> スキル進捗表示に使うゲーム側の現在時刻を取得します。 </param>
        public SkillController(IMusicSyncService musicSyncService, Func<float> getUnscaledTime)
        {
            _musicSyncService = musicSyncService ?? throw new ArgumentNullException(nameof(musicSyncService));
            _getUnscaledTime = getUnscaledTime ?? throw new ArgumentNullException(nameof(getUnscaledTime));
        }

        /// <summary> スキルの発動に成功したとき、対応するアニメーションを再生するためのイベント。 </summary>
        public event Action<string> OnSkillAnimationRequested;

        /// <summary> スキルの発動に成功したとき、対応するボイスを再生するためのイベント。 </summary>
        public event Action OnSkillVoiceRequested;

        /// <summary> スキルで構える武器の表示を要求するイベントです。 </summary>
        public event Action<BeatType> OnSkillWeaponRequested;

        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="skillExecutionControllers"> スキル実行Controller一覧です。 </param>
        public void Initialize(SkillExecutionController[] skillExecutionControllers)
        {
            if (skillExecutionControllers == null)
            {
                throw new ArgumentNullException(nameof(skillExecutionControllers));
            }

            _musicSyncService.OnRhythmTimedOut -= HandleRhythmTimedOutHandler;
            _skillExecutionControllers = skillExecutionControllers;
            _musicSyncService.OnRhythmTimedOut += HandleRhythmTimedOutHandler;
        }

        /// <summary>
        ///     リズムタイムアウトの購読を解除します。
        /// </summary>
        public void Dispose()
        {
            _musicSyncService.OnRhythmTimedOut -= HandleRhythmTimedOutHandler;
            _skillExecutionControllers = Array.Empty<SkillExecutionController>();
        }

        /// <summary>
        ///     指定された行動と入力でスキルの発動判定を行い、発動した場合は実行する。
        /// </summary>
        /// <param name="actionType"> 行動種別です。 </param>
        /// <param name="beatType"> 現在ビートです。 </param>
        /// <param name="unscaledTime"> クールダウンやUI表示に使う、ゲーム側の現在時刻です。 </param>
        /// <param name="musicTime"> リズム入力履歴に記録する、音楽の再生時間です。 </param>
        /// <param name="isJustHit"> ジャスト入力によるスキル発動かどうか。 </param>
        /// <returns> スキル発動の結果、通常攻撃のダメージを適用するかどうかのポリシーです。 </returns>
        public SkillNormalAttackDamagePolicy TryExecuteSkill(BattleActionType actionType, BeatType beatType, float unscaledTime, float musicTime, bool isJustHit, bool canUseSkill)
        {
            _musicSyncService.RegisterBattleActionHistory(actionType, beatType);
            SkillNormalAttackDamagePolicy normalAttackDamagePolicy =
                SkillNormalAttackDamagePolicy.Apply;

            // スキル発動不可の場合、処理を終了する
            if (!canUseSkill)
            {
                return normalAttackDamagePolicy;
            }

            for (int i = 0; i < _skillExecutionControllers.Length; i++)
            {
                SkillExecutionResult result = _skillExecutionControllers[i].TryExecuteSkill(beatType, unscaledTime, musicTime, actionType, isJustHit);
                if (result.ResultType == SkillExecutionResultType.Executed)
                {
                    OnSkillAnimationRequested?.Invoke(result.AnimationKey);
                    OnSkillVoiceRequested?.Invoke();
                    OnSkillWeaponRequested?.Invoke(result.WeaponBeatType);

                    if (result.SkillNormalAttackDamagePolicy == SkillNormalAttackDamagePolicy.Skip)
                    {
                        normalAttackDamagePolicy = SkillNormalAttackDamagePolicy.Skip;
                    }
                }
            }

            return normalAttackDamagePolicy;
        }

        private SkillExecutionController[] _skillExecutionControllers;
        private readonly IMusicSyncService _musicSyncService;
        private readonly Func<float> _getUnscaledTime;

        /// <summary>
        ///     全スキルの入力系列を破棄し、進捗表示をリセットします。
        /// </summary>
        private void HandleRhythmTimedOutHandler()
        {
            float now = _getUnscaledTime();
            for (int i = 0; i < _skillExecutionControllers.Length; i++)
            {
                _skillExecutionControllers[i].ResetInputProgress(now);
            }
        }
    }
}
