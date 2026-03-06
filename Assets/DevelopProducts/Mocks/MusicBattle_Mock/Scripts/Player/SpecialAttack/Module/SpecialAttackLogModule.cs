using UnityEngine;

namespace Mock.MusicBattle.Player
{
    public class SpecialAttackLogModule : ISpecialAttackModule
    {
        public void Execute(SpecialAttackDTO dto)
        {
            string log = _text;

            if (string.IsNullOrEmpty(log))
            {
                log = "empty";
            }

            Debug.Log(log, dto.Player);
        }

        [SerializeField]
        private string _text;
    }
}
