using UnityEngine;
using System;

namespace KillChord.Runtime.Adaptor.InGame.StageSelect
{
    public class SelectedBattleStageState
    {
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

        public bool HasSelectedBattleStage { get; private set; }

        public void SelectBattleStage(string battleStageName)
        {
            if(string.IsNullOrWhiteSpace(battleStageName))
            {
                throw new ArgumentException("バトルステージのシーン名はnullまたは空であってはなりません",nameof(battleStageName));
            }

            _currentBattleStageName = battleStageName;
            HasSelectedBattleStage = true;
        }

        public void Clear()
        {
            _currentBattleStageName = string.Empty;
            HasSelectedBattleStage = false;
        }

        private string _currentBattleStageName = string.Empty;
    }
}
