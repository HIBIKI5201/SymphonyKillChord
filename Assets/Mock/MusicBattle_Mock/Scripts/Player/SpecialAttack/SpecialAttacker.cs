using CriWare;
using Mock.MusicBattle.MusicSync;
using System.Threading;
using Unity.Plastic.Antlr3.Runtime;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    public class SpecialAttacker
    {
        public SpecialAttacker(
            GameObject player,
            PlayerStatus status,
            MusicSyncManager musicSyncManager,
            CriAtomSource source, 
            CancellationToken destroyCancellationToken) 
        { 
            _player = player;
            _status = status;
            _musicSyncManager = musicSyncManager;
            _source = source;
            _destroyCancellationToken = destroyCancellationToken;
        }

        public bool CheckPatternMatch(out int index)
        {
            for (int i = 0; i < _status.SpecialAttackDatas.Length; i++)
            {
                RythemPatternData data = _status.SpecialAttackDatas[i].RythemPattern;
                if (_musicSyncManager.IsMatchInputTimeSignature(data))
                {
                    Debug.Log($"MusicSync Signature Pattern Matched! Pattern: {string.Join(", ", data.SignaturePattern.ToArray())}");

                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public void Execute(int index)
        {
            SpecialAttackDTO dto = new(
                _player,
                _source,
                _destroyCancellationToken
                );

            _status.SpecialAttackDatas[index].Execute(dto);
        }

        private readonly GameObject _player;
        private readonly PlayerStatus _status;
        private readonly MusicSyncManager _musicSyncManager;
        private readonly CriAtomSource _source;
        private readonly CancellationToken _destroyCancellationToken;
    }
}
