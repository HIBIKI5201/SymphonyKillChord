using KillChord.Runtime.Domain.OutGame.Screen;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     制作メンバー情報リポジトリの抽象。
    /// </summary>
    /// <remarks>
    ///     取得元が CSV か DB かといった保存形式の詳細は InfraStructure 層の実装が持ちます。
    /// </remarks>
    public interface IMemberRepository
    {
        /// <summary>
        ///     全ての制作メンバー情報を取得します。
        /// </summary>
        /// <returns> 制作メンバー情報の一覧です。 </returns>
        IReadOnlyList<MemberData> GetAllMembers();
    }
}
