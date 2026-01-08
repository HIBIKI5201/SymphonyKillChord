using Mock.MusicBattle.MusicSync;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    [Serializable]
    public struct SpecialAttackData
    {
        public RythemPatternData RythemPattern => _rythemPattern;
        public ISpecialAttackModule[] Modules => _modules;

        [SerializeField]
        private RythemPatternData _rythemPattern;
        [SerializeReference, SubclassSelector]
        private ISpecialAttackModule[] _modules;
    }
}
