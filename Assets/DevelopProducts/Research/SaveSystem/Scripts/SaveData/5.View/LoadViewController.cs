using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Research.SaveSystem
{
    public class LoadViewController : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="loadGame"></param>
        /// <param name="saveLoadEvents"></param>
        public void Initialize(LoadGame loadGame, SaveLoadEvents saveLoadEvents)
        {
            _loadGame = loadGame;
            _saveLoadEvents = saveLoadEvents;
            _saveLoadEvents.OnLoadStart += ActivateShelter;
            _saveLoadEvents.OnLoadEnd += DeactivateShelter;
        }
        /// <summary>
        ///     ロードボタン押下時の処理。
        /// </summary>
        public void OnLoadButtonClick()
        {
            _loadGame.LoadGameAsync(SetupSaveData);
        }

        #region ライフサイクル
        private void OnDestroy()
        {
            if (_saveLoadEvents == null) return;
            _saveLoadEvents.OnLoadStart -= ActivateShelter;
            _saveLoadEvents.OnLoadEnd -= DeactivateShelter;
        }
        #endregion

        [SerializeField, Tooltip("")]
        private GameObject _shelter;
        [SerializeField, Tooltip("")]
        private Text _shelterText;
        [SerializeField, Tooltip("")]
        private InputField _inputGold;
        [SerializeField, Tooltip("")]
        private InputField _inputHpMax;
        [SerializeField, Tooltip("")]
        private InputField _inputAttack;
        [SerializeField, Tooltip("")]
        private InputField _inputCritRate;
        [SerializeField, Tooltip("")]
        private InputField _inputCritScale;
        [SerializeField, Tooltip("")]
        private InputField _inputEquipments;
        [SerializeField, Tooltip("")]
        private InputField _inputSkills;
        [SerializeField, Tooltip("")]
        private InputField _inputMissionProgress;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxMission;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxEquipment;
        [SerializeField, Tooltip("")]
        private Toggle[] _chkboxSkill;

        private LoadGame _loadGame;
        private SaveLoadEvents _saveLoadEvents;

        /// <summary>
        ///     ロードしたデータを画面に設定する。
        /// </summary>
        /// <param name="saveData"></param>
        private void SetupSaveData(KillChordGameData saveData)
        {
            // デバッグ用処理
            SetupPlayerStatus(saveData);
            SetupMissionProgress(saveData);
            SetupMissionUnlock(saveData);
            SetupEquipmentUnlock(saveData);
            SetupSkillUnlock(saveData);
        }

        #region デバッグ用
        private void SetupPlayerStatus(KillChordGameData saveData)
        {
            _inputGold.text = saveData.Gold.ToString();
            _inputHpMax.text = saveData.HpMax.ToString();
            _inputAttack.text = saveData.Attack.ToString();
            _inputCritRate.text = saveData.CritRate.ToString();
            _inputCritScale.text = saveData.CritScale.ToString();
            _inputEquipments.text = string.Join(',', saveData.Equipments);
            _inputSkills.text = string.Join(',', saveData.Skills);
        }

        private void SetupMissionProgress(KillChordGameData saveData)
        {
            List<int> progress = saveData.MissionProgress;
            _inputMissionProgress.text = progress.Count == 0 ? "0" : string.Join(',', progress.ToArray());
        }

        private void SetupMissionUnlock(KillChordGameData saveData)
        {
            List<int> missions = saveData.MissionUnlock;
            for (int i = 0; i < _chkboxMission.Length; i++)
            {
                int id = _chkboxMission[i].GetComponent<MissionData>().Id;
                if (missions.Contains(id))
                {
                    _chkboxMission[i].isOn = true;
                }
                else
                {
                    _chkboxMission[i].isOn = false;
                }
            }
        }
        private void SetupEquipmentUnlock(KillChordGameData saveData)
        {
            List<int> equipments = saveData.EquipmentUnlock;
            for (int i = 0; i < _chkboxEquipment.Length; i++)
            {
                int id = _chkboxEquipment[i].GetComponent<EquipmentData>().Id;
                if (equipments.Contains(id))
                {
                    _chkboxEquipment[i].isOn = true;
                }
                else
                {
                    _chkboxEquipment[i].isOn = false;
                }
            }
        }
        private void SetupSkillUnlock(KillChordGameData saveData)
        {
            List<int> skills = saveData.SkillUnlock;
            for (int i = 0; i < _chkboxSkill.Length; i++)
            {
                int id = _chkboxSkill[i].GetComponent<SkillData>().Id;
                if (skills.Contains(id))
                {
                    _chkboxSkill[i].isOn = true;
                }
                else
                {
                    _chkboxSkill[i].isOn = false;
                }
            }
        }

        private void ActivateShelter()
        {
            _shelter.SetActive(true);
            _shelterText.text = "Loading...";
        }

        private void DeactivateShelter()
        {
            _shelter.SetActive(false);
        }
        #endregion    
    }
}