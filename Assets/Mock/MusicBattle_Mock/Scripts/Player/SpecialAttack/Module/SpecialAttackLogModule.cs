using UnityEngine;

namespace Mock.MusicBattle.Player
{
    public class SpecialAttackLogModule : ISpecialAttackModule
    {
        public void Execute()
        {
            string log = _text;

            if (string.IsNullOrEmpty(log))
            {
                log = "empty";
            }

            Debug.Log(log);
        }

        [SerializeField]
        private string _text;
    }
}
