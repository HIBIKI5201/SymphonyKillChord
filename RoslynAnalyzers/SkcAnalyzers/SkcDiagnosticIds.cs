namespace SymphonyKillChord.Analyzers
{
    /// <summary> アナライザの診断IDを集約する。 </summary>
    internal static class SkcDiagnosticIds
    {
        /// <summary> SerializeFieldにTooltipが無い。 </summary>
        public const string SerializeFieldTooltip = "SKC0001";

        /// <summary> Debug.LogError/LogWarningのメッセージが規定の接頭辞形式でない。 </summary>
        public const string LogMessagePrefix = "SKC0002";

        /// <summary> ピュア層(Domain/Application/Adaptor)の型がMonoBehaviour/ScriptableObjectを継承している。 </summary>
        public const string PureLayerUnityBase = "SKC0003";

        /// <summary> プロパティにpublic setを使っている。 </summary>
        public const string PublicSetter = "SKC0004";

        /// <summary> 名前空間がフォルダ構成と一致しない。 </summary>
        public const string NamespaceFolder = "SKC0005";

        /// <summary> 1ファイルに複数の公開型がある、またはファイル名が型名と一致しない。 </summary>
        public const string OneTypePerFile = "SKC0006";

        /// <summary> フィールド名が規約に合わない。 </summary>
        public const string FieldNaming = "SKC0007";

        /// <summary> 定数がアッパースネークケースでない。 </summary>
        public const string ConstNaming = "SKC0008";

        /// <summary> インターフェース名が I で始まらない。 </summary>
        public const string InterfaceNaming = "SKC0009";

        /// <summary> イベント名が On で始まらない。 </summary>
        public const string EventNaming = "SKC0010";

        /// <summary> bool型プロパティが Is / Has で始まらない。 </summary>
        public const string BoolPropertyNaming = "SKC0011";

        /// <summary> 型・メソッド・プロパティがパスカルケースでない、または引数がキャメルケースでない。 </summary>
        public const string CasingNaming = "SKC0012";

        /// <summary> フィールドを直接公開している。 </summary>
        public const string PublicField = "SKC0013";

        /// <summary> 制御文のブロックが波カッコで囲まれていない。 </summary>
        public const string Braces = "SKC0014";

        /// <summary> アクセス修飾子が明示されていない。 </summary>
        public const string ExplicitAccessibility = "SKC0015";

        /// <summary> async void の使い方が規約に合わない。 </summary>
        public const string AsyncVoid = "SKC0016";

        /// <summary> メソッドや公開プロパティ・イベントにサマリーが無い。 </summary>
        public const string MissingSummary = "SKC0017";

        /// <summary> 日本語のサマリーが「。」で終わっていない。 </summary>
        public const string SummaryPeriod = "SKC0018";

        /// <summary> using ディレクティブがファイルの先頭にまとまっていない。 </summary>
        public const string UsingPlacement = "SKC0019";

        /// <summary> ピュア層以外が他モジュールへ直接依存している。 </summary>
        public const string CrossModuleDependency = "SKC0020";

        /// <summary> レイヤーの参照方向に反する依存がある。 </summary>
        public const string LayerDependency = "SKC0021";

        /// <summary> メンバーの並び順が規約に合わない。 </summary>
        public const string MemberOrder = "SKC0022";

        /// <summary> 診断のカテゴリ名。 </summary>
        public const string Category = "SymphonyKillChord.CodeGuidelines";
    }
}
