using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.Persistent.Localization
{
    /// <summary>
    ///     UI ToolkitのImageに、現在の入力機器の入力アイコンを表示します。
    ///     UICommonの <c>ui.input.glyph.キー</c> が返すスプライト名を、TMPのSprite Assetから切り出して使います。
    /// </summary>
    public sealed class InputGlyphImage : IDisposable
    {
        /// <summary>
        ///     Localizationの初期化後に入力アイコンの購読を開始します。
        ///     取得できるまでは、Imageに設定済みの画像をそのまま表示します。
        /// </summary>
        /// <param name="image"> 表示先のImageです。 </param>
        /// <param name="spriteAsset"> 入力アイコンを収めたSprite Assetです。 </param>
        /// <param name="glyphKey"> 入力アイコンの翻訳キー末尾です。 </param>
        /// <exception cref="ArgumentNullException"> 表示先またはSprite Assetがnullの場合に発生します。 </exception>
        /// <exception cref="ArgumentException"> 翻訳キー末尾が空の場合に発生します。 </exception>
        public InputGlyphImage(Image image, TMP_SpriteAsset spriteAsset, string glyphKey)
        {
            _image = image ?? throw new ArgumentNullException(nameof(image));
            _spriteAsset = spriteAsset ?? throw new ArgumentNullException(nameof(spriteAsset));
            if (string.IsNullOrEmpty(glyphKey))
            {
                throw new ArgumentException("翻訳キー末尾を指定してください。", nameof(glyphKey));
            }

            LocalizationInitializer.RunWhenInitialized(isReady =>
            {
                if (_isDisposed || !isReady)
                {
                    return;
                }

                _localizedText = new LocalizedElementText(TABLE_NAME, GLYPH_ENTRY_PREFIX + glyphKey, HandleGlyphChanged);
            });
        }

        /// <summary>
        ///     入力アイコンの購読を解除します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _localizedText?.Dispose();
            _localizedText = null;
            _isDisposed = true;
        }

        private const string TABLE_NAME = "UICommon";
        private const string GLYPH_ENTRY_PREFIX = "ui.input.glyph.";
        private const string NO_GLYPH = "-";
        private static readonly Regex SPRITE_NAME_PATTERN = new("<sprite name=\"([^\"]+)\">", RegexOptions.Compiled);

        private readonly Image _image;
        private readonly TMP_SpriteAsset _spriteAsset;
        private LocalizedElementText _localizedText;
        private bool _isDisposed;

        /// <summary>
        ///     入力アイコンの翻訳が変わったときに、Imageの画像を切り替えます。
        /// </summary>
        /// <param name="glyph"> TMPのスプライトタグを含む入力アイコンの翻訳です。 </param>
        private void HandleGlyphChanged(string glyph)
        {
            // 割り当てがない機器では、案内の画像を出さない。
            if (glyph == NO_GLYPH)
            {
                _image.style.display = DisplayStyle.None;
                return;
            }

            // 複数のアイコンを並べる操作は1枚の画像にできないため、先頭のアイコンを使う。
            Match match = SPRITE_NAME_PATTERN.Match(glyph ?? string.Empty);
            if (!match.Success || !TryGetSpriteRect(match.Groups[1].Value, out Rect sourceRect))
            {
                Debug.LogWarning($"[{nameof(InputGlyphImage)}] 入力アイコンの画像が見つかりませんでした。Glyph: {glyph}");
                return;
            }

            _image.style.display = DisplayStyle.Flex;
            _image.sprite = null;
            _image.image = _spriteAsset.spriteSheet;
            _image.sourceRect = sourceRect;
        }

        /// <summary>
        ///     Sprite Assetからスプライトの切り出し範囲を取得します。
        /// </summary>
        /// <param name="spriteName"> スプライト名です。 </param>
        /// <param name="sourceRect"> テクスチャ左上を原点とした切り出し範囲です。 </param>
        /// <returns> 取得できた場合はtrueです。 </returns>
        private bool TryGetSpriteRect(string spriteName, out Rect sourceRect)
        {
            sourceRect = default;
            int index = _spriteAsset.GetSpriteIndexFromName(spriteName);
            if (index < 0 || _spriteAsset.spriteSheet == null)
            {
                return false;
            }

            // TMPのグリフは左下原点、UI ToolkitのsourceRectは左上原点のため、縦方向を反転する。
            UnityEngine.TextCore.GlyphRect glyphRect = _spriteAsset.spriteCharacterTable[index].glyph.glyphRect;
            int textureHeight = _spriteAsset.spriteSheet.height;
            sourceRect = new Rect(
                glyphRect.x,
                textureHeight - glyphRect.y - glyphRect.height,
                glyphRect.width,
                glyphRect.height);
            return true;
        }
    }
}
