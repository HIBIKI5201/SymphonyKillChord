using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキルの入力進行状態を管理するクラス。
    /// </summary>
    public class SkillInputProgressState
    {
        /// <summary>
        ///     スキル定義を指定して、入力進捗が0の状態で生成する。
        /// </summary>
        public SkillInputProgressState(SkillDefinition definition)
        {
            _skillDefinition = definition;
            _currentMachedCount = 0;
            _nextBeatTypeIndex = 0;
        }

        /// <summary> パターンと一致した入力の数。 </summary>
        public int CurrentMachedCount => _currentMachedCount;

        /// <summary>
        ///     入力された拍の種類がパターンの次の拍と一致するかを判定し、進捗を更新する。
        /// </summary>
        public void CheckInputBeatType(BeatType beatType)
        {
            // パターンが完了している場合はリセット
            if (_nextBeatTypeIndex >= _skillDefinition.SkillPattern.Signatures.Length)
            {
                ResetProgress();
            }
            if (beatType == _skillDefinition.SkillPattern.Signatures[_nextBeatTypeIndex])
            {
                _currentMachedCount++;
                _nextBeatTypeIndex++;
            }
            else
            {
                ResetProgress();
            }
        }

        /// <summary>
        ///     入力進捗を最初に戻す。
        /// </summary>
        public void ResetProgress()
        {
            _currentMachedCount = 0;
            _nextBeatTypeIndex = 0;
        }

        private SkillDefinition _skillDefinition;
        private int _currentMachedCount;
        private int _nextBeatTypeIndex;
    }
}
