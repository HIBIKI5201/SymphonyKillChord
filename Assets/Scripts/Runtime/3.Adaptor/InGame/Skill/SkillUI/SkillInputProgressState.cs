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
        public SkillInputProgressState(SkillDefinition definition)
        {
            _skillDefinition = definition;
            _currentMachedCount = 0;
            _nextBeatTypeIndex = 0;
        }

        public int CurrentMachedCount => _currentMachedCount;

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
