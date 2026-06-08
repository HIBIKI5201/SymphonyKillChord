using UnityEngine;
using System;

namespace KillChord.Runtime.Adaptor.InGame.StageSelect
{
    /// <summary>
    ///     選択されているバトルステージの状態を管理するクラス。
    /// </summary>
    public class SelectedBattleStageState
    {
        /// <summary> 現在選択されているステージの名前。 </summary>
        public string CurrentBattleStageName
        {
            get
            {
                if(!HasSelectedBattleStage)
                {
                    throw new InvalidOperationException("バトルシーンが選択されていません"); 
                }

                return _currentBattleStageName;
            }
        }

        /// <summary> バトルステージが選択されているか。 </summary>
        public bool HasSelectedBattleStage { get; private set; }

        /// <summary>
        ///     バトルするステージを選択する。
        /// </summary>
        /// <param name="battleStageName"> ステージシーン名。 </param>
        /// <exception cref="ArgumentException"></exception>
        public void SelectBattleStage(string battleStageName)
        {
            if(string.IsNullOrWhiteSpace(battleStageName))
            {
                throw new ArgumentException("バトルステージのシーン名はnullまたは空であってはなりません",nameof(battleStageName));
            }

            _currentBattleStageName = battleStageName;
            HasSelectedBattleStage = true;
        }

        /// <summary>
        ///     選択状態をクリアする。
        /// </summary>
        public void Clear()
        {
            _currentBattleStageName = string.Empty;
            HasSelectedBattleStage = false;
        }

        private string _currentBattleStageName = string.Empty;
    }
}
