using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IPortrait の参照情報を取得するリポジトリ。
    /// </summary>
    public interface IPortraitRepository
    {
        /// <summary>
        ///     ID に対応する立ち絵定義を取得する。見つからない場合は false を返す。
        /// </summary>
        bool TryFindById(PortraitId id, out PortraitDefinition portrait);
    }
}
