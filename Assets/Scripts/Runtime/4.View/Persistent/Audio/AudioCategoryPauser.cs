using CriWare;
using System;

namespace KillChord.Runtime.View.Persistent.Audio
{
    /// <summary>
    ///     CRIのカテゴリ単位で、音をまとめて一時停止・再開するクラスです。
    ///     止めたカテゴリだけを覚えておき、再開や破棄のときに戻します。
    /// </summary>
    public sealed class AudioCategoryPauser : IDisposable
    {
        /// <summary>
        ///     一時停止するカテゴリ名を指定して初期化します。
        /// </summary>
        /// <param name="categoryNames"> CRI Atom Craftで定義したカテゴリ名。 </param>
        public AudioCategoryPauser(params string[] categoryNames)
        {
            _categoryNames = categoryNames ?? Array.Empty<string>();
        }

        /// <summary> カテゴリを止めている間はtrue。 </summary>
        public bool IsPaused => _isPaused;

        /// <summary>
        ///     指定したカテゴリの音をまとめて一時停止します。
        /// </summary>
        public void Pause()
        {
            SetPaused(true);
        }

        /// <summary>
        ///     止めたカテゴリの音を再開します。
        /// </summary>
        public void Resume()
        {
            SetPaused(false);
        }

        /// <summary>
        ///     止めたままにしないよう、破棄するときに再開します。
        /// </summary>
        public void Dispose()
        {
            Resume();
        }

        private readonly string[] _categoryNames;
        private bool _isPaused;

        /// <summary>
        ///     カテゴリの一時停止状態を切り替えます。
        /// </summary>
        /// <param name="isPaused"> 一時停止する場合はtrue。 </param>
        private void SetPaused(bool isPaused)
        {
            if (_isPaused == isPaused)
            {
                return;
            }

            foreach (string categoryName in _categoryNames)
            {
                if (!string.IsNullOrEmpty(categoryName))
                {
                    CriAtomExCategory.Pause(categoryName, isPaused);
                }
            }

            _isPaused = isPaused;
        }
    }
}
