using System;
using System.Threading.Tasks;
using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Application.InGame.Buff;
using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Character;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class BuffSystemInit : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        async void Start()
        {
            CharacterEntity attacker = CharacterFactory.Create(characterData);
            CharacterEntity target = CharacterFactory.Create(characterData);
            AttackResult attackResult = new AttackResult(finalDamage, false);
            BuffContext context = new BuffContext(attacker, target, attackResult);
            buffSystem.Add(buff);
            Debug.Log(attacker.BaseDamage.Value);
            BuffContext result = buffSystem.Execute(context, BuffExecuteTiming.Attack_Logic_Before);
            Debug.Log(attacker.BaseDamage.Value);
            Debug.Log(result.AttackResult.FinalDamage.Value);

            await Task.Delay(TimeSpan.FromSeconds(5f));
            Debug.Log(attacker.BaseDamage.Value);
        }

        IBuff buff = new BuffTest();
        BuffSystem buffSystem = new();
        Damage finalDamage = new Damage(10);
        [SerializeField]
        CharacterData characterData;
    }
}
