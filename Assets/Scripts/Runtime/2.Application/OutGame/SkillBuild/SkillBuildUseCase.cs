using KillChord.Runtime.Domain.OutGame.SkillBuild;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.SkillBuild
{
    /// <summary>
    ///     装備スキル構成の検証、保存、確定を行うユースケース。
    /// </summary>
    public sealed class SkillBuildUseCase
    {
        /// <summary>
        ///     SkillBuildUseCase クラスのコンストラクタ。
        /// </summary>
        /// <param name="skillBuildDefinition"> 装備スキルの定義を表すオブジェクト。 </param>
        /// <param name="skillBuildRepository"> 装備スキル構成リポジトリ。 </param>
        public SkillBuildUseCase(
            SkillBuildDefinition skillBuildDefinition,
            ISkillBuildRepository skillBuildRepository)
        {
            _skillBuildDefinition = skillBuildDefinition
                ?? throw new ArgumentNullException(nameof(skillBuildDefinition));
            _skillBuildRepository = skillBuildRepository
                ?? throw new ArgumentNullException(nameof(skillBuildRepository));
        }

        /// <summary>
        ///     改造画面のセーブデータを非同期で保存するメソッド。
        /// </summary>
        /// <param name="equippedSkills"> 保存する装備スキル構成。 </param>
        /// <returns> 非同期操作の完了を表す Task オブジェクト。 </returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public async Task SaveSkillBuildAsync(IReadOnlyList<EquippedSkill> equippedSkills)
        {
            if (equippedSkills == null)
            {
                throw new ArgumentNullException(nameof(equippedSkills));
            }

            _skillBuildDefinition.ValidateEquipment(equippedSkills);
            EquippedSkill[] newEquippedSkills = CopyEquippedSkills(equippedSkills);
            await _skillBuildRepository.SaveSkillBuildAsync(newEquippedSkills);

            // 永続化に成功した編成だけをドメインの確定状態へ反映する。
            _skillBuildDefinition.UpdateEquippedSkills(newEquippedSkills);
        }

        /// <summary>
        ///     指定したスキルのレベルを1上げ、改造Pを1消費する。
        /// </summary>
        /// <param name="skillId"> 対象スキルID。 </param>
        /// <param name="baseLevel"> 保存記録が無い場合の基準レベル(テンプレートの初期レベル)。 </param>
        /// <returns> 改造Pが不足している等の理由で実行できなかった場合は false。 </returns>
        public Task<bool> LevelUpSkillAsync(int skillId, int baseLevel)
        {
            return _skillBuildRepository.TryLevelUpSkillAsync(skillId, baseLevel);
        }

        private readonly SkillBuildDefinition _skillBuildDefinition;
        private readonly ISkillBuildRepository _skillBuildRepository;

        /// <summary>
        ///     装備スキル一覧を配列へコピーする。
        /// </summary>
        /// <param name="equippedSkills"> コピー元の装備スキル一覧。 </param>
        /// <returns> コピーした装備スキル配列。 </returns>
        private EquippedSkill[] CopyEquippedSkills(IReadOnlyList<EquippedSkill> equippedSkills)
        {
            EquippedSkill[] result = new EquippedSkill[equippedSkills.Count];
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                result[i] = equippedSkills[i];
            }

            return result;
        }
    }
}
