using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Research.Chou.OutGame
{
    public class SaveGame : MonoBehaviour
    {
        /// <summary>セーブ開始時に発火する</summary>
        public event Action OnSaveStarted;
        /// <summary>セーブ完了時に発火する</summary>
        public event Action OnSaveFinished;

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
            OnSaveStarted += ActivateShelter;
            OnSaveFinished += DeactivateShelter;
            //【DEBUG END】
        }
        private void OnDestroy()
        {
            //【DEBUG START】
            OnSaveStarted += ActivateShelter;
            OnSaveFinished += DeactivateShelter;
            //【DEBUG END】
        }
        #endregion

        public void Save()
        {
            StartCoroutine(nameof(SaveSequence));
        }

        private IEnumerator SaveSequence()
        {
            OnSaveStarted?.Invoke();
            LoadSaveData();
            ReadPlayerStatus();
            ReadMissionProgress();
            ReadMissionUnlock();
            ReadEquipmentUnlock();
            ReadSkillUnlock();

            SymphonyFrameWork.System.SaveDataSystem<KillChordSaveData>.Save();
            yield return new WaitForSeconds(2f);
            OnSaveFinished?.Invoke();
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

        private void ReadPlayerStatus()
        {
            int lv = int.Parse(_inputLv.text);
            long exp = long.Parse(_inputExp.text);
            long gold = long.Parse(_inputGold.text);
            float hpMax = float.Parse(_inputHpMax.text);
            float attack = float.Parse(_inputAttack.text);
            float critRate = float.Parse(_inputCritRate.text);
            float critScale = float.Parse(_inputCritScale.text);
            int[] equipments = _inputEquipments.text.Split(',').Select(int.Parse).ToArray();
            int[] skills = _inputSkills.text.Split(',').Select(int.Parse).ToArray();
            PlayerStatus status = new PlayerStatus(lv, exp, gold, hpMax, attack, critRate, critScale, equipments, skills);
            _saveData.PlayerStatus = status;
        }

        #region デバッグ用
        private void ReadMissionProgress()
        {
            HashSet<int> progress = new HashSet<int>();
            int[] missionIds = _inputMissionProgress.text.Split(',').Select(int.Parse).ToArray();
            for (int i = 0; i < missionIds.Length; i++)
            {
                progress.Add(missionIds[i]);
            }
            _saveData.MissionProgress = progress;
        }

        private void ReadMissionUnlock()
        {
            HashSet<int> missions = new HashSet<int>();
            for (int i = 0; i < _chkboxMission.Length; i++)
            {
                if (_chkboxMission[i].isOn)
                {
                    missions.Add(_chkboxMission[i].GetComponent<MissionData>().Id);
                }
            }
            _saveData.MissionUnlock = missions;
        }
        private void ReadEquipmentUnlock()
        {
            HashSet<int> equipments = new HashSet<int>();
            for (int i = 0; i < _chkboxEquipment.Length; i++)
            {
                if (_chkboxEquipment[i].isOn)
                {
                    equipments.Add(_chkboxEquipment[i].GetComponent<EquipmentData>().Id);
                }
            }
            _saveData.EquipmentUnlock = equipments;
        }
        private void ReadSkillUnlock()
        {
            HashSet<int> skills = new HashSet<int>();
            for (int i = 0; i < _chkboxSkill.Length; i++)
            {
                if (_chkboxSkill[i].isOn)
                {
                    skills.Add(_chkboxSkill[i].GetComponent<SkillData>().Id);
                }
            }
            _saveData.SkillUnlock = skills;
        }
        private void ActivateShelter()
        {
            _shelter.SetActive(true);
            _shelterText.text = "Saving...";
        }

        private void DeactivateShelter()
        {
            _shelter.SetActive(false);
        }
        #endregion
    }
}