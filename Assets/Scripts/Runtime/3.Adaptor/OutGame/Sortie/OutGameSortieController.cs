using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Sortie
{
    /// <summary>
    ///     出撃要求をユースケースへ伝えるコントローラー。
    /// </summary>
    public sealed class OutGameSortieController
    {
        /// <summary>
        ///     コントローラーを初期化する。
        /// </summary>
        /// <param name="useCase">出撃ユースケース。</param>
        public OutGameSortieController(OutGameSortieUseCase useCase)
        {
            _useCase = useCase;
        }

        /// <summary>
        ///     ステージ種別に応じた出撃処理を開始する。
        /// </summary>
        /// <param name="stageType"> ステージ種別。 </param>
        /// <param name="targetSceneName"> 遷移先シーン名。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 処理に成功した場合は true。 </returns>
        public async Task<bool> RequestSortieAsync(
            StageType stageType,
            string currentSceneName,
            string targetSceneName,
            CancellationToken cancellationToken)
        {
            return await _useCase.RequestSortieAsync(
                stageType,
                currentSceneName,
                targetSceneName,
                cancellationToken);
        }

        private readonly OutGameSortieUseCase _useCase;
    }
}
