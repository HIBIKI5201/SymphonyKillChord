using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.OutGame.SkillBuild;
using KillChord.Runtime.Utility.OutGame.Savedata;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.SkillBuild
{
    /// <summary>
    ///     セーブデータへ初期解放・初期装備スキルを補完する初期化モジュール。
    /// </summary>
    public sealed class InitialSkillLoadoutInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(InitialSkillLoadoutInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 90;

        [SerializeField, Tooltip("ゲーム開始時の初期解放・初期装備スキルを定義するアセットです。")]
        private InitialSkillLoadoutAsset _initialSkillLoadout;

        /// <summary>
        ///     セーブデータが未設定の場合に、初期解放・初期装備スキルを補完する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            if (_initialSkillLoadout == null)
            {
                Debug.LogError($"[{nameof(InitialSkillLoadoutInitializer)}] {nameof(InitialSkillLoadoutAsset)} が設定されていません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                Debug.LogError($"[{nameof(InitialSkillLoadoutInitializer)}] {nameof(SavedataSystem)} が取得できませんでした。", this);
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();
            SaveData saveData = await savedataSystem.LoadAsync<SaveData>();
            cancellationToken.ThrowIfCancellationRequested();

            if (TryApplyInitialSkillLoadout(saveData))
            {
                await savedataSystem.SaveAsync(saveData);
            }

            return true;
        }

        /// <summary>
        ///     未設定の解放・装備スキルにのみ初期値を補完する。
        /// </summary>
        /// <param name="saveData"> 対象のセーブデータです。 </param>
        /// <returns> 更新した場合はtrue。 </returns>
        private bool TryApplyInitialSkillLoadout(SaveData saveData)
        {
            bool isChanged = false;

            if (saveData.SkillUnlock.UnlockedSkillIds.Length == 0)
            {
                saveData.SkillUnlock.SetUnlockedSkillIds(_initialSkillLoadout.GetUnlockedSkillIds());
                isChanged = true;
            }

            if (saveData.SkillBuild.EquipmentSkillIDs.Count == 0)
            {
                saveData.SkillBuild.SetEquipmentSkillIDs(new List<int>(_initialSkillLoadout.GetEquippedSkillIds()));
                isChanged = true;
            }

            return isChanged;
        }
    }
}
