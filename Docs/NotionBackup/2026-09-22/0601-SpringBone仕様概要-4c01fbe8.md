# SpringBone仕様概要

- id: 3167c2c6-cc02-80ed-8890-da9c4c01fbe8
- path: Symphony Kill Chord / システム概要 / SpringBone仕様概要
- last_edited: 2026-04-06T03:11:33.471Z

- 概要: 揺れ物に関するパッケージ
- カテゴリー: 開発用


## 説明

### **SpringBoneとは　　　　　　　　　**
- ユニティ・テクノロジーズ・ジャパンが提供している揺れもののシステム
---

### 参考資料
- ‣
- ‣
- ‣

## 詳細
---

#### セットアップ
- **ダウンロード
**[https://github.com/unity3d-jp/UnityChanSpringBone.git?path=/#release/1.2](https://github.com/unity3d-jp/UnityChanSpringBone/tree/release/1.2)**
**Unity6でRelease1.1版を導入するとずっと警告が出てくるのでUnity6で使う場合はRelease1.2を使用する
  release1.1
  [image: スクリーンショット 2026-02-27 010234.png] attachment:2968266e-b44a-46f6-86d0-9238f0baf94d:スクリーンショット_2026-02-27_010234.png
  release1.2
  [image: スクリーンショット 2026-02-27 010342.png] attachment:3300e632-71ee-4abe-ba5c-cf5363d680a7:スクリーンショット_2026-02-27_010342.png

---
- **手順1**
  [image: スクリーンショット 2026-02-28 204918.png] attachment:e3af2583-b2b2-4689-a4fc-543a30295023:スクリーンショット_2026-02-28_204918.png
  - まず動かしたいモデルとメニューバーのWindowからAnimationを選択し、SpringBone→SpringBoneを選択してSpringBoneWindowを開く
- **手順2**
  [image: スクリーンショット 2026-02-28 205331.png] attachment:6ddf85b2-7833-4080-906b-06fae987b3a5:スクリーンショット_2026-02-28_205331.png
  - モデルを選択してCreate or Update Managerをクリックしてください。モデルにSpringManagerがアタッチされる。アタッチされると足元に板状のコライダーも同時に追加される
※Boneが板状のコライダーより下に配置されるとバグる
  - SpringManagerはモデルについているSpringBoneを管理するコンポーネント
- **手順3**
  [image: スクリーンショット 2026-02-28 210654.png] attachment:589a0a41-e2fa-41c9-824f-7ec34461d793:スクリーンショット_2026-02-28_210654.png
  - 次に動かしたいボーンを全て選択して”Add SpringBone”→”Create Origin”の順番にクリック
  - 階層が一番下のオブジェクトにはSpringBoneがアタッチされないのですべて動かしたいのであれば空のオブジェクトを最下層に配置する
  **これで揺れものとしての動きはできるようになる**
---

### SpringManager詳細
- 📄 [[SpringManager]] (3207c2c6-cc02-80f7-aa22-dfcc80f4e978)

### SpringBone詳細
- 📄 [[SpringBone]] (3207c2c6-cc02-80c1-9b99-f8ab3d7d9e6f)

### SpringBoneWindow詳細
- 📄 [[SpringBoneWindow]] (3207c2c6-cc02-805c-bd12-d5d3c87051eb)

## 問題点修正
- 📄 [[[対処済み]シーン開始時に揺れ物ボーンが暴れる]] (3207c2c6-cc02-8076-9df3-e0da4c37d971)
- 📄 [[[要確認]揺れ物の先のボーンが暴れる]] (3207c2c6-cc02-8022-b813-e764a8929ea8)

### SpringBoneの引継ぎについて
- 📄 [[SpringBoneの設定の引継ぎ]] (3397c2c6-cc02-805d-9bc9-df2eaa9ed496)
