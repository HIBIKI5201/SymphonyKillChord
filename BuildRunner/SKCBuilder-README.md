# SKCBuilder

SymphonyKillChord の自動ビルド（GitHub Actions のセルフホストランナー）が作るフォルダ。

- 中の `<ランナー名>` フォルダは**ジャンクション（短い別名）**で、実体はランナーの作業フォルダ（`BuildRunner\_work\SymphonyKillChord\SymphonyKillChord`）にある。
- ランナーの作業フォルダはパスが長く、Unity が扱えるパスの長さ（260文字）を超えてビルドに失敗するため、ビルドのときは Unity をこの短いパスから起動している。
- ビルドのたびにワークフローが作り直す。この README もビルドのたびに上書きされる（元は SymphonyKillChord リポジトリの `BuildRunner/SKCBuilder-README.md`）。

## 消すときの注意

- **PowerShell の `Remove-Item -Recurse` で消さない。** ジャンクションの先（ランナーの作業フォルダ）の中身まで消えることがある。
- ジャンクションだけを外すときは、コマンドプロンプトの `rmdir` を使う（リンクだけが外れ、中身は残る）。

  ```
  rmdir C:\SKCBuilder\<ランナー名>
  ```

- ランナーをやめたあとは、ジャンクションを `rmdir` で外してから、このフォルダごと削除してよい。
- 次のビルドで作り直されるので、外してもビルドには影響しない。

詳しくは SymphonyKillChord リポジトリの `BuildRunner/README.md` を参照。
