# アセット台帳と Drive・Unity・CRI の照合（Issue #2359 P7 / 回答シート Q31）

- 作成日: 2026-11-11
- 対象: Notion マスターアセットリスト（101行）、SE リスト・BGM リスト（38 Cue）、キャラクターボイス、Google Drive `03_Art` / `04_Sound` の Export、Unity（develop `4af8ef28`）の参照名、CRI `CueSheet_0.acb` の CueNameTable
- 完了条件（P7）: 同じアセットを別物として発注せず、コード上に残す旧称と誤表記を区別できる（指摘 S14・S16）
- 方法: 台帳はミラー `Docs/NotionSpecifications` から読み、Drive は API でフォルダ木とファイルサイズを取得した。Unity は `.meta` の GUID と、yaml・USS・UXML から集めた参照 GUID（2766件）を使い、Drive Export とは「拡張子が同じでサイズ20KB超かつ同サイズ」で突き合わせた。ACB は @UTF テーブルを解析して Cue 名を取り出した。PC の Unity は使っていない。
- 前提: Drive の他者所有の原本は 11-09 に `_Old` 改名済み。本照合は `_Old` を除いた現行ファイルを対象にした。

## 結論

- 美術101行のうち約60行は、下の別名表を当てれば台帳・Drive・Unity が1対1で対応する。**同じアセットが別名で二重に存在する例（重複発注の危険）は、別名表に載せた組だけで、それ以外には見つからなかった。**
- 旧称と誤表記は区別できる。Unity 側に残る名前は「旧称」（別名表 A）、Drive 側に残る誤字は「誤表記」（表 B、#1996 で Unity 側だけ直したもの）に分けた。
- 台帳に対する Drive・Unity の欠けは「未制作」「03_Art 外・対象外」「Export と Unity の版違い」「Drive にあり Unity にない」の4種類に分かれる（下記）。
- サウンドは ACB とリポジトリの実体が一致し、SE・BGM 台帳の38 Cue はすべて ACB にあり Unity から参照されている。古いのは台帳の記述（行番号・注記）とボイスのページ。
- **P7 の調査は完了**。残りは台帳・Notion の書き直しと Unity 側の改名で、これは調査ではなく修正作業なので別 Issue に分けるのがよい。

## A. Unity に残る旧称（Drive・台帳の正式名 → Unity のファイル名）

同じ実体とみられる組。根拠は同サイズ、または名前・用途・参照先の一致で、ファイル内容のハッシュは比べていない（同サイズの組も「同一の候補」として扱う）。発注・検索は左の正式名で行い、右はコード・Prefab 上の旧称として扱う。

| 区分 | 台帳・Drive の正式名 | Unity の名前 | 備考 |
| --- | --- | --- | --- |
| 背景小物 | `ENV_Prop_Tire` / `VEH_Bus` / `VEH_Helicopter` / `ENV_Prop_Aircon` / `ENV_Prop_WeaponBox` / `VEH_MilitaryCar` | `Bg_*`（Tire, Bus, helicopter, OutdoorUnit, WeaponBox, MilitaryTruck） | Aircon=OutdoorUnit、MilitaryCar=MilitaryTruck は名前自体も違う |
| VFX | `VFX_Attack_01`〜`04` | `Firing_01`〜`04` | |
| VFX | `VFX_Hit_01` / `VFX_Hit_03` | `Hidan_Hiban01` / `Hidan_Hiban03` | |
| VFX | `VFX_Hit_Player` | `Hidan_Hiban02` | 同サイズ 589796 |
| VFX | `VFX_Explosion` | `Bakuhatu` | **版違い**（Drive 350351 / Unity 353995） |
| VFX | `VFX_MoveDust`（Walk / Run / SpeedLine） | `walk_smoke` / `run_smoke` / `speed_Lines_run_01` | 3つとも Prefab がどこからも GUID 参照されていない（未使用） |
| モーション | `MOT_Dodge_Player` | `20260816_dodge.fbx` | |
| モーション | `MOT_RightAttack` / `MOT_LeftAttack` | `Symphony_ARpose_0615_3` / `Symphony_HGpose_0615_3` | |
| 武器 | `WPN_Revolver` | `WPN_OTs_20_Gnom` | 実銃名 |
| 武器 | `WPN_SniperRifle` / `WPN_LeverActionRifle` / `WPN_HandGun` | `M82_SniperRifle[0911]` / `…_0821` / `…[0508]` | 日付付き |
| 武器 | `WPN_AssaultRifle` | `Copia de assault_rifle_LOW[07_07_2026].fbx`（フォルダ名 `Assult Rifle`） | フォルダ名も誤字 |
| キャラ | `CHS_Prelude` / `CHS_Triad` | Drive では `9号透過` / `1号透過`、Unity は `CHS_*.png` | Drive 側が旧称 |
| キャラ | `CHS_Symphony` | Drive `symphony立ち絵-ポーズ差分.png` = Unity `CHS_Symphony.png` | 同サイズ 3893187、参照1件 |
| キャラ | `CHM_Symphony_model` | `Symphony_0828.fbx` | |
| キャラ | `CHM_Soldier14` | `Soldier14_0808.fbx` | サイズがほぼ同じ（9335888 / 9335920）。再書き出しの差とみられる |
| UI | `UI_TextBox*` | `UI_Scenario_TextBox.*` | Unity 側に誤字 `Flame2` |
| UI | シナリオのボタン（日本語名） | `UI_AutoButton` など | |
| UI | `ENV_BG_City` / `UI_TitleLogo` / `UI_TitleScreen` | `BackGround_OutdoorsInCity.jpg` / `title_logo.png` / `Title_Background.png` | |
| UI | 攻撃表示デカール | `EnemyAttack_Circle_*Gear` | |

