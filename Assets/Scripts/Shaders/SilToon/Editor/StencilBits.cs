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
    }
}
