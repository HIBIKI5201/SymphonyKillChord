#!/bin/bash
# GitHub のコラボレーターを管理する。gh（GitHub CLI）でログインしたアカウントの権限で動く。
#
# 使い方: collaborators.sh <list|invite> [--owner <アカウント>] [--include-forks]
#   list    リポジトリごとに、どのアカウントがコラボレーターかを表で出す（何も変えない）
#   invite  入力したアカウントを、対象の全リポジトリへコラボレーターとして招待する（確認あり）
#           invite <アカウント> [--yes] で、入力と確認を省ける
#
# 対象: --owner のアカウント（省略時は gh でログインしているアカウント）が持つリポジトリ。
#       アーカイブ済みは除く。フォークは --include-forks を付けたときだけ含める。
# 招待の権限: 個人アカウントのリポジトリでは GitHub の仕様で常に write（書き込み）になる。
#             組織のリポジトリでは --permission <read|triage|write|maintain|admin>（既定 write）で選べる。

mode=""
owner=""
target=""
include_forks=0
assume_yes=0
permission="push"

usage() {
    sed -n '2,12p' "$0" | sed 's/^# \{0,1\}//'
}

while [ $# -gt 0 ]; do
    case "$1" in
        list|invite) mode="$1" ;;
        --owner) shift; owner="$1" ;;
        --include-forks) include_forks=1 ;;
        --yes|-y) assume_yes=1 ;;
        --permission)
            shift
            case "$1" in
                read) permission="pull" ;;
                write) permission="push" ;;
                triage|maintain|admin) permission="$1" ;;
                *) echo "不明な権限: $1（read / triage / write / maintain / admin のどれか）" >&2; exit 2 ;;
            esac ;;
        -h|--help) usage; exit 0 ;;
        -*) echo "不明な引数: $1" >&2; usage >&2; exit 2 ;;
        *)
            if [ "$mode" = "invite" ] && [ -z "$target" ]; then
                target="$1"
            else
                echo "不明な引数: $1" >&2; usage >&2; exit 2
            fi ;;
    esac
    shift
done

if [ -z "$mode" ]; then
    usage >&2
    exit 2
fi

# --- gh の確認 -------------------------------------------------------------
if ! command -v gh > /dev/null 2>&1; then
    echo "gh（GitHub CLI）が見つからない。"
    echo "  Windows: PowerShell で winget install --id GitHub.cli を実行し、ターミナルを開き直す"
    echo "  macOS:   brew install gh"
    echo "入れたあと gh auth login でログインしてから、もう一度動かす。"
    exit 1
fi
me=$(gh api user --jq .login 2> /dev/null)
if [ -z "$me" ]; then
    echo "gh にログインしていない（またはネットワークにつながらない）。"
    echo "ターミナルで gh auth login を実行してログインしてから、もう一度動かす。"
    exit 1
fi
[ -n "$owner" ] || owner="$me"

# --- 対象のリポジトリ ------------------------------------------------------
repo_query='.[] | select(.isArchived | not)'
[ $include_forks -eq 1 ] || repo_query="$repo_query | select(.isFork | not)"
repos=$(gh repo list "$owner" --limit 1000 --json nameWithOwner,isArchived,isFork \
    --jq "$repo_query | .nameWithOwner" | sort -f)
if [ -z "$repos" ]; then
    echo "$owner のリポジトリが見つからない（アーカイブ済み・フォークは除外している）。"
    exit 1
fi
repo_count=$(printf '%s\n' "$repos" | wc -l | tr -d ' ')

