namespace DevelopProducts.ToonShader
{
    /// <summary>
    /// SilToon がステンシルバッファを用途ごとに分割して使うためのビット定義。
    /// 1つのバッファを複数機能で共有するため、各機能は自分のビットだけを
    /// ReadMask / WriteMask で読み書きすること。
    /// </summary>
    public static class StencilBits
    {
        /// <summary>目・眉毛を髪の上に透過表示するためのビット。</summary>
        public const int EyeThrough = 1 << 0;

        /// <summary>顔ポリゴンの領域を示すビット。髪の FakeShadow パスがマスクとして参照する。</summary>
        public const int FaceRegion = 1 << 1;

        /// <summary>
        /// FakeShadowパスが「このピクセルは影を描き終えた」と記録するビット。
        /// 髪ポリゴンの重なりで同じピクセルが多重に暗くなるのを防ぐ。
        /// SilToon.shader の FAKE_SHADOW パスがリテラルで持つ値と一致させること。
        /// </summary>
        public const int FakeShadowDrawn = 1 << 2;
    }
}
