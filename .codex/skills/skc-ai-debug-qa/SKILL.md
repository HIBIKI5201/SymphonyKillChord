---
name: skc-ai-debug-qa
description: Symphony Kill ChordのQAシートに沿ってJSON状態取得、戦闘イベント記録、有限時間の観測、拍指定攻撃を行う。
---

プロジェクトルートの .agents/skills/skc-ai-debug-attack-queue/SKILL.md を読み、共有JSとEditor APIを使用する。
QA項目との対応は Docs/QA/AI_QAツール対応表.md を参照する。QAの進め方とQAシートの正本は Notion「QA」（page id `3e37c2c6-cc02-8198-8919-e7d9fba1f7b8`）で、`python scripts/notion/spec_query.py get <page id>` で写しを読む（手順は Docs/agent/README.md「必要な資料だけを読む」）。実行成功とQA合格を区別し、実機確認をEditor結果だけで合格にしない。
