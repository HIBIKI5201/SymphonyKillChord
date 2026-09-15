# ボタン実装の用語集

ボタンの実装を指示するときに使う3つの用語と、それぞれが指す実ファイル/クラス/USSクラスをまとめる。
「〇〇の実装を直して」とだけ伝えれば、この表を見て対象箇所が一意に分かる状態にすることが目的。

## ボタンの見た目の実装

画像・色・サイズなど静的な見た目のみを指す。スケールの動きやタイミングは含まない。

- 対象: [Button.uss](../../../../Level/Scenes/Develop/OutGameTest/ScreenTransitionTest/UI%20Toolkit/Uss/Button.uss) の `.btn-skin` クラス
- 内容: 通常時 `UI_Button_off.png`、hover/focus/active時 `UI_Button_on.png` に切り替える背景画像スキン。デフォルトサイズ(200×100px)も持つ。個別のボタンで別サイズにしたい場合はUXML側で `style="width: ...; height: ...;"` のようなinline styleを指定すれば、C#を書かずに上書きできる

## アニメーションの実装

スケールの動き・タイミング・イージングのみを指す。画像には触れない。

- 対象:
  - [Button.uss](../../../../Level/Scenes/Develop/OutGameTest/ScreenTransitionTest/UI%20Toolkit/Uss/Button.uss) の `.btn-scale-feedback` クラス(クリック時の縮小演出、transitionの基本設定)
  - [ButtonPulseAnimationManipulator.cs](ButtonPulseAnimationManipulator.cs)(hover/focus中に拡大・縮小を途切れなく繰り返す連続パルス)
  - [ButtonAnimationExtensions.cs](ButtonAnimationExtensions.cs) の `EnableButtonPulseAnimation`(パルスを要素へ付与する拡張メソッド)

## ボタンの実装(全部)

見た目 + アニメーション + クリック/フォーカス等の挙動をまとめた、完成品のボタン一式を指す。

- 対象: [ButtonPresetExtensions.cs](ButtonPresetExtensions.cs) の `ApplyBasicButtonPreset`
- 内容: 上記の「見た目」「アニメーション」のUSSクラス付与、[UINavigationExtensions.cs](../Navigation/UINavigationExtensions.cs) によるフォーカス/クリック対応(`MakeNavigable` / `RegisterActivation`)、パルスアニメーションの付与(`EnableButtonPulseAnimation`)を1呼び出しでまとめて行う

## 使い方の例

新しいボタンにこのテンプレートを丸ごと適用する場合:

```csharp
using KillChord.Runtime.View.OutGame.Common;

var disposable = someButtonElement.ApplyBasicButtonPreset(() => Debug.Log("クリックされた"));
// 不要になったら disposable.Dispose() で解除する
```

見た目だけ、アニメーションだけを個別に使いたい場合は、UXML側で `class="btn-skin"` や `class="btn-scale-feedback"` を個別に付け、必要に応じて `EnableButtonPulseAnimation()` のみを呼び出す。
