# Third-Party Notices

このプロジェクトでは、以下のサードパーティ素材を使用しています。

## Google Fonts — Material Symbols / Material Icons

- **提供元**: Google
- **ライセンス**: Apache License, Version 2.0
- **ライセンス全文**: http://www.apache.org/licenses/LICENSE-2.0
- **配布元リポジトリ**: https://github.com/google/material-design-icons

```
Copyright 2026 Google LLC

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```
## CRIWARE（CRI ADX LE）

- **提供元**: 株式会社CRI・ミドルウェア
- **ライセンス**: CRI ADX LE 使用許諾
- **表記義務**: 配信紹介ページ（iOS・Android ではストアの詳細テキスト）の末尾などに著作表記を載せる。日本国以外の地域では英語版を使う。ロゴ表示は推奨（必須ではない）。決定 R39 により、ゲーム内のクレジットにも載せる

```
本ソフトウェアには、(株)CRI・ミドルウェアの「CRIWARE」が使用されています。
This software uses "CRIWARE (TM)" by CRI Middleware Co., Ltd.
```

## フォント

フォントのライセンス全文は、各フォントと同じフォルダに置いている。

| フォント | 権利者 | ライセンス | 全文 |
| --- | --- | --- | --- |
| IBM Plex Serif | Copyright © 2017 IBM Corp. with Reserved Font Name "Plex" | SIL Open Font License 1.1 | `Assets/Arts/Fonts/IBMPlexSerif-OFL.txt` |
| Inria Serif | Copyright 2017 The Inria Serif Project Authors (https://github.com/BlackFoundryCom/InriaFonts) | SIL Open Font License 1.1 | `Assets/Arts/Fonts/InriaSerif-OFL.txt` |
| Kosugi Maru | Copyright 2010 The Kosugi Maru Project Authors (https://github.com/googlefonts/kosugi-maru) | Apache License 2.0 | `Assets/Arts/Fonts/KosugiMaru-LICENSE.txt` |
| Anton | Copyright 2020 The Anton Project Authors (https://github.com/googlefonts/AntonFont) | SIL Open Font License 1.1 | `Assets/Arts/Fonts/Anton/OFL.txt` |

## オープンソースライブラリ（MIT License）

ランタイム用に導入しているパッケージ（コードから使っていないものも、ビルドに含まれうるため載せる）。どれも MIT License で、全文は下にまとめて載せる。

| ライブラリ | 権利者 | 配布元 |
| --- | --- | --- |
| LitMotion | Copyright (c) 2023 Yusuke Nakada | https://github.com/annulusgames/LitMotion |
| UniTask | Copyright (c) 2019 Yoshifumi Kawai / Cysharp, Inc. | https://github.com/Cysharp/UniTask |
| R3 | Copyright (c) 2024 Cysharp, Inc. | https://github.com/Cysharp/R3 |
| ZLinq | Copyright (c) Yoshifumi Kawai, Copyright (c) Cysharp, Inc. | https://github.com/Cysharp/ZLinq |
| ZString | Copyright (c) 2020 Cysharp, Inc. | https://github.com/Cysharp/ZString |
| ObservableCollections | Copyright (c) 2021 Cysharp, Inc. | https://github.com/Cysharp/ObservableCollections |
| BitonicPixelSorter | Copyright (c) 2020 ruccho | https://github.com/ruccho/BitonicPixelSorter |
| UnityChanSpringBone | Copyright (c) 2018 Unity Technologies | https://github.com/unity3d-jp/UnityChanSpringBone |
| Microsoft.Bcl.AsyncInterfaces、Microsoft.Bcl.TimeProvider、System.ComponentModel.Annotations、System.Runtime.CompilerServices.Unsafe、System.Threading.Channels（R3 の依存） | Copyright (c) .NET Foundation and Contributors | https://github.com/dotnet/runtime |

```
MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 載せていないもの

- エディタでだけ使うもの（Google.Apis、System.CodeDom、System.Management、NuGetForUnity、unity-cli-loop、CopyComponentsByRegex、graph-visualizer など）は実行ファイルに含まれないため載せていない
- Unity 公式パッケージは Unity の利用規約に従う
- アセットストア・外部の素材は、出所と表記義務の確認が済んでから載せる（#1970）
