using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    /// スキル発動の判定と実行を扱うユースケースクラス。
    /// </summary>
    public class SkillUsecase
    {
        /// <summary>
        /// コンストラクタ。必要なサービスを注入する。
        /// </summary>
        public SkillUsecase(ISkillTargetResolver targetResolver, CharacterEntity playerEntity)
        {
            _targetResolver = targetResolver;
            _playerEntity = playerEntity;
        }

        /// <summary>
        ///     スキルが発動可能な場合、発動する。
        /// </summary>
        public bool TryExecuteSkill(
            SkillDefinition skillDefinition, //対象スキル
            BeatType beatType) //入力の拍子種類
        {
            if (_targetResolver.TryGetCurrentTargetEntity(out var target))
            {
                SkillEffectContext context = new SkillEffectContext(target, _playerEntity, beatType,null); //スキル効果の実行に必要な情報をまとめたコンテキストを作成
                skillDefinition.Effect.Execute(context); //ターゲット情報を渡してスキル効果を実行
                _playerEntity.BuffSystem.Execute(new BuffContext(_playerEntity,target),BuffExecuteTiming.Skill);
                //TODO: プレイヤーの基礎攻撃力の取得。現在のビート数から判定できるかも。
            }
            else
            {
                return false; //ターゲットがいない場合はスキルを発動しない 
            }

            return true;
        }
        private readonly ISkillTargetResolver _targetResolver;
        private readonly CharacterEntity _playerEntity;
    }
}
