using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.OutGame.Screen
{
    /// <summary>
    ///     CSV から制作メンバー情報を取得するリポジトリ。
    /// </summary>
    public sealed class MemberCsvRepository : IMemberRepository
    {
        /// <summary>
        ///     制作メンバー CSV を解析してリポジトリを初期化します。
        /// </summary>
        /// <param name="csvText"> 制作メンバー CSV の全文です。 </param>
        /// <exception cref="ArgumentNullException"> CSV の全文が null の場合に発生します。 </exception>
        public MemberCsvRepository(string csvText)
        {
            if (csvText == null)
            {
                throw new ArgumentNullException(nameof(csvText));
            }

            _members = new MemberCsvParser().Parse(csvText);
        }

        /// <summary>
        ///     全ての制作メンバー情報を取得します。
        /// </summary>
        /// <returns> 制作メンバー情報の一覧です。 </returns>
        public IReadOnlyList<MemberData> GetAllMembers()
        {
            return _members;
        }

        private readonly IReadOnlyList<MemberData> _members;
    }
}
