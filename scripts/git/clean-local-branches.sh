#!/bin/bash
# develop に取り込み済みのローカルブランチを削除する。
# 削除は `git branch -d`（未マージなら git が拒否する安全版）だけを使い、強制削除はしない。
#
# 対象外: 現在のブランチ / develop / main / master / 他のワークツリーで使用中のブランチ / develop に未取り込みのコミットがあるブランチ
# 使い方: clean-local-branches.sh [--dry-run] [--yes]
#   --dry-run  削除候補を表示して終了する
#   --yes      確認を省略して削除する

dry_run=0
assume_yes=0
for arg in "$@"; do
    case "$arg" in
        --dry-run) dry_run=1 ;;
        --yes|-y) assume_yes=1 ;;
        *) echo "不明な引数: $arg" >&2; exit 2 ;;
    esac
done

cd "$(dirname "$0")/../.." || exit 1

base=origin/develop
git fetch --prune --quiet || echo "警告: fetch に失敗した。手元の $base で判定する" >&2
git rev-parse --verify --quiet "$base" > /dev/null || { echo "$base が見つからない" >&2; exit 1; }

current=$(git branch --show-current)
# ワークツリーで使用中のブランチ（現在のものを含む）
in_use=$(git worktree list --porcelain | sed -n 's|^branch refs/heads/||p')

delete=()
keep_unmerged=()
keep_worktree=()
while IFS= read -r b; do
    case "$b" in develop|main|master|"$current") continue ;; esac
    if grep -qxF "$b" <<< "$in_use"; then
        keep_worktree+=("$b")
    elif git merge-base --is-ancestor "$b" "$base"; then
        delete+=("$b")
    else
        keep_unmerged+=("$b ($(git rev-list --count "$base..$b") 件が未取り込み)")
    fi
done < <(git for-each-ref --format='%(refname:short)' refs/heads)

echo "判定の基準: $base"
echo
echo "削除候補 (${#delete[@]}):"
[ ${#delete[@]} -gt 0 ] && printf '  %s\n' "${delete[@]}"
if [ ${#keep_worktree[@]} -gt 0 ]; then
    echo; echo "残す: 別のワークツリーで使用中 (${#keep_worktree[@]}):"
    printf '  %s\n' "${keep_worktree[@]}"
fi
if [ ${#keep_unmerged[@]} -gt 0 ]; then
    echo; echo "残す: develop に未取り込み (${#keep_unmerged[@]}):"
    printf '  %s\n' "${keep_unmerged[@]}"
fi
echo

[ ${#delete[@]} -eq 0 ] && { echo "削除するブランチはない。"; exit 0; }
[ $dry_run -eq 1 ] && exit 0

if [ $assume_yes -ne 1 ]; then
    read -r -p "上の ${#delete[@]} 本を削除する? [y/N] " answer
    case "$answer" in y|Y) ;; *) echo "中止した。"; exit 0 ;; esac
fi

# develop 以外を基準に判定したため、HEAD 基準の -d が拒否する場合は失敗として報告する。
failed=0
for b in "${delete[@]}"; do
    git branch -d "$b" || failed=1
done
exit $failed
