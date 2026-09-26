# SpringBoneの設定の引継ぎ

- id: 3397c2c6-cc02-805d-9bc9-df2eaa9ed496
- path: Symphony Kill Chord / システム概要 / SpringBone仕様概要 / SpringBoneの設定の引継ぎ
- last_edited: 2026-04-05T11:25:04.344Z

SpringBoneは内部的にBoneの名前と値で対応をとっているのでBoneの名前を変えないように
する必要があります。
構造は変わっても問題なく引継ぎができます。
- 手順１
  [image: スクリーンショット 2026-04-05 185715.png] attachment:308d8bab-2196-46e2-9efb-d43099ba470d:スクリーンショット_2026-04-05_185715.png
  > 💡 
    Inspectorでプレハブを選択した状態でSaveボタンを押下
- 手順２
  [image: スクリーンショット 2026-04-05 185725.png] attachment:478a1817-31db-4d1b-a126-7446af903620:スクリーンショット_2026-04-05_185725.png
  > 💡 
    Save to CSVを選択後、保存先のフォルダを選択して保存
- 手順３
  [image: スクリーンショット 2026-04-05 193010.png] attachment:9897f77a-d45b-4a56-9187-83b1bb2d88ce:スクリーンショット_2026-04-05_193010.png
  > 💡 
    新たなプレハブをInspectorで指定し、SpringBoneWindowでLoadを押下、SetUpFromCSVfileを押し、手順２で作ったCSVを選択すると適用
