using Mock.MusicBattle.MusicSync;
using SymphonyFrameWork.Attribute;
using System;
using UnityEditorInternal;
using UnityEngine;

namespace Mock.MusicBattle.Player
{
    [Serializable]
    public struct SpecialAttackData
    {
        #region プロパティ
        public RythemPatternData RythemPattern => _rythemPattern;
        public ISpecialAttackModule[] Modules => _modules;
        #endregion

        #region Publicメソッド
        public void Excute()
        {
            foreach (ISpecialAttackModule module in _modules)
            {
                module?.Execute();
            }
        }
        #endregion

        #region シリアライズフィールド
        [SerializeField]
        private RythemPatternData _rythemPattern;
        [SerializeReference, SubclassSelector]
        private ISpecialAttackModule[] _modules;
        #endregion

        #region デバッグ
        public void Assert(UnityEngine.Object context = null)
        {
            Debug.Assert(_rythemPattern != null, "リズムパターンがありません。", context);
            Debug.Assert(0 < _modules.Length, "モジュールが割り当てられていません。", context);
            for (int i = 0; i < _modules.Length; i++)
            {
                ISpecialAttackModule module = _modules[i];
                Debug.Assert(module != null, $"{i}番目のモジュールがnullです。",context);
            }
        }
        #endregion
    }
}
