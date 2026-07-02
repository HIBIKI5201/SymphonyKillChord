using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Utility.OutGame.Savedata;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
    ///     装備スキルの組み合わせから、再生するBGMのキュー名を解決するサービスクラス。
    /// </summary>
    public sealed class SkillBgmResolveService
    {
        /// <summary>
        ///     カタログを指定してサービスを生成する。
        /// </summary>
        /// <param name="catalog"> 装備スキルとBGMの対応カタログ。 </param>
        /// <exception cref="ArgumentNullException"> catalog が null の場合にスローされます。 </exception>
        public SkillBgmResolveService(SkillBgmCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        /// <summary>
        ///     装備スキルの組み合わせに対応するBGMのキュー名を非同期で解決する。
        ///     セーブデータを優先し、存在しない場合は SkillBuildDefinition から取得する。
        /// </summary>
        /// <returns> 解決されたBGMのキュー名。 </returns>
        public async ValueTask<string> ResolveCueAsync()
        {
            IReadOnlyList<int> skillIds = await LoadEquippedSkillIdsAsync();
            return _catalog.Resolve(skillIds);
        }

        private readonly SkillBgmCatalog _catalog;

        /// <summary>
        ///     装備スキルIDの一覧を取得する。
        ///     セーブデータが存在すればそこから、存在しなければ SkillBuildDefinition から取得する。
        /// </summary>
        /// <returns> 装備スキルIDの一覧。 </returns>
        private async ValueTask<IReadOnlyList<int>> LoadEquippedSkillIdsAsync()
        {
            // セーブデータが存在する場合は、それを優先して装備スキルIDを取得する
            if (ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem)
                && savedataSystem.Exists<SaveData>())
            {
                SaveData saveData = await savedataSystem.LoadAsync<SaveData>();
                if (saveData?.SkillBuild != null)
                {
                    return saveData.SkillBuild.EquipmentSkillIDs;
                }
            }

            // セーブデータが存在しない場合は、既存の装備定義から装備スキルIDを取得する
            if (ServiceLocator.TryGetInstance(out SkillBuildDefinition buildDefinition)
                && buildDefinition.EquippedSkills != null)
            {
                List<int> skillIds = new List<int>(buildDefinition.EquippedSkills.Count);
                for (int i = 0; i < buildDefinition.EquippedSkills.Count; i++)
                {
                    EquippedSkill equippedSkill = buildDefinition.EquippedSkills[i];
                    if (equippedSkill.HasSkill)
                    {
                        skillIds.Add(equippedSkill.SkillData.Id);
                    }
                }

                return skillIds;
            }

            // どちらからも取得できない場合は空の一覧を返す
            return Array.Empty<int>();
        }
    }
}
