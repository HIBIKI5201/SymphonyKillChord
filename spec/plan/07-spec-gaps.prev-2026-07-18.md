# Stage 5: 仕様ギャップ

**ステータス: complete（静的仕様35/35の比較について完了）。**

| Severity | Gap | Impact |
|---|---|---|
| High | All note／1.5小節timeout契約が実装経路に見つからない | command開始と被弾後復帰が設計と変わる |
| High | player shotのline-of-sightがTODO | TPSの標的・遮蔽物ルールを破る |
| High | 装備連動interactive BGM wiringがなくtest cue再生 | Build選択が音による入力支援へ届かない |
| High | stage reward crediting経路が見つからない | 研究・強化loopが閉じない |
| High | saveのskill level/options/encryption/atomic recoveryが不足 | 進捗喪失と移行不能リスク |
| High | enemy damageに`new Damage(10)`経路 | content dataとbalanceが乖離 |
| High | PC/Android FPS、load、memoryの自動証拠がない | beta acceptanceを再現できない |
| High | HUD仕様が見出しのみで、音と同等のbeat／hit／skill視覚cue受入条件がない | 聴覚依存、戦闘中の因果不明 |
| Medium | 同末尾noteの同時装備禁止が未強制 | 同時成立が曖昧になる |
| Medium | rhythm sourceが6拍種と8拍種で矛盾 | authoring不能／未対応pattern混入 |
| Medium | result完了後の遷移先がHome／Operationで矛盾 | flow acceptance未定義 |
| Medium | skill-tree効果・slot unlockにTODO | 支払ったnodeが成長へ反映されない |
| Medium | tutorialは順序列挙のみでprompt fade、skip、help、後続transfer判定がない | 完了しても学習を証明できない |
| Medium | titleのdata resetに確認、backup、取消、結果通知が未記載 | 誤操作による不可逆損失 |
| Medium | accessibilityの字幕、色覚、振動、remap、文字可読性が未定義 | 音中心のcore promiseへアクセスできない |
| Medium | auto lockの優先規則に「HPまたは距離」の未決定が残る | 同一入力の結果が定義不能 |
| Medium | mutable外部ZIPをchecksumなしでimport | 同一Git revisionでもbuildが変わる |

## 決定が必要な事項

1. 専用リズム判定ページを正本にして6拍種へ統一するか、12/16を全経路へ追加する。
2. result遷移先、reward確定時点、再試行時のidempotencyを一つの状態機械で定める。
3. save version、per-skill level、暗号化境界、atomic replace、破損復旧、destructive reset確認を一体で設計する。
4. tutorialのtarget action、prompt fade、再プレイ／skip、通常stageでのunaided transferを受入条件化する。
5. beat／hit／enemy telegraphを視覚・聴覚・触覚で冗長化し、設定と端末ごとの代替を定義する。
