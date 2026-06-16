using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

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
        /// 入力と行動を記録し、発動可能なスキルがあれば効果の実行と演出の要求を行う。
        /// </summary>
        public bool TryExecuteSkill(
            SkillDefinition skillDefinition, //装備中のスキル群
            BattleActionType actionType,    //行動の種類
            BeatType beatType,  //入力された攻撃の種類
            float unscaledTime) //入力された攻撃のタイミング（ゲーム時間）
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
