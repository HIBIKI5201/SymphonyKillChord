using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Utility.Constant;
using R3;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     拍の進行を小節に変換して購読し、区切りごとにBGMセレクターラベルの
    ///     切り替えを駆動するコントローラークラス。
    ///     既存の音楽同期クロックへ相乗りするため、自前の毎フレーム更新は持たない。
    /// </summary>
    public sealed class EquipmentBgmController : IDisposable
    {
        /// <summary>
        ///     新しいコントローラーを生成する。
        /// </summary>
        /// <param name="musicSyncState"> 音楽同期状態。 </param>
        /// <param name="equipmentBgmService"> 装備BGMサービス。 </param>
        /// <param name="bgmSelectorPlayer"> セレクターラベルの適用先。 </param>
        /// <param name="selectorName"> CRIのセレクター名。 </param>
        /// <param name="onLabelApplied"> ラベル切替時の通知先。引数は（小節番号, ラベル名）。動作確認用のログ出力などに使用する。 </param>
        public EquipmentBgmController(
            MusicSyncState musicSyncState,
            EquipmentBgmService equipmentBgmService,
            IBgmSelectorPlayer bgmSelectorPlayer,
            string selectorName,
            Action<int, string> onLabelApplied = null)
        {
            _musicSyncState = musicSyncState ?? throw new ArgumentNullException(nameof(musicSyncState));
            _equipmentBgmService = equipmentBgmService ?? throw new ArgumentNullException(nameof(equipmentBgmService));
            _bgmSelectorPlayer = bgmSelectorPlayer ?? throw new ArgumentNullException(nameof(bgmSelectorPlayer));
            _selectorName = selectorName;
            _onLabelApplied = onLabelApplied;
        }

        /// <summary>
        ///     初期ラベルを適用し、拍ストリームの購読を開始する。
        /// </summary>
        public void Start()
        {
            if (_subscription != null || !_equipmentBgmService.HasSequence)
            {
                return;
            }

            ApplyLabel(_equipmentBgmService.InitialLabel);

            _subscription = _musicSyncState.CurrentBeatRx
                .Select(static beat => ToMeasureIndex(beat))
                .DistinctUntilChanged()
                .Subscribe(OnMeasureChanged);
        }

        /// <summary>
        ///     購読を解除する。
        /// </summary>
        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }

        /// <summary>
        ///     小節が変わった際に、必要であればセレクターラベルを切り替える。
        /// </summary>
        /// <param name="measureIndex"> 現在の小節番号。 </param>
        private void OnMeasureChanged(int measureIndex)
        {
            if (!_equipmentBgmService.TryResolveLabel(measureIndex, out string label))
            {
                return;
            }

            if (!ApplyLabel(label))
            {
                return;
            }

            _onLabelApplied?.Invoke(measureIndex, label);
        }

        /// <summary>
        ///     セレクターラベルを適用する。
        /// </summary>
        /// <param name="label"> 適用するセレクターラベル。 </param>
        /// <returns> 適用した場合はtrue。 </returns>
        private bool ApplyLabel(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return false;
            }

            _bgmSelectorPlayer.SetSelectorLabel(_selectorName, label);
            return true;
        }

        /// <summary>
        ///     拍を小節番号へ変換する。
        /// </summary>
        /// <param name="beat"> 曲頭からの通し拍。 </param>
        /// <returns> 曲頭からの通し小節番号。 </returns>
        private static int ToMeasureIndex(int beat)
        {
            return (int)Math.Floor(beat / MusicConstants.STANDARD_BEATS_PER_BAR);
        }

        private readonly MusicSyncState _musicSyncState;
        private readonly EquipmentBgmService _equipmentBgmService;
        private readonly IBgmSelectorPlayer _bgmSelectorPlayer;
        private readonly string _selectorName;
        private readonly Action<int, string> _onLabelApplied;

        private IDisposable _subscription;
    }
}
