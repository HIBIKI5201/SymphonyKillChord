using SymphonyFrameWork.System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Research.Chou.OutGame
{
    public class LoadGame : MonoBehaviour
    {
        /// <summary>ロード開始時に発火する</summary>
        public event Action OnLoadStarted;
        /// <summary>ロード完了時に発火する</summary>
        public event Action OnLoadFinished;

        [SerializeField, Tooltip("")]
        private GameObject _shelter;
        [SerializeField, Tooltip("")]
        private Text _shelterText;
        [SerializeField, Tooltip("")]
        private InputField _inputLv;
        [SerializeField, Tooltip("")]
        private InputField _inputExp;
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

        private KillChordSaveData _saveData;

        #region ライフサイクル
        private void Awake()
        {
            //【DEBUG START】
            OnLoadStarted += ActivateShelter;
            OnLoadFinished += DeactivateShelter;
            //【DEBUG END】
        }
        private void OnDestroy()
        {
            //【DEBUG START】
            OnLoadStarted += ActivateShelter;
            OnLoadFinished += DeactivateShelter;
            //【DEBUG END】
        }
        #endregion

        public void Load()
        {
            try
            {
                _saveData = SaveDataSystem<KillChordSaveData>.Data;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }
            StartCoroutine(nameof(SetupDataSequence));
        }

        private IEnumerator SetupDataSequence()
        {
            OnLoadStarted?.Invoke();
            LoadSaveData();
            SetupPlayerStatus();
            SetupMissionProgress();
            SetupMissionUnlock();
            SetupEquipmentUnlock();
            SetupSkillUnlock();
            yield return new WaitForSeconds(2f);
            OnLoadFinished?.Invoke();
        }

        private void LoadSaveData()
        {
            try
            {
                _saveData = SymphonyFrameWork.System.SaveDataSystem<KillChordSaveData>.Data;
            }
            catch (Exception e)
            {
                _saveData = new KillChordSaveData();
            }
        }

        #region デバッグ用
        private void SetupPlayerStatus()
        {
            PlayerStatus status = _saveData.PlayerStatus;
            _inputLv.text = status.Lv.ToString();
            _inputExp.text = status.Exp.ToString();
            _inputGold.text = status.Gold.ToString();
            _inputHpMax.text = status.HpMax.ToString();
            _inputAttack.text = status.Attack.ToString();
            _inputCritRate.text = status.CritRate.ToString();
            _inputCritScale.text = status.CritScale.ToString();
            _inputEquipments.text = string.Join(',', status.Equipments);
            _inputSkills.text = string.Join(',', status.Skills);
        }

        private void SetupMissionProgress()
        {
            HashSet<int> progress = _saveData.MissionProgress;
            _inputMissionProgress.text = string.Join(',', progress.ToArray());
        }

        private void SetupMissionUnlock()
        {
            HashSet<int> missions = _saveData.MissionUnlock;
            for (int i = 0; i < _chkboxMission.Length; i++)
            {
                int id = _chkboxMission[i].GetComponent<MissionData>().Id;
                if (missions.Contains(id))
                {
                    _chkboxMission[i].isOn = true;
                }
            }
        }
        private void SetupEquipmentUnlock()
        {
            HashSet<int> equipments = _saveData.EquipmentUnlock;
            for (int i = 0; i < _chkboxEquipment.Length; i++)
            {
                int id = _chkboxEquipment[i].GetComponent<EquipmentData>().Id;
                if (equipments.Contains(id))
                {
                    _chkboxEquipment[i].isOn = true;
                }
            }
        }
        private void SetupSkillUnlock()
        {
            HashSet<int> skills = _saveData.SkillUnlock;
            for (int i = 0; i < _chkboxSkill.Length; i++)
            {
                int id = _chkboxSkill[i].GetComponent<SkillData>().Id;
                if (skills.Contains(id))
                {
                    _chkboxSkill[i].isOn = true;
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