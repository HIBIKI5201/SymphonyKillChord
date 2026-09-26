//using System;
//using System.Threading.Tasks;
//using KillChord.Runtime.Adaptor.OutGame.Scenario;
//using KillChord.Runtime.Application.InGame.Buff;
//using KillChord.Runtime.Domain;
//using KillChord.Runtime.Domain.InGame.Battle;
//using KillChord.Runtime.Domain.InGame.Buff;
//using KillChord.Runtime.Domain.InGame.Character;
//using KillChord.Runtime.InfraStructure.InGame.Character;
//using UnityEngine;

//namespace KillChord.Runtime.Composition
//{
//    /// <summary>
//    ///     バフシステムの簡易動作確認を行う初期化クラス。
//    /// </summary>
//    public class BuffSystemInit : MonoBehaviour
//    {
//        /// <summary>
//        ///     動作確認用にバフを登録して実行する。
//        /// </summary>
//        private async void Start()
//        {
//            CharacterEntity attacker = CharacterFactory.Create(characterData);
//            CharacterEntity target = CharacterFactory.Create(characterData);
//            AttackResult attackResult = new AttackResult(_finalDamage, false);
//            BuffContext context = new BuffContext(attacker, target, attackResult);
//            _buffSystem.Add(_buff);
//            Debug.Log(attacker.BaseDamage.Value);
//            BuffContext result = _buffSystem.Execute(context, BuffExecuteTiming.Attack_Logic_Before);
//            Debug.Log(attacker.BaseDamage.Value);
//            Debug.Log(result.AttackResult.FinalDamage.Value);

//            await Task.Delay(TimeSpan.FromSeconds(5f));
//            Debug.Log(attacker.BaseDamage.Value);
//        }

//        private readonly IBuff _buff = new DamageDownBuff(new BuffMetaData(BuffExecuteTiming.Attack_Logic_Before));
//        private readonly BuffSystem _buffSystem = new();
//        private readonly Damage _finalDamage = new(10);

//        [SerializeField]
//        private CharacterDefinitionAsset characterData;
//    }
//}