## B. Drive に残る誤表記（#1996 で Unity 側だけ直したもの）

| Drive のファイル | Unity の現名 | 台帳行 |
| --- | --- | --- |
| `hoge-bullet.fbx` | `BarrageBullet.fbx`（参照1件） | `ENV_Prop_FlakShell` |
| `KillChored_HomePageQR.png` | `KillChord_HomePageQR`（参照1件） | QR コード |
| `BgM_MillitaryCar_*` | `MilitaryTruck`（綴り修正済み） | `VEH_MilitaryCar` |

Drive の名前を直すかは提出者・外注先の管理に関わるため、ここでは直していない。

## C. Drive Export と Unity の版が違うもの

| 台帳行 | Drive Export | Unity が使うもの |
| --- | --- | --- |
| `MOT_Skill_HandGun` | `gunaction_sazawa_0812` | `Symphony_skill_*_v001` |
| `MOT_Skill_Rifle` | `TGS_RifleMotion_polished` | `Symphony_skill_*_v001` |
| `MOT_Move_Player` | `TGS_runcycle_polished_abe_v1`、`Symphony_Run_Test_By_Sasaya` | `Symphony_run_*` |
| `CHM_Infantry` | `CHM_Enemy_MD`（`MD_MotionEditing_Lowpoly_01/02`） | `Assets/Arts/Models/EnemyLowPoly/US-Soldier_50k.fbx`（GUID 参照0件）と `EnemyInfantry_LowPoly50k` Prefab |
| `VFX_Explosion` | 350351 bytes | `Bakuhatu` 353995 bytes |

Drive の Export が新しい版なのか、Unity が後で手直しした版なのかは日付だけでは決められない。どちらを正とするかは担当者の判断が要る。

## D. Drive Export にあって Unity にないもの

- `UI_Tutorial`: 画像22枚。Unity には `TutorialPopup/Screenshot_JustAttack.png` だけ。
- `UI_ControllerIcon`: Xbox アイコン18枚。
- `UI_ButtonFrame`、`ENV_BG_Camp`
- `ENV_Field.unitypackage`: `STG_City` と `ENV_Ground_*` を含むパッケージなので、ファイル単位では照合できない。

## E. 台帳にあって Drive にフォルダがないもの（21行）と、フォルダが空のもの（1行）

- 未制作（16行）: `EMB_Kapellmeister`、`UI_ComboCounter`、`VFX_Skill`、`VEH_Car_Generic`、`UI_BuffDebuffIcon`、`MOT_IdleHome`、`MOT_IdleBattle`、`UI_MissionWindow`、`CHS_Voronov`、`VFX_Dodge`、`VFX_Knockback`、`VFX_FieldFogDust`、`VFX_EnemyDefeat`、`MOT_Idle_Enemy`、`MOT_Attack_Enemy`、`MOT_Move_Enemy`
- 03_Art の外、または対象外（5行）: `UI_AppIcon_Symphony`、`UI_StoreBanner`、`GamePV.mp4`、`Anton-Regular`（フォント）、`StageStart`
- フォルダはあるが空（1行）: `CHS_Soldier14`

## F. サウンド

- Drive `04_Sound/CRI` の `CueSheet_0.acb`（18263808）と `.acf`（7616）は、リポジトリの `StreamingAssets` と同サイズ。`CueSheet_0.h` は5月のままで1 Cue しか載っていない（古い）。
- ACB の Cue は125。SE・BGM 台帳の38 Cue はすべて ACB にあり、Unity から参照されている。
- 台帳の「使用箇所」の行番号が古い。例: `Player.prefab` 1355→1312、612→586、1156→1113。`InGame.unity` 34070→33878、13490→13222。敵 Prefab は約17行ずれている。
- 出撃音の注記「出撃ボタンにこのクラスが付いていない」は古い。現在の `StageDetail.uxml` には `ui-se-sortie` が付いている。
- ACB にあるが未使用の Cue: `SE_Enemy_Alert_First/Second`（旧称）、`SE_Symphony_Damage_Light/Heavy`（未接続）。
- `SE_Cursor` は台帳にも ACB にもない。`SE_Just` も ACB にまだない（#2540 待ち）。
- ボイス（`キャラクターボイス` ページ）が古い。
  - ページでは「14号とシナリオのボイスは Cue なし」だが、ACB には `VOICE_Soldier14_*`（7）と `VOICE_Symphony_Scenario*`（16）・`VOICE_Triad_Scenario*`（15）の計31がある。
  - `VOICE_Symphony_Home2` がページに載っていない。
  - `Player.prefab` は旧称 `VOICE_Battlestart`、`VOICE_Damage_Light`、`VOICE_Skill` を使っている。クリアボイスは `VOICE_Symphony_Gameclear1`〜`3`。
  - トライアドの `Tutorial1`〜`3`、`10`、`11` は未使用（#2074）。`Tutorial4`〜`9` は `MissionDefinition_Tutorial` から使われている。

## 次の作業（提案。いずれも未着手）

1. 台帳に「旧称・Unity 名」の列を足し、表 A の対応を書く（Notion 書き込み）。
2. SE・BGM 台帳の使用箇所の行番号と出撃音の注記を直す。行番号ではなく GUID や要素名で書くと古くなりにくい。
3. `キャラクターボイス` ページを ACB の現状（Soldier14・シナリオ・Home2）に合わせる。
4. 表 C の版違いについて、どちらを正とするかを担当者に確認する。
5. Unity 側の旧称を正式名に改名するかを決める（GUID は保たれるが、Addressables のキーやコード上の文字列参照の確認が要る）。
6. 未使用の `VFX_MoveDust` 系 Prefab、`US-Soldier_50k.fbx`、未使用 Cue の扱いを決める。
