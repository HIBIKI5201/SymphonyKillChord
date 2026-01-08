using UnityEngine;

namespace Mock.MusicBattle.Player
{
    public class SpecialAttackLog : ISpecialAttackModule
    {
        public void Execute()
        {
            Debug.Log(_text);
        }

        [SerializeField]
        private string _text;
    }
}
