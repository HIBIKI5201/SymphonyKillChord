using CriWare;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    public class SpecialAttackSoundModule : ISpecialAttackModule
    {
        public void Execute(SpecialAttackDTO dto)
        {
            CriAtomSource source = dto.Source;
            if (source == null) { return; }

            source.cueName = _queueName;
            source.Play();
        }

        [SerializeField, Tooltip("キュー名")]
        private string _queueName = string.Empty;

        #region デバッグ
        public void Assert(Object context)
        {
            Debug.Assert(_queueName != null, context);
        }
        #endregion
    }
}