# --- list: 表を出す --------------------------------------------------------
list_collaborators() {
    local data rows failed=0
    data=$(mktemp) || exit 1
    echo "コラボレーターを集めている（$owner のリポジトリ $repo_count 件、ログイン: $me）..."
    while IFS= read -r repo; do
        printf '%s\t\t\n' "$repo" >> "$data"
        if rows=$(gh api --paginate "repos/$repo/collaborators?affiliation=direct&per_page=100" \
            --jq '.[] | "\(.login)\t\(.role_name)"' 2> /dev/null); then
            [ -z "$rows" ] || printf '%s\n' "$rows" | awk -v r="$repo" '{ print r "\t" $0 }' >> "$data"
        else
            printf '%s\t\terror\n' "$repo" >> "$data"
            failed=1
        fi
        if rows=$(gh api --paginate "repos/$repo/invitations?per_page=100" \
            --jq '.[] | "\(.invitee.login)\tinvited"' 2> /dev/null); then
            [ -z "$rows" ] || printf '%s\n' "$rows" | awk -v r="$repo" '{ print r "\t" $0 }' >> "$data"
        fi
    done <<< "$repos"
    echo

    # 行: リポジトリ、列: アカウント。所有者（$owner）は全リポジトリで admin なので列に出さない。
    awk -F '\t' -v owner="$owner" '
        # 表の中は ASCII だけにする（awk の種類や文字コードの設定で、日本語の幅がずれないように）
        function pad(s, w) { return sprintf("%-" w "s", s) }
        {
            if (!($1 in seen_repo)) { seen_repo[$1] = 1; repo_list[++repo_n] = $1 }
            if ($3 == "error") { if (!($1 in unreadable)) unreadable_n++; unreadable[$1] = 1; next }
            if ($2 == "" || tolower($2) == tolower(owner)) next
            if (!($2 in seen_user)) { seen_user[$2] = 1; user_list[++user_n] = $2 }
            cell[$1, $2] = $3
            if ($3 == "invited") pending[$2]++
            else joined[$2]++
        }
        END {
            if (user_n == 0) { print "所有者のほかにコラボレーターはいない。"; exit }
            # アカウントを名前順に並べる
            for (i = 1; i <= user_n; i++) for (j = i + 1; j <= user_n; j++)
                if (tolower(user_list[j]) < tolower(user_list[i])) { t = user_list[i]; user_list[i] = user_list[j]; user_list[j] = t }
            rw = length("repository")
            for (r = 1; r <= repo_n; r++) if (length(repo_list[r]) > rw) rw = length(repo_list[r])
            for (u = 1; u <= user_n; u++) { cw[u] = length(user_list[u]); if (cw[u] < 8) cw[u] = 8 }

            line = pad("repository", rw)
            for (u = 1; u <= user_n; u++) line = line "  " pad(user_list[u], cw[u])
            print line
            line = sprintf("%" rw "s", ""); gsub(/ /, "-", line)
            for (u = 1; u <= user_n; u++) { d = sprintf("%" cw[u] "s", ""); gsub(/ /, "-", d); line = line "  " d }
            print line
            for (r = 1; r <= repo_n; r++) {
                line = pad(repo_list[r], rw)
                for (u = 1; u <= user_n; u++) {
                    if (repo_list[r] in unreadable) c = "?"
                    else c = ((repo_list[r], user_list[u]) in cell) ? cell[repo_list[r], user_list[u]] : "-"
                    line = line "  " pad(c, cw[u])
                }
                print line
            }
            print ""
            print "admin / maintain / write / triage / read = 参加している（権限）、invited = 招待を受けていない、- = 入っていない、? = 読めなかった"
            print ""
            print "アカウントごとの数（全 " repo_n " 件）:"
            for (u = 1; u <= user_n; u++)
                printf "  %s  参加 %d / 招待中 %d / 入っていない %d\n", pad(user_list[u], cw[u]), joined[user_list[u]], pending[user_list[u]], repo_n - unreadable_n - joined[user_list[u]] - pending[user_list[u]]
        }
    ' "$data"
    rm -f "$data"
    if [ $failed -ne 0 ]; then
        echo
        echo "警告: 「?」のリポジトリはコラボレーターを読めなかった（管理者の権限が要る）。"
        return 1
    fi
    return 0
}

# --- invite: 全リポジトリへ招待する ---------------------------------------
invite_collaborator() {
    if [ -z "$target" ]; then
        read -r -p "招待する GitHub アカウント名: " target
    fi
    target=$(printf '%s' "$target" | tr -d '[:space:]' | sed 's/^@//')
    if [ -z "$target" ]; then
        echo "アカウント名が空なので中止した。"
        return 1
    fi
    if ! printf '%s' "$target" | grep -Eq '^[A-Za-z0-9]([A-Za-z0-9-]{0,37}[A-Za-z0-9])?$'; then
        echo "\"$target\" は GitHub のアカウント名として使えない文字を含む。"
        return 1
    fi
    if ! gh api "users/$target" --jq .login > /dev/null 2>&1; then
        echo "GitHub アカウント \"$target\" が見つからない。"
        return 1
    fi
    if [ "$(printf '%s' "$target" | tr '[:upper:]' '[:lower:]')" = "$(printf '%s' "$owner" | tr '[:upper:]' '[:lower:]')" ]; then
        echo "\"$target\" はリポジトリの所有者なので招待できない。"
        return 1
    fi

    echo
    echo "\"$target\" を、次の $repo_count 件のリポジトリへコラボレーターとして招待する:"
    printf '  %s\n' $repos
    echo
    echo "相手には GitHub からメールが届き、受け入れると書き込みができるようになる。"
    if [ $assume_yes -ne 1 ]; then
        read -r -p "招待する? [y/N] " answer
        case "$answer" in y|Y) ;; *) echo "中止した。"; return 0 ;; esac
    fi
    echo

    local invited=0 already=0 failed=0 status
    while IFS= read -r repo; do
        # 201 = 招待を作った（招待中の再実行も 201）、204 = すでにコラボレーター
        status=$(gh api -X PUT "repos/$repo/collaborators/$target" -f permission="$permission" \
            --include 2> /dev/null | head -n 1 | awk '{print $2}')
        case "$status" in
            201) echo "  招待した      $repo"; invited=$((invited + 1)) ;;
            204) echo "  すでに参加    $repo"; already=$((already + 1)) ;;
            *)   echo "  失敗(${status:-接続})  $repo"; failed=$((failed + 1)) ;;
        esac
    done <<< "$repos"
    echo
    echo "招待した $invited 件 / すでに参加 $already 件 / 失敗 $failed 件"
    [ $failed -eq 0 ] || { echo "失敗したものは、リポジトリの管理者の権限があるか確かめる。"; return 1; }
    return 0
}

case "$mode" in
    list) list_collaborators ;;
    invite) invite_collaborator ;;
esac
